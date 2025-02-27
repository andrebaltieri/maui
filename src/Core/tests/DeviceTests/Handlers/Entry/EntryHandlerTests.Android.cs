using System.Linq;
using System.Threading.Tasks;
using Android.Text;
using Android.Text.Method;
using Android.Views.InputMethods;
using AndroidX.AppCompat.Widget;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;
using AColor = Android.Graphics.Color;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EntryHandlerTests
	{
		[Fact(DisplayName = "PlaceholderColor Initializes Correctly")]
		public async Task PlaceholderColorInitializesCorrectly()
		{
			var entry = new EntryStub()
			{
				Placeholder = "Test",
				PlaceholderColor = Colors.Yellow
			};

			await ValidatePropertyInitValue(entry, () => entry.PlaceholderColor, GetNativePlaceholderColor, entry.PlaceholderColor);
		}

		[Fact(DisplayName = "ReturnType Initializes Correctly")]
		public async Task ReturnTypeInitializesCorrectly()
		{
			var xplatReturnType = ReturnType.Next;
			var entry = new EntryStub()
			{
				Text = "Test",
				ReturnType = xplatReturnType
			};

			ImeAction expectedValue = ImeAction.Next;

			var values = await GetValueAsync(entry, (handler) =>
			{
				return new
				{
					ViewValue = entry.ReturnType,
					PlatformViewValue = GetNativeReturnType(handler)
				};
			});

			Assert.Equal(xplatReturnType, values.ViewValue);
			Assert.Equal(expectedValue, values.PlatformViewValue);
		}

		[Fact(DisplayName = "Horizontal TextAlignment Initializes Correctly")]
		public async Task HorizontalTextAlignmentInitializesCorrectly()
		{
			var xplatHorizontalTextAlignment = TextAlignment.End;

			var entry = new EntryStub()
			{
				Text = "Test",
				HorizontalTextAlignment = xplatHorizontalTextAlignment
			};

			Android.Views.TextAlignment expectedValue = Android.Views.TextAlignment.ViewEnd;

			var values = await GetValueAsync(entry, (handler) =>
			{
				return new
				{
					ViewValue = entry.HorizontalTextAlignment,
					PlatformViewValue = GetNativeHorizontalTextAlignment(handler)
				};
			});

			Assert.Equal(xplatHorizontalTextAlignment, values.ViewValue);
			values.PlatformViewValue.AssertHasFlag(expectedValue);
		}

		[Fact(DisplayName = "Vertical TextAlignment Initializes Correctly")]
		public async Task VerticalTextAlignmentInitializesCorrectily()
		{
			var xplatVerticalTextAlignment = TextAlignment.End;

			var entry = new EntryStub
			{
				Text = "Test",
				VerticalTextAlignment = xplatVerticalTextAlignment
			};

			Android.Views.GravityFlags expectedValue = Android.Views.GravityFlags.Bottom;

			var values = await GetValueAsync(entry, (handler) =>
			{
				return new
				{
					ViewValue = entry.VerticalTextAlignment,
					PlatformViewValue = GetNativeVerticalTextAlignment(handler)
				};
			});

			Assert.Equal(xplatVerticalTextAlignment, values.ViewValue);
			values.PlatformViewValue.AssertHasFlag(expectedValue);
		}

		static AppCompatEditText GetNativeEntry(EntryHandler entryHandler) =>
			entryHandler.PlatformView;

		string GetNativeText(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).Text;

		static void SetNativeText(EntryHandler entryHandler, string text) =>
			GetNativeEntry(entryHandler).Text = text;

		static int GetCursorStartPosition(EntryHandler entryHandler)
		{
			var control = GetNativeEntry(entryHandler);
			return control.SelectionStart;
		}

		static void UpdateCursorStartPosition(EntryHandler entryHandler, int position)
		{
			var control = GetNativeEntry(entryHandler);
			control.SetSelection(position);
		}

		Color GetNativeTextColor(EntryHandler entryHandler)
		{
			int currentTextColorInt = GetNativeEntry(entryHandler).CurrentTextColor;
			AColor currentTextColor = new AColor(currentTextColorInt);
			return currentTextColor.ToColor();
		}

		bool GetNativeIsPassword(EntryHandler entryHandler)
		{
			var inputType = GetNativeEntry(entryHandler).InputType;
			return inputType.HasFlag(InputTypes.TextVariationPassword) || inputType.HasFlag(InputTypes.NumberVariationPassword);
		}

		bool GetNativeIsTextPredictionEnabled(EntryHandler entryHandler) =>
			!GetNativeEntry(entryHandler).InputType.HasFlag(InputTypes.TextFlagNoSuggestions);

		string GetNativePlaceholder(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).Hint;

		Color GetNativePlaceholderColor(EntryHandler entryHandler)
		{
			int currentHintTextColor = GetNativeEntry(entryHandler).CurrentHintTextColor;
			AColor currentPlaceholderColor = new AColor(currentHintTextColor);
			return currentPlaceholderColor.ToColor();
		}

		bool GetNativeIsReadOnly(EntryHandler entryHandler)
		{
			var editText = GetNativeEntry(entryHandler);

			return !editText.Focusable && !editText.FocusableInTouchMode;
		}

		bool GetNativeIsNumericKeyboard(EntryHandler entryHandler)
		{
			var editText = GetNativeEntry(entryHandler);
			var inputTypes = editText.InputType;

			return editText.KeyListener is NumberKeyListener
				&& (inputTypes.HasFlag(InputTypes.NumberFlagDecimal) && inputTypes.HasFlag(InputTypes.ClassNumber) && inputTypes.HasFlag(InputTypes.NumberFlagSigned));
		}

		bool GetNativeIsChatKeyboard(EntryHandler entryHandler)
		{
			var editText = GetNativeEntry(entryHandler);
			var inputTypes = editText.InputType;

			return inputTypes.HasFlag(InputTypes.ClassText) && inputTypes.HasFlag(InputTypes.TextFlagCapSentences) && inputTypes.HasFlag(InputTypes.TextFlagNoSuggestions);
		}

		bool GetNativeIsEmailKeyboard(EntryHandler entryHandler)
		{
			var editText = GetNativeEntry(entryHandler);
			var inputTypes = editText.InputType;

			return (inputTypes.HasFlag(InputTypes.ClassText) && inputTypes.HasFlag(InputTypes.TextVariationEmailAddress));
		}

		bool GetNativeIsTelephoneKeyboard(EntryHandler entryHandler)
		{
			var editText = GetNativeEntry(entryHandler);
			var inputTypes = editText.InputType;

			return inputTypes.HasFlag(InputTypes.ClassPhone);
		}

		bool GetNativeIsUrlKeyboard(EntryHandler entryHandler)
		{
			var editText = GetNativeEntry(entryHandler);
			var inputTypes = editText.InputType;

			return inputTypes.HasFlag(InputTypes.ClassText) && inputTypes.HasFlag(InputTypes.TextVariationUri);
		}

		bool GetNativeIsTextKeyboard(EntryHandler entryHandler)
		{
			var editText = GetNativeEntry(entryHandler);
			var inputTypes = editText.InputType;

			return inputTypes.HasFlag(InputTypes.ClassText) && inputTypes.HasFlag(InputTypes.TextFlagCapSentences) && !inputTypes.HasFlag(InputTypes.TextFlagNoSuggestions);
		}

		Android.Views.TextAlignment GetNativeHorizontalTextAlignment(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).TextAlignment;

		Android.Views.GravityFlags GetNativeVerticalTextAlignment(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).Gravity;

		ImeAction GetNativeReturnType(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).ImeOptions;

		bool GetNativeClearButtonVisibility(EntryHandler entryHandler)
		{
			var nativeEntry = GetNativeEntry(entryHandler);
			var unfocusedDrawables = nativeEntry.GetCompoundDrawables();

			bool compoundsValidWhenUnfocused = !unfocusedDrawables.Any(a => a != null);

			// This will display 'X' drawable.
			nativeEntry.RequestFocus();

			var focusedDrawables = nativeEntry.GetCompoundDrawables();

			// Index 2 for FlowDirection.LeftToRight.
			bool compoundsValidWhenFocused = focusedDrawables.Length == 4 && focusedDrawables[2] != null;

			return compoundsValidWhenFocused && compoundsValidWhenUnfocused;
		}

		[Fact(DisplayName = "CharacterSpacing Initializes Correctly")]
		public async Task CharacterSpacingInitializesCorrectly()
		{
			var xplatCharacterSpacing = 4;

			var entry = new EntryStub()
			{
				CharacterSpacing = xplatCharacterSpacing,
				Text = "Some Test Text"
			};

			float expectedValue = entry.CharacterSpacing.ToEm();

			var values = await GetValueAsync(entry, (handler) =>
			{
				return new
				{
					ViewValue = entry.CharacterSpacing,
					PlatformViewValue = GetNativeCharacterSpacing(handler)
				};
			});

			Assert.Equal(xplatCharacterSpacing, values.ViewValue);
			Assert.Equal(expectedValue, values.PlatformViewValue, EmCoefficientPrecision);
		}

		double GetNativeCharacterSpacing(EntryHandler entryHandler)
		{
			var editText = GetNativeEntry(entryHandler);

			if (editText != null)
			{
				return editText.LetterSpacing;
			}

			return -1;
		}

		int GetNativeCursorPosition(EntryHandler entryHandler)
		{
			var editText = GetNativeEntry(entryHandler);

			if (editText != null)
				return editText.SelectionEnd;

			return -1;
		}

		int GetPlatformSelectionLength(EntryHandler entryHandler)
		{
			var editText = GetNativeEntry(entryHandler);

			if (editText != null)
				return editText.SelectionEnd - editText.SelectionStart;

			return -1;
		}
	}
}
