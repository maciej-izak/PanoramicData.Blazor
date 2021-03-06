﻿using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PanoramicData.Blazor.Demo.Data;
using PanoramicData.Blazor.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PanoramicData.Blazor.Demo.Pages
{
	public partial class PDFileExplorerPage
	{
		private readonly IDataProviderService<FileExplorerItem> _dataProvider = new TestFileSystemDataProvider();
		private PDFileExplorer? FileExplorer { get; set; } = null!;

		/// <summary>
		/// Injected javascript interop object.
		/// </summary>
		[Inject] public IJSRuntime JSRuntime { get; set; } = null!;

		/// <summary>
		/// Injected navigation manager.
		/// </summary>
		[Inject] protected NavigationManager NavigationManager { get; set; } = null!;

		[CascadingParameter] protected EventManager? EventManager { get; set; }

		public async Task OnTableDownloadRequest(TableSelectionEventArgs<FileExplorerItem> args)
		{
			// Method A: this method works up to file sizes of 125MB - limit imposed by System.Text.Json (04/08/20)
			//var bytes = System.IO.File.ReadAllBytes("Download/file_example_WEBM_1920_3_7MB.webm");
			//var base64 = System.Convert.ToBase64String(bytes);
			//await JSRuntime.InvokeVoidAsync("downloadFile", $"{System.IO.Path.GetFileNameWithoutExtension(args.Item.Name)}.webm", base64).ConfigureAwait(true);

			// Method B: to avoid size limit and conversion to base64 - use javascript to get from controller method

			// demo downloads the same WEBM file
			foreach (var item in args.Items)
			{
				item.Name = System.IO.Path.ChangeExtension(item.Name, ".webm");
			}
			await JSRuntime.InvokeVoidAsync("panoramicDataDemo.downloadFiles", args).ConfigureAwait(false);
		}

		public async Task OnUploadRequest(DropZoneEventArgs args)
		{
			// example of canceling an upload request
			if (args.Files.Any(x => System.IO.Path.GetExtension(x.Name) == ".zip"))
			{
				args.Cancel = true;
				args.CancelReason = "ZIP Archive files can not be uploaded.";
				//await JSRuntime.InvokeVoidAsync("panoramicData.alert", new[] { "Upload canceled: Limit is 10MB per file" }).ConfigureAwait(true);
			}
		}

		public void OnUploadStarted(DropZoneUploadEventArgs args)
		{
			// add example additional field to pass with upload
			args.FormFields.Add("sessionId", FileExplorer.SessionId);
		}

		public async Task OnUploadCompleted(DropZoneUploadCompletedEventArgs args)
		{
			// need to add to data provider as file not really uploaded to physical drive
			if (args.Success)
			{
				await _dataProvider.CreateAsync(new FileExplorerItem
				{
					DateCreated = DateTimeOffset.Now,
					DateModified = DateTimeOffset.Now,
					EntryType = FileExplorerItemType.File,
					FileSize = args.Size,
					Path = $"{args.Path.TrimEnd('/')}/{args.Name}"
				}, CancellationToken.None).ConfigureAwait(true);
			}
		}

		public void OnUpdateToolbarState(List<ToolbarItem> items)
		{
			// add custom toolbar button - if not already created
			var createFileButton = items.Find(x => x.Key == "create-file");
			if (createFileButton == null)
			{
				items.Insert(3, new ToolbarButton { Key = "create-file", Text = "New File", ToolTip = "Create a new file", CssClass = "btn-secondary", IconCssClass = "fas fa-fw fa-file-medical" });
			}
		}

		public async Task OnToolbarClick(string key)
		{
			// example of a custom action
			if (key == "create-file")
			{
				await CreateFile().ConfigureAwait(true);
			}
		}

		public void OnUpdateTableContextMenuState(MenuItemsEventArgs args)
		{
			// add custom toolbar button
			var createFileButton = args.MenuItems.Find(x => x.Key == "create-file");
			if (createFileButton == null)
			{
				// not existing - so create
				createFileButton = new MenuItem { Key = "create-file", Text = "New File", IconCssClass = "fas fa-fw fa-file-medical" };
				args.MenuItems.Insert(2, createFileButton);
			}

			// update custom item state - enabled only when no selection
			if (FileExplorer?.SelectedFilesAndFolders.Length == 0)
			{
				createFileButton.IsDisabled = false;
				args.Cancel = false;
			}
			else
			{
				createFileButton.IsDisabled = true;
			}
		}

		public async Task OnTableContextMenuClick(MenuItemEventArgs args)
		{
			if (args.MenuItem.Key == "create-file")
			{
				await CreateFile().ConfigureAwait(true);
			}
		}

		private async Task CreateFile()
		{
			var newPath = $"{FileExplorer?.FolderPath}/{DateTime.Now:yyyyMMdd_HHmmss_fff}.txt";

			var result = await _dataProvider.CreateAsync(new FileExplorerItem
			{
				EntryType = FileExplorerItemType.File,
				Path = newPath,
				FileSize = 100,
				DateCreated = DateTimeOffset.UtcNow,
				DateModified = DateTimeOffset.UtcNow
			}, CancellationToken.None).ConfigureAwait(true);

			if (result.Success && FileExplorer != null)
			{
				await FileExplorer.RefreshTableAsync().ConfigureAwait(true);
				await FileExplorer.RefreshToolbarAsync().ConfigureAwait(true);
			}
		}

		private string GetIconCssClass(FileExplorerItem item)
		{
			if (item.EntryType == FileExplorerItemType.Directory)
			{
				if (item.Path == "/")
				{
					return "fas fa-server";
				}
				if (item.ParentPath == "/")
				{
					return "fas fa-hdd";
				}
			}
			return TestFileSystemDataProvider.GetIconClass(item);
		}
	}
}
