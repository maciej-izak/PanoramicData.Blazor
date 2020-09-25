﻿using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using PanoramicData.Blazor.Services;
using PanoramicData.Blazor.Extensions;
using System.Security.Cryptography.X509Certificates;

namespace PanoramicData.Blazor.Web.Data
{
	public class TestFileSystemDataProvider : IDataProviderService<FileExplorerItem>
	{
		private readonly List<FileExplorerItem> _testData = new List<FileExplorerItem>();

		/// <summary>
		/// Gets or sets the root folder. All operations can only be performed under this folder.
		/// </summary>
		public string RootFolder { get; set; } = @"/My Computer";

		public TestFileSystemDataProvider()
		{
			// generate test data - Paths can not edit with trailing slashes
			_testData.Add(new FileExplorerItem { Path = @"/My Computer", CanCopyMove = false });
			_testData.Add(new FileExplorerItem { Path = @"/My Computer/C:", CanCopyMove = false });
			_testData.Add(new FileExplorerItem { Path = @"/My Computer/C:/ProgramData" });
			_testData.Add(new FileExplorerItem { Path = @"/My Computer/C:/ProgramData/Acme" });
			_testData.Add(new FileExplorerItem { Path = @"/My Computer/C:/ProgramData/Acme/Readme.txt", EntryType = FileExplorerItemType.File, DateModified = DateTimeOffset.UtcNow });
			_testData.Add(new FileExplorerItem { Path = @"/My Computer/C:/ProgramData/Acme/UserGuide.pdf", EntryType = FileExplorerItemType.File, DateModified = DateTimeOffset.UtcNow });
			_testData.Add(new FileExplorerItem { Path = @"/My Computer/C:/ProgramData/stats.txt", EntryType = FileExplorerItemType.File, DateModified = DateTimeOffset.UtcNow });
			_testData.Add(new FileExplorerItem { Path = @"/My Computer/C:/Temp" });
			_testData.Add(new FileExplorerItem { Path = @"/My Computer/C:/Temp/a53fde.tmp", EntryType = FileExplorerItemType.File, IsHidden = true, DateModified = DateTimeOffset.UtcNow });
			_testData.Add(new FileExplorerItem { Path = @"/My Computer/C:/Temp/b76jba.tmp", EntryType = FileExplorerItemType.File, IsHidden = true, DateModified = DateTimeOffset.UtcNow });
			_testData.Add(new FileExplorerItem { Path = @"/My Computer/C:/Temp/p21wsa.tmp", EntryType = FileExplorerItemType.File, IsHidden = true, DateModified = DateTimeOffset.UtcNow });
			_testData.Add(new FileExplorerItem { Path = @"/My Computer/C:/Users" });
			_testData.Add(new FileExplorerItem { Path = @"/My Computer/D:", CanCopyMove = false });
			_testData.Add(new FileExplorerItem { Path = @"/My Computer/D:/Data" });
			_testData.Add(new FileExplorerItem { Path = @"/My Computer/D:/Data/Backup" });
			_testData.Add(new FileExplorerItem { Path = @"/My Computer/D:/Data/Backup/20200131_mydb.bak", EntryType = FileExplorerItemType.File, DateModified = DateTimeOffset.UtcNow });
			_testData.Add(new FileExplorerItem { Path = @"/My Computer/D:/Data/Backup/20200229_mydb.bak", EntryType = FileExplorerItemType.File, DateModified = DateTimeOffset.UtcNow });
			_testData.Add(new FileExplorerItem { Path = @"/My Computer/D:/Data/Backup/20200331_mydb.bak", EntryType = FileExplorerItemType.File, DateModified = DateTimeOffset.UtcNow });
			_testData.Add(new FileExplorerItem { Path = @"/My Computer/D:/Data/Backup/20200430_mydb.bak", EntryType = FileExplorerItemType.File, DateModified = DateTimeOffset.UtcNow });
			_testData.Add(new FileExplorerItem { Path = @"/My Computer/D:/Data/WeeklyStats.json", EntryType = FileExplorerItemType.File, DateModified = DateTimeOffset.UtcNow });
			_testData.Add(new FileExplorerItem { Path = @"/My Computer/D:/Data/MonthlyStats.json", EntryType = FileExplorerItemType.File, DateModified = DateTimeOffset.UtcNow });
			_testData.Add(new FileExplorerItem { Path = @"/My Computer/D:/Logs" });
			_testData.Add(new FileExplorerItem { Path = @"/My Computer/D:/Logs/20200430_agent.log", EntryType = FileExplorerItemType.File, DateModified = DateTimeOffset.UtcNow });
			_testData.Add(new FileExplorerItem { Path = @"/My Computer/D:/Logs/20200501_agent.log", EntryType = FileExplorerItemType.File, DateModified = DateTimeOffset.UtcNow });
			_testData.Add(new FileExplorerItem { Path = @"/My Computer/D:/Logs/20200502_agent.log", EntryType = FileExplorerItemType.File, DateModified = DateTimeOffset.UtcNow });
			_testData.Add(new FileExplorerItem { Path = @"/My Computer/D:/Readme.txt", EntryType = FileExplorerItemType.File, DateModified = DateTimeOffset.UtcNow });
		}

		/// <summary>
		/// Sends details of a query to be performed on the underlying data set and returns the results.
		/// </summary>
		/// <param name="request">Details of the query to be performed.</param>
		/// <param name="cancellationToken">A cancellation token for the async operation.</param>
		/// <returns>A new DataResponse instance containing the result of the query.</returns>
		public async Task<DataResponse<FileExplorerItem>> GetDataAsync(DataRequest<FileExplorerItem> request, CancellationToken cancellationToken)
		{
			var total = _testData.Count;
			var items = new List<FileExplorerItem>();
			await Task.Run(() =>
			{
				var query = _testData
					.AsQueryable<FileExplorerItem>();

				// if search text given then take that as the parent path value
				// if null then return all items (load all example)
				// if empty string then return root item (load on demand example)
				if (request.SearchText != null)
				{
					if (request.SearchText == string.Empty)
					{
						query = query.Where(x => x.Path == RootFolder);
					}
					else
					{
						query = query.Where(x => x.ParentPath == request.SearchText);
					}
				}

				total = query.Count();

				// realize query
				items = query.ToList();

				// remove parent path from all root items
				//items.ForEach(x => x.ParentPath = x.Path == RootFolder ? string.Empty : x.ParentPath);

			}).ConfigureAwait(false);
			return new DataResponse<FileExplorerItem>(items, total);
		}

		/// <summary>
		/// Requests that the item is deleted.
		/// </summary>
		/// <param name="item">The item to be deleted.</param>
		/// <param name="cancellationToken">A cancellation token for the async operation.</param>
		/// <returns>A new OperationResponse instance that contains the results of the operation.</returns>
		public async Task<OperationResponse> DeleteAsync(FileExplorerItem item, CancellationToken cancellationToken)
		{
			var result = new OperationResponse();
			await Task.Run(() =>
			{
				if(_testData.RemoveAll(x => item.EntryType == FileExplorerItemType.Directory
					? x.Path.StartsWith(item.Path)
					: x.Path == item.Path) > 0)
				{
					result.Success = true;
				}
				else
				{
					result.ErrorMessage = "Path not found";
				}
			}).ConfigureAwait(false);
			return result;
		}

		/// <summary>
		/// Requests the given item is updated by applying the given delta.
		/// </summary>
		/// <param name="item">The original item to be updated.</param>
		/// <param name="delta">A dictionary with new property values.</param>
		/// <param name="cancellationToken">A cancellation token for the async operation.</param>
		/// <returns>A new OperationResponse instance that contains the results of the operation.</returns>
		public async Task<OperationResponse> UpdateAsync(FileExplorerItem item, IDictionary<string, object> delta, CancellationToken cancellationToken)
		{
			var result = new OperationResponse();
			await Task.Run(() =>
			{
				// find original item
				var existingItem = _testData.FirstOrDefault(x => x.Path == item.Path);
				if(existingItem == null)
				{
					result.ErrorMessage = "Item not found";
				}
				else
				{
					// only path updates supported
					var pathProp = delta.ContainsKey("Path");
					if(pathProp == null)
					{
						result.ErrorMessage = "Only Path property update supported";
					}
					else
					{
						var newPath = delta["Path"]?.ToString();
						if(string.IsNullOrWhiteSpace(newPath))
						{
							result.ErrorMessage = "Invalid value for Path property";
						}
						else
						{
							// check for move/copy - is the target a directory?
							var targetItem = _testData.FirstOrDefault(x => x.Path == newPath);
							if(targetItem?.EntryType == FileExplorerItemType.Directory)
							{
								// move / copy
								var copyProp = delta.GetType().GetProperty("Copy");
								var isCopy = copyProp?.GetValue(delta)?.ToString().ToLower() == "true";
								MoveOrCopyItem(existingItem, newPath, isCopy);
							}
							else
							{
								// rename
								var previousPath = existingItem.Path;
								existingItem.Path = newPath;
								if (existingItem.EntryType == FileExplorerItemType.Directory)
								{
									// update child paths
									_testData.ForEach(x =>
									{
										x.Path = x.Path.ReplacePathPrefix(previousPath, newPath);
									});
								}
							}
							result.Success = true;
						}
					}
				}
			}).ConfigureAwait(false);
			return result;
		}

		private void MoveOrCopyItem(FileExplorerItem item, string parentPath, bool isCopy)
		{
			if (item.EntryType == FileExplorerItemType.File)
			{
				if (isCopy)
				{
					_testData.Add(new FileExplorerItem
					{
						DateCreated = DateTime.UtcNow,
						DateModified = item.DateModified,
						EntryType = FileExplorerItemType.File,
						FileSize = item.FileSize,
						IsHidden = item.IsHidden,
						IsSystem = item.IsSystem,
						IsReadOnly = item.IsReadOnly,
						Path = Path.Combine(parentPath, item.Name)
					});
				}
				else
				{
					item.Path = Path.Combine(parentPath, item.Name);
				}
			}
			else
			{
				// first move / copy folder entry
				var originalPath = item.Path;
				if (isCopy)
				{
					_testData.Add(new FileExplorerItem
					{
						DateCreated = DateTime.UtcNow,
						DateModified = item.DateModified,
						EntryType = FileExplorerItemType.Directory,
						Path = Path.Combine(parentPath, item.Name)
					});
				}
				else
				{
					item.Path = Path.Combine(parentPath, item.Name);
				}

				// move / copy child items
				var subItems = _testData.Where(x => x.ParentPath == originalPath);
				foreach(var subItem in subItems)
				{
					MoveOrCopyItem(subItem, item.Path, isCopy);
				}
			}
		}

		/// <summary>
		/// Requests the given item is created.
		/// </summary>
		/// <param name="item">New item details.</param>
		/// <param name="cancellationToken">A cancellation token for the async operation.</param>
		/// <returns>A new OperationResponse instance that contains the results of the operation.</returns>
		public async Task<OperationResponse> CreateAsync(FileExplorerItem item, CancellationToken cancellationToken)
		{
			var result = new OperationResponse();
			await Task.Run(() =>
			{
				item.DateCreated = item.DateModified = DateTimeOffset.UtcNow;
				_testData.Add(item);
				result.Success = true;
			});
			return result;
		}
	}
}
