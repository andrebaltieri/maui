﻿using System;
using Foundation;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class EntryHandler : ViewHandler<IEntry, MauiTextField>
	{
		protected override MauiTextField CreatePlatformView() =>
			new MauiTextField
			{
				BorderStyle = UITextBorderStyle.RoundedRect,
				ClipsToBounds = true
			};

		protected override void ConnectHandler(MauiTextField platformView)
		{
			platformView.ShouldReturn = OnShouldReturn;
			platformView.EditingDidBegin += OnEditingBegan;
			platformView.EditingChanged += OnEditingChanged;
			platformView.EditingDidBegin += OnEditingBegan;
			platformView.EditingDidEnd += OnEditingEnded;
			platformView.TextPropertySet += OnTextPropertySet;
			platformView.ShouldChangeCharacters += OnShouldChangeCharacters;
		}

		protected override void DisconnectHandler(MauiTextField platformView)
		{
			platformView.EditingDidBegin -= OnEditingBegan;
			platformView.EditingChanged -= OnEditingChanged;
			platformView.EditingDidBegin -= OnEditingBegan;
			platformView.EditingDidEnd -= OnEditingEnded;
			platformView.TextPropertySet -= OnTextPropertySet;
			platformView.ShouldChangeCharacters -= OnShouldChangeCharacters;
		}

		public static void MapText(IEntryHandler handler, IEntry entry)
		{
			handler.PlatformView?.UpdateText(entry);

			// Any text update requires that we update any attributed string formatting
			MapFormatting(handler, entry);
		}

		public static void MapTextColor(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateTextColor(entry);

		public static void MapIsPassword(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateIsPassword(entry);

		public static void MapHorizontalTextAlignment(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateHorizontalTextAlignment(entry);

		public static void MapVerticalTextAlignment(IEntryHandler handler, IEntry entry) =>
			handler?.PlatformView?.UpdateVerticalTextAlignment(entry);

		public static void MapIsTextPredictionEnabled(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateIsTextPredictionEnabled(entry);

		public static void MapMaxLength(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateMaxLength(entry);

		public static void MapPlaceholder(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdatePlaceholder(entry);

		public static void MapPlaceholderColor(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdatePlaceholder(entry);

		public static void MapIsReadOnly(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateIsReadOnly(entry);

		public static void MapKeyboard(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateKeyboard(entry);

		public static void MapReturnType(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateReturnType(entry);

		public static void MapFont(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateFont(entry, handler.GetRequiredService<IFontManager>());

		public static void MapCharacterSpacing(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateCharacterSpacing(entry);

		public static void MapCursorPosition(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateCursorPosition(entry);

		public static void MapSelectionLength(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateSelectionLength(entry);

		public static void MapClearButtonVisibility(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateClearButtonVisibility(entry);

		public static void MapFormatting(IEntryHandler handler, IEntry entry)
		{
			handler.PlatformView?.UpdateMaxLength(entry);

			// Update all of the attributed text formatting properties
			handler.PlatformView?.UpdateCharacterSpacing(entry);

			// Setting any of those may have removed text alignment settings,
			// so we need to make sure those are applied, too
			handler.PlatformView?.UpdateHorizontalTextAlignment(entry);
		}

		protected virtual bool OnShouldReturn(UITextField view)
		{
			view.ResignFirstResponder();

			// TODO: Focus next View

			VirtualView?.Completed();

			return false;
		}

		void OnEditingBegan(object? sender, EventArgs e)
		{
			if (VirtualView == null || PlatformView == null)
				return;

			PlatformView?.UpdateSelectionLength(VirtualView);

			VirtualView.IsFocused = true;
		}

		void OnEditingChanged(object? sender, EventArgs e) =>
			VirtualView.UpdateText(PlatformView.Text);

		void OnEditingEnded(object? sender, EventArgs e)
		{
			if (VirtualView == null || PlatformView == null)
				return;

			VirtualView.UpdateText(PlatformView.Text);
			VirtualView.IsFocused = false;
		}

		void OnTextPropertySet(object? sender, EventArgs e) =>
			VirtualView.UpdateText(PlatformView.Text);

		bool OnShouldChangeCharacters(UITextField textField, NSRange range, string replacementString) =>
			VirtualView.TextWithinMaxLength(textField.Text, range, replacementString);
	}
}