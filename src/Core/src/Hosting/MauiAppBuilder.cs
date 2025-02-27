﻿using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui.Devices;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.Hosting
{
	/// <summary>
	/// A builder for .NET MAUI cross-platform applications and services.
	/// </summary>
	public sealed class MauiAppBuilder
	{
		private readonly MauiApplicationServiceCollection _services = new();
		private Func<IServiceProvider>? _createServiceProvider;
		private readonly Lazy<ConfigurationManager> _configuration;
		private ILoggingBuilder? _logging;

		internal MauiAppBuilder(bool useDefaults)
		{
			// Lazy-load the ConfigurationManager, so it isn't created if it is never used.
			// Don't capture the 'this' variable in AddSingleton, so MauiAppBuilder can be GC'd.
			var configuration = new Lazy<ConfigurationManager>(() => new ConfigurationManager());
			Services.AddSingleton<IConfiguration>(sp => configuration.Value);

			_configuration = configuration;

			if (useDefaults)
			{
				// Register required services
				this.ConfigureMauiHandlers(configureDelegate: null);

				this.ConfigureFonts();
				this.ConfigureImageSources();
				this.ConfigureAnimations();
				this.ConfigureCrossPlatformLifecycleEvents();
				this.ConfigureDispatching();

				this.UseEssentials();

#if WINDOWS
				this.Services.TryAddEnumerable(ServiceDescriptor.Transient<IMauiInitializeService, MauiCoreInitializer>());
#endif
			}
		}

		class MauiCoreInitializer : IMauiInitializeService
		{
			public void Initialize(IServiceProvider services)
			{
#if WINDOWS
				var dispatcher = 
					services.GetService<IDispatcher>() ??
					MauiWinUIApplication.Current.Services.GetRequiredService<IDispatcher>();

				if (!dispatcher.IsDispatchRequired)
					SetupResources();
				else
					dispatcher.Dispatch(SetupResources);

				void SetupResources()
				{
					var dictionaries = UI.Xaml.Application.Current?.Resources?.MergedDictionaries;
					if (UI.Xaml.Application.Current?.Resources != null && dictionaries != null)
					{
						// WinUI
						UI.Xaml.Application.Current.Resources.AddLibraryResources<UI.Xaml.Controls.XamlControlsResources>();

						// Microsoft.Maui
						UI.Xaml.Application.Current.Resources.AddLibraryResources("MicrosoftMauiCoreIncluded", "ms-appx:///Microsoft.Maui/Platform/Windows/Styles/Resources.xbf");
					}
				}
#endif
			}
		}

		/// <summary>
		/// A collection of services for the application to compose. This is useful for adding user provided or framework provided services.
		/// </summary>
		public IServiceCollection Services => _services;

		/// <summary>
		/// A collection of configuration providers for the application to compose. This is useful for adding new configuration sources and providers.
		/// </summary>
		public ConfigurationManager Configuration => _configuration.Value;

		/// <summary>
		/// A collection of logging providers for the application to compose. This is useful for adding new logging providers.
		/// </summary>
		public ILoggingBuilder Logging
		{
			get
			{
				return _logging ??= InitializeLogging();

				ILoggingBuilder InitializeLogging()
				{
					// if someone accesses the Logging builder, ensure Logging has been initialized.
					Services.AddLogging();
					return new LoggingBuilder(Services);
				}
			}
		}

		/// <summary>
		/// Registers a <see cref="IServiceProviderFactory{TBuilder}" /> instance to be used to create the <see cref="IServiceProvider" />.
		/// </summary>
		/// <param name="factory">The <see cref="IServiceProviderFactory{TBuilder}" />.</param>
		/// <param name="configure">
		/// A delegate used to configure the <typeparamref T="TBuilder" />. This can be used to configure services using
		/// APIS specific to the <see cref="IServiceProviderFactory{TBuilder}" /> implementation.
		/// </param>
		/// <typeparam name="TBuilder">The type of builder provided by the <see cref="IServiceProviderFactory{TBuilder}" />.</typeparam>
		/// <remarks>
		/// <para>
		/// <see cref="ConfigureContainer{TBuilder}(IServiceProviderFactory{TBuilder}, Action{TBuilder})"/> is called by <see cref="Build"/>
		/// and so the delegate provided by <paramref name="configure"/> will run after all other services have been registered.
		/// </para>
		/// <para>
		/// Multiple calls to <see cref="ConfigureContainer{TBuilder}(IServiceProviderFactory{TBuilder}, Action{TBuilder})"/> will replace
		/// the previously stored <paramref name="factory"/> and <paramref name="configure"/> delegate.
		/// </para>
		/// </remarks>
		public void ConfigureContainer<TBuilder>(IServiceProviderFactory<TBuilder> factory, Action<TBuilder>? configure = null) where TBuilder : notnull
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			_createServiceProvider = () =>
			{
				var container = factory.CreateBuilder(Services);
				configure?.Invoke(container);
				return factory.CreateServiceProvider(container);
			};
		}

		/// <summary>
		/// Builds the <see cref="MauiApp"/>.
		/// </summary>
		/// <returns>A configured <see cref="MauiApp"/>.</returns>
		public MauiApp Build()
		{
			ConfigureDefaultLogging();

			IServiceProvider serviceProvider = _createServiceProvider != null
				? _createServiceProvider()
				: _services.BuildServiceProvider();

			MauiApp builtApplication = new MauiApp(serviceProvider);

			// Mark the service collection as read-only to prevent future modifications
			_services.IsReadOnly = true;

			var initServices = builtApplication.Services.GetServices<IMauiInitializeService>();
			if (initServices != null)
			{
				foreach (var instance in initServices)
				{
					instance.Initialize(builtApplication.Services);
				}
			}

			return builtApplication;
		}

		private sealed class LoggingBuilder : ILoggingBuilder
		{
			public LoggingBuilder(IServiceCollection services)
			{
				Services = services;
			}

			public IServiceCollection Services { get; }
		}

		private void ConfigureDefaultLogging()
		{
			// By default, if no one else has configured logging, add a "no-op" LoggerFactory
			// and Logger services with no providers. This way when components try to get an
			// ILogger<> from the IServiceProvider, they don't get 'null'.
			Services.TryAdd(ServiceDescriptor.Singleton<ILoggerFactory, NullLoggerFactory>());
			Services.TryAdd(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(NullLogger<>)));
		}

		private sealed class NullLoggerFactory : ILoggerFactory
		{
			public void AddProvider(ILoggerProvider provider) { }

			public ILogger CreateLogger(string categoryName) => NullLogger.Instance;

			public void Dispose() { }
		}

		private sealed class NullLogger<T> : ILogger<T>, IDisposable
		{
			public IDisposable BeginScope<TState>(TState state) => this;

			public void Dispose() { }

			public bool IsEnabled(LogLevel logLevel) => false;

			public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
			{
			}
		}
	}
}
