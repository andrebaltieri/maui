﻿#nullable enable
#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.MauiTextView;
#elif MONOANDROID
using PlatformView = AndroidX.AppCompat.Widget.AppCompatEditText;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.TextBox;
#elif TIZEN
using PlatformView = Tizen.UIExtensions.ElmSharp.Entry;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler : IEditorHandler
	{
		public static IPropertyMapper<IEditor, IEditorHandler> Mapper = new PropertyMapper<IEditor, IEditorHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IEditor.Background)] = MapBackground,
			[nameof(IEditor.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(IEditor.Font)] = MapFont,
			[nameof(IEditor.IsReadOnly)] = MapIsReadOnly,
			[nameof(IEditor.IsTextPredictionEnabled)] = MapIsTextPredictionEnabled,
			[nameof(IEditor.MaxLength)] = MapMaxLength,
			[nameof(IEditor.Placeholder)] = MapPlaceholder,
			[nameof(IEditor.PlaceholderColor)] = MapPlaceholderColor,
			[nameof(IEditor.Text)] = MapText,
			[nameof(IEditor.TextColor)] = MapTextColor,
			[nameof(IEditor.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
			[nameof(IEditor.VerticalTextAlignment)] = MapVerticalTextAlignment,
			[nameof(IEditor.Keyboard)] = MapKeyboard,
			[nameof(IEditor.CursorPosition)] = MapCursorPosition,
			[nameof(IEditor.SelectionLength)] = MapSelectionLength
		};

		public static CommandMapper<IPicker, IEditorHandler> CommandMapper = new(ViewCommandMapper)
		{
		};

		public EditorHandler() : base(Mapper)
		{
		}

		public EditorHandler(IPropertyMapper? mapper = null) : base(mapper ?? Mapper)
		{
		}

		IEditor IEditorHandler.VirtualView => VirtualView;

		PlatformView IEditorHandler.PlatformView => PlatformView;
	}
}
