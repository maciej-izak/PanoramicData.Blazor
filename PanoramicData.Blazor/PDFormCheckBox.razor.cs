﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PanoramicData.Blazor.Extensions;

namespace PanoramicData.Blazor
{
	public partial class PDFormCheckBox
	{
		[Parameter] public bool Value { get; set; }

		[Parameter] public bool Disabled { get; set; }

		[Parameter] public EventCallback<bool> ValueChanged { get; set; }

		private void OnClick()
		{
			if (!Disabled)
			{
				ToggleValue();
			}
		}

		private void OnKeyPress(KeyboardEventArgs args)
		{
			if (!Disabled && args.Code.In("Space", "Enter"))
			{
				ToggleValue();
			}
		}

		private void ToggleValue()
		{
			Value = !Value;
			ValueChanged.InvokeAsync(Value);
		}
	}
}
