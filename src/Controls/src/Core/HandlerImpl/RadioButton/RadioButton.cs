﻿using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../../docs/Microsoft.Maui.Controls/RadioButton.xml" path="Type[@FullName='Microsoft.Maui.Controls.RadioButton']/Docs" />
	public partial class RadioButton
	{
		IMauiContext MauiContext => Handler?.MauiContext ?? throw new InvalidOperationException("MauiContext not set");

		/// <include file="../../../../docs/Microsoft.Maui.Controls/RadioButton.xml" path="//Member[@MemberName='ControlsRadioButtonMapper']/Docs" />
		public static IPropertyMapper<RadioButton, RadioButtonHandler> ControlsRadioButtonMapper =
			   new PropertyMapper<RadioButton, RadioButtonHandler>(RadioButtonHandler.Mapper)
			   {
#if IOS || ANDROID
				   [nameof(IRadioButton.Content)] = MapContent
#endif
			   };

		internal new static void RemapForControls()
		{
			RadioButtonHandler.Mapper = ControlsRadioButtonMapper;

#if ANDROID
			RadioButtonHandler.PlatformViewFactory = CreatePlatformView;
#endif
		}
	}
}
