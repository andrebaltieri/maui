﻿#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using ElmSharp;
using Microsoft.Maui.Controls.Internals;
using Tizen.UIExtensions.ElmSharp;
using DPExtensions = Tizen.UIExtensions.ElmSharp.DPExtensions;

namespace Microsoft.Maui.Controls.Platform
{
	public class ShellFlyoutItemAdaptor : ItemAdaptor
	{
		Dictionary<EvasObject, View> _nativeFormsTable = new Dictionary<EvasObject, View>();
		Dictionary<object, View?> _dataBindedViewTable = new Dictionary<object, View?>();
		
		Shell _shell;
		View? _headerCache;
		IMauiContext _context;

		protected Shell Shell => _shell;

		public bool HasHeader { get; set; }

		protected virtual DataTemplate? DefaultItemTemplate => null;

		protected virtual DataTemplate? DefaultMenuItemTemplate => null;

		public ShellFlyoutItemAdaptor(Shell shell, IMauiContext context, IEnumerable items, bool hasHeader) : base(items)
		{
			_shell = shell;
			_context = context;
			HasHeader = hasHeader;
		}

		public override EvasObject? CreateNativeView(EvasObject parent)
		{
			return CreateNativeView(0, parent);
		}

		DataTemplate? GetDataTemplate(int index)
		{
			var item = this[index];
			if (item != null && item is BindableObject bo)
			{
				DataTemplate? dataTemplate = (Shell as IShellController)?.GetFlyoutItemDataTemplate(bo);
				if (item is IMenuItemController)
				{
					if (DefaultMenuItemTemplate != null && Shell.MenuItemTemplate == dataTemplate)
						dataTemplate = DefaultMenuItemTemplate;
				}
				else
				{
					if (DefaultItemTemplate != null && Shell.ItemTemplate == dataTemplate)
						dataTemplate = DefaultItemTemplate;
				}

				var template = dataTemplate.SelectDataTemplate(item, Shell);

				return template;
			}

			return null;
		}

		public override EvasObject? CreateNativeView(int index, EvasObject parent)
		{

			var template = GetDataTemplate(index);

			if (template != null)
			{
				var content = (View)template.CreateContent();
				var native = content.ToPlatform(_context);

				_nativeFormsTable[native] = content;
				return native;
			}

			return null;
		}

		public override EvasObject? GetFooterView(EvasObject parent)
		{
			return null;
		}

		public override EvasObject? GetHeaderView(EvasObject parent)
		{
			if (!HasHeader)
				return null;

			_headerCache = ((IShellController)Shell).FlyoutHeader;

			if (_headerCache != null)
			{
				var native = _headerCache.ToPlatform(_context);
				return native;
			}

			return null;
		}

		public override Size MeasureFooter(int widthConstraint, int heightConstraint)
		{
			return new Size(0, 0);
		}

		public override Size MeasureHeader(int widthConstraint, int heightConstraint)
		{
			return _headerCache?.Measure(DPExtensions.ConvertToScaledDP(widthConstraint), DPExtensions.ConvertToScaledDP(heightConstraint)).Request.ToEFLPixel() ?? new Size(0, 0);
		}

		public override Size MeasureItem(int widthConstraint, int heightConstraint)
		{
			return MeasureItem(0, widthConstraint, heightConstraint);
		}

		public override Size MeasureItem(int index, int widthConstraint, int heightConstraint)
		{
			var item = this[index];
			if (item != null && _dataBindedViewTable.TryGetValue(item, out View? createdView) && createdView != null)
			{
				return createdView.Measure(DPExtensions.ConvertToScaledDP(widthConstraint), DPExtensions.ConvertToScaledDP(heightConstraint), MeasureFlags.IncludeMargins).Request.ToEFLPixel();
			}

			return new Size(0, 0);
		}

		public override void RemoveNativeView(EvasObject native)
		{
			native?.Unrealize();
		}

		public override void SetBinding(EvasObject native, int index)
		{
			if (_nativeFormsTable.TryGetValue(native, out View? view))
			{
				ResetBindedView(view);
				var item = this[index];
				if (item != null)
				{
					view.BindingContext = item;
					_dataBindedViewTable[item] = view;
				}

				view.MeasureInvalidated += OnItemMeasureInvalidated;
				Shell.AddLogicalChild(view);
			}
		}

		public override void UnBinding(EvasObject native)
		{
			if (_nativeFormsTable.TryGetValue(native, out View? view))
			{
				view.MeasureInvalidated -= OnItemMeasureInvalidated;
				ResetBindedView(view);
			}
		}

		void ResetBindedView(View view)
		{
			if (view.BindingContext != null && _dataBindedViewTable.ContainsKey(view.BindingContext))
			{
				_dataBindedViewTable[view.BindingContext] = null;
				Shell.RemoveLogicalChild(view);
				view.BindingContext = null;
			}
		}

		void OnItemMeasureInvalidated(object? sender, EventArgs e)
		{
			var data = (sender as View)?.BindingContext ?? null;
			if (data != null)
			{
				int index = GetItemIndex(data);
				if (index != -1)
				{
					CollectionView?.ItemMeasureInvalidated(index);
				}
			}
		}
	}
}
