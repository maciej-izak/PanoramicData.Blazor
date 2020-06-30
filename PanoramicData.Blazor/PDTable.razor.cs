﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PanoramicData.Blazor.Services;
using PanoramicData.Blazor.Exceptions;
using PanoramicData.Blazor.Extensions;

namespace PanoramicData.Blazor
{
	public partial class PDTable<TItem>
	{
		/// <summary>
		/// Injected log service.
		/// </summary>
		[Inject] protected ILogger<PDTable<TItem>> Logger { get; set; } = new NullLogger<PDTable<TItem>>();

		/// <summary>
		/// Injected navigation manager.
		/// </summary>
		[Inject] protected NavigationManager NavigationManager { get; set; } = null!;

		/// <summary>
		/// Child HTML content.
		/// </summary>
		[Parameter] public RenderFragment ChildContent { get; set; } = null!;

		/// <summary>
		/// Gets or sets the IDataProviderService instance to use to fetch data.
		/// </summary>
		[Parameter] public IDataProviderService<TItem> DataProvider { get; set; } = null!;

		/// <summary>
		/// Gets or sets the CSS class to apply to the tables container element.
		/// </summary>
		[Parameter] public string TableClass { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the CSS class to apply to the table header element.
		/// </summary>
		[Parameter] public string THeadClass { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the message to be displayed when no data is available.
		/// </summary>
		[Parameter] public string NoDataMessage { get; set; } = "No data";

		/// <summary>
		/// Gets or sets a delegate to be called if an exception occurs.
		/// </summary>
		[Parameter] public EventCallback<Exception> ExceptionHandler { get; set; }

		/// <summary>
		/// Gets or sets the default sort column.
		/// </summary>
		[Parameter] public string? DefaultSortColumn { get; set; }

		/// <summary>
		/// Gets or sets the default sort direction.
		/// </summary>
		[Parameter] public SortDirection DefaultSortDirection { get; set; }

		/// <summary>
		/// Gets a full list of all columns.
		/// </summary>
		public List<PDColumn<TItem>> Columns { get; } = new List<PDColumn<TItem>>();

		/// <summary>
		/// Gets a list of columns to be displayed.
		/// </summary>
		protected List<PDColumn<TItem>> ColumnsToDisplay => Columns;

		/// <summary>
		/// Gets the items to be displayed as rows.
		/// </summary>
		protected IEnumerable<TItem> ItemsToDisplay { get; private set; } = Enumerable.Empty<TItem>();

		/// <summary>
		/// Has the table been initialized?
		/// </summary>
		protected bool TableInitialised { get; set; } = false;

		protected override void OnInitialized()
		{
			if (DataProvider is null)
			{
				throw new PDTableException($"{nameof(DataProvider)} must not be null.");
			}
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			// If this is the first time we've finished rendering, then all the columns
			// have been added to the table so we'll go and get the data for the first time
			if (firstRender)
			{
				try
				{
					if (DefaultSortColumn != null)
					{
						var columnToSortBy = Columns.SingleOrDefault(c => c.Title == DefaultSortColumn);
						if (columnToSortBy != null)
						{
							await columnToSortBy.SortByAsync(DefaultSortDirection).ConfigureAwait(true);
						}
					}

					var defaultSortColumns = Columns.Where(c => c.DefaultSortColumn).ToList();
					if (defaultSortColumns.Count > 1)
					{
						throw new PDTableException($"Only one column can have {nameof(PDColumn<TItem>.DefaultSortColumn)} set to true.");
					}

					// Get the requested table parameters from the QueryString
					var uri = new Uri(NavigationManager.Uri);
					var query = QueryHelpers.ParseQuery(uri.Query);

					// Sort
					if (query.TryGetValue("sort", out var requestedSortFields))
					{
						var sortFieldSpecs = requestedSortFields[0].Split('|');
						if (sortFieldSpecs.Length == 2)
						{
							// Find the sort column if we can
							var targetSortColumn = Columns.SingleOrDefault(c => string.Equals(c.PropertyInfo?.Name, sortFieldSpecs[0], StringComparison.InvariantCultureIgnoreCase));
							if (targetSortColumn != null)
							{
								var requestedSortDirection = sortFieldSpecs[1] switch
								{
									"asc" => SortDirection.Ascending,
									"desc" => SortDirection.Descending,
									_ => targetSortColumn.DefaultSortDirection
								};
								await targetSortColumn.SortByAsync(requestedSortDirection).ConfigureAwait(true);
							}
						}
					}
				}
				catch (Exception ex)
				{
					await HandleExceptionAsync(ex).ConfigureAwait(true);
				}
				finally
				{
					TableInitialised = true;
					StateHasChanged();
				}

				try
				{
					await GetDataAsync().ConfigureAwait(true);
					StateHasChanged();
				}
				catch (Exception ex)
				{
					await HandleExceptionAsync(ex).ConfigureAwait(true);
				}
			}
		}

		/// <summary>
		/// Adds the given column to the list of available columns.
		/// </summary>
		/// <param name="column">The PDColumn to be added.</param>
		public async Task AddColumnAsync(PDColumn<TItem> column)
		{
			try
			{
				Columns.Add(column);
				StateHasChanged();
			}
			catch (Exception ex)
			{
				await HandleExceptionAsync(ex).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Centralized method to process exceptions.
		/// </summary>
		/// <param name="ex">Exception that has been raised.</param>
		public async Task HandleExceptionAsync(Exception ex)
		{
			Logger.LogError(ex, ex.Message);
			await ExceptionHandler.InvokeAsync(ex).ConfigureAwait(true);
		}

		/// <summary>
		/// Requests data from the data provider using the current settings.
		/// </summary>
		public async Task GetDataAsync()
		{
			try
			{
				var sortColumn = Columns.SingleOrDefault(c => c.SortColumn);
				var request = new DataRequest<TItem>
				{
					Skip = 0,
					Take = 10,
					ForceUpdate = false,
					SortFieldExpression = sortColumn?.Field,
					SortDirection = sortColumn?.SortDirection
				};

				// perform query data
				var response = await DataProvider
					.GetDataAsync(request, CancellationToken.None)
					.ConfigureAwait(true);

				ItemsToDisplay = response.Items;
			}
			finally
			{
			}
		}

		/// <summary>
		/// Sort the data by the specified column.
		/// </summary>
		/// <param name="column">The column to sort by.</param>
		/// <remarks>To disable sorting for any given column, set its Sortable property set to false.</remarks>
		public async Task SortBy(PDColumn<TItem> column)
		{
			if (column.Sortable)
			{
				await column.SortByAsync().ConfigureAwait(true);
				var sortStr = column.SortDirection == SortDirection.Ascending ? "asc" : "desc";
				// Update the URI for bookmarking
				NavigationManager.SetUri(new Dictionary<string, object> { { "sort", $"{column.PropertyInfo!.Name}|{sortStr}" } });
				await GetDataAsync().ConfigureAwait(true);
			}
		}
	}
}
