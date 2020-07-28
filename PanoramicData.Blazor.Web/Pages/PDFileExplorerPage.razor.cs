﻿using System.Threading.Tasks;
using PanoramicData.Blazor.Services;
using PanoramicData.Blazor.Web.Data;

namespace PanoramicData.Blazor.Web.Pages
{
	public partial class PDFileExplorerPage
    {
		private IDataProviderService<FileExplorerItem> _dataProvider = new TestFileSystemDataProvider { RootFolder = "My Computer" };

		public async Task OnTreeContextMenuClick(MenuItemEventArgs args)
		{
			//var tree = (PDTree<FileExplorerItem>)args.Sender;
			if (args.MenuItem.Text == "Delete")
			{
				// prompt user to confirm action
			}
		}

		public void OnTableContextMenuClick(MenuItemEventArgs args)
		{
			if (args.MenuItem.Text == "Delete")
			{
				// prompt user to confirm action
			}
		}
	}
}
