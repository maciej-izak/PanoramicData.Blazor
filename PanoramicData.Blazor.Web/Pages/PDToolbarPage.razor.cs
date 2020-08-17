﻿using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PanoramicData.Blazor.Web.Pages
{
    public partial class PDToolbarPage
    {
		private string _events = string.Empty;
		private bool _showButtons = true;
		private bool _enableButtons = true;
		private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>
		{
			new ToolbarButton { Key = "tb-open", Text = "Open", IconCssClass = "fas fa-folder-open", ToolTip="Open something" },
			new ToolbarButton { Key = "tb-rename", Text="Rename", ToolTip="Rename something" },
			new ToolbarSeparator(),
			new ToolbarButton { Key = "tb-download", Text="Download", IconCssClass="fas fa-file-download", ToolTip="Download something" },
			new ToolbarButton { Key = "tb-enabledisable", Text="Disable Buttons", ToolTip="Enable or disable buttons", ShiftRight = true },
			new ToolbarButton { Key = "tb-showhide", Text="Hide Buttons", ToolTip="Show or hide buttons" }
		};

		public void OnOpen()
		{
			_events += $"Open Button Clicked{Environment.NewLine}";
		}

		public void OnRename()
		{
			_events += $"Rename Button Clicked{Environment.NewLine}";
		}

		public void OnDownload()
		{
			_events += $"Download Button Clicked{Environment.NewLine}";
		}

		public void OnButtonClick(string key)
		{
			switch(key)
			{
				case "tb-enabledisable":
					{
						ToolbarItems[0].IsEnabled = !ToolbarItems[0].IsEnabled;
						ToolbarItems[1].IsEnabled = !ToolbarItems[1].IsEnabled;
						ToolbarItems[3].IsEnabled = !ToolbarItems[3].IsEnabled;
						var button = ToolbarItems[4] as ToolbarButton;
						if (button.Text.StartsWith("Disable"))
						{
							button.Text = "Enable buttons";
						}
						else
						{
							button.Text = "Disable buttons";
						}
					}
					break;

				case "tb-showhide":
					{
						ToolbarItems[0].IsVisible = !ToolbarItems[0].IsVisible;
						ToolbarItems[1].IsVisible = !ToolbarItems[1].IsVisible;
						ToolbarItems[2].IsVisible = !ToolbarItems[2].IsVisible;
						ToolbarItems[3].IsVisible = !ToolbarItems[3].IsVisible;
						var button = ToolbarItems[5] as ToolbarButton;
						if (button.Text.StartsWith("Show"))
						{
							button.Text = "Hide buttons";
						}
						else
						{
							button.Text = "Show buttons";
						}
					}
					break;

				default:
					_events += $"Button {key} Clicked{Environment.NewLine}";
					break;
			}
		}
	}
}
