﻿using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace PanoramicData.Blazor
{
	public partial class PDDropZone : IDisposable
	{
		private static int _idSequence;
		private DotNetObjectReference<PDDropZone>? _dotNetReference;

		[Inject] public IJSRuntime JSRuntime { get; set; } = null!;

		/// <summary>
		/// Gets or sets the child content that the drop zone wraps.
		/// </summary>
		[Parameter] public RenderFragment? ChildContent { get; set; }

		/// <summary>
		/// Event raised whenever the user drops files onto the drop zone.
		/// </summary>
		[Parameter] public EventCallback<DropZoneEventArgs> Drop { get; set; }

		/// <summary>
		/// Event raised whenever a file upload starts.
		/// </summary>
		[Parameter] public EventCallback<DropZoneUploadEventArgs> UploadStarted { get; set; }

		/// <summary>
		/// Event raised periodically during a file upload.
		/// </summary>
		[Parameter] public EventCallback<DropZoneUploadProgressEventArgs> UploadProgress { get; set; }

		/// <summary>
		/// Event raised whenever a file upload completes.
		/// </summary>
		[Parameter] public EventCallback<DropZoneUploadCompletedEventArgs> UploadCompleted { get; set; }

		/// <summary>
		/// Event raised when all uploads have completed.
		/// </summary>
		[Parameter] public EventCallback AllUploadsComplete { get; set; }

		/// <summary>
		/// Gets or sets the URL where file uploads should be sent.
		/// </summary>
		[Parameter] public string? UploadUrl { get; set; }

		/// <summary>
		/// Gets or sets a unique identifier for the upload session.
		/// </summary>
		[Parameter] public string SessionId { get; set; } = Guid.NewGuid().ToString();

		/// <summary>
		/// Sets the maximum time in seconds to wait for an upload to complete.
		/// </summary>
		[Parameter] public int Timeout { get; set; } = 30000;

		/// <summary>
		/// Sets the maximum file upload size in MB.
		/// </summary>
		[Parameter] public int MaxFileSize { get; set; } = 256;

		/// <summary>
		/// Sets whether to auto scroll when multiple files uploaded.
		/// </summary>
		[Parameter] public bool AutoScroll { get; set; } = true;

		/// <summary>
		/// Gets the unique identifier of this panel.
		/// </summary>
		public string Id { get; private set; } = string.Empty;

		protected override void OnInitialized()
		{
			Id = $"pddz{++_idSequence}";
		}

		protected async override Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
				_dotNetReference = DotNetObjectReference.Create(this);
				var options = new { url = UploadUrl, timeout = Timeout, autoScroll = AutoScroll, maxFilesize = MaxFileSize };
				if (!string.IsNullOrWhiteSpace(UploadUrl))
				{
					await JSRuntime.InvokeVoidAsync("panoramicData.initDropzone", $"#{Id}", options, SessionId, _dotNetReference).ConfigureAwait(true);
				}
			}
		}

		[JSInvokable("PanoramicData.Blazor.PDDropZone.OnDrop")]
		public async Task<object> OnDrop(DropZoneFile[] files)
		{
			var args = new DropZoneEventArgs(this, files);
			await Drop.InvokeAsync(args).ConfigureAwait(true);
			return new
			{
				cancel = args.Cancel,
				reason = args.CancelReason,
				rootDir = args.BaseFolder,
				state = args.State,
				files
			};
		}

		[JSInvokable("PanoramicData.Blazor.PDDropZone.OnUploadBegin")]
		public async Task<string[]> OnUploadBeginAsync(DropZoneFile file)
		{
			if (file is null)
			{
				throw new ArgumentNullException(nameof(file));
			}
			if (file.Path is null)
			{
				throw new ArgumentException("file's Path Property should not be null.", nameof(file));
			}
			if (file.Name is null)
			{
				throw new ArgumentException("file's Name Property should not be null.", nameof(file));
			}
			var args = new DropZoneUploadEventArgs(file.Path, file.Name, file.Size, file.Key, file.SessionId);
			await UploadStarted.InvokeAsync(args).ConfigureAwait(true);
			if (args.FormFields.Count == 0)
			{
				return new string[0];
			}
			else
			{
				var fields = new System.Collections.Generic.List<string>();
				foreach (var kvp in args.FormFields)
				{
					fields.Add($"{kvp.Key}={kvp.Value}");
				}
				return fields.ToArray();
			}
		}

		[JSInvokable("PanoramicData.Blazor.PDDropZone.OnUploadProgress")]
		public void OnUploadProgress(DropZoneFileUploadProgress file)
		{
			if (file is null)
			{
				throw new ArgumentNullException(nameof(file));
			}
			if (file.Path is null)
			{
				throw new ArgumentException("file's Path Property should not be null.", nameof(file));
			}
			if (file.Name is null)
			{
				throw new ArgumentException("file's Name Property should not be null.", nameof(file));
			}
			UploadProgress.InvokeAsync(new DropZoneUploadProgressEventArgs(file.Path, file.Name, file.Size, file.Key, file.SessionId, file.Progress));
		}

		[JSInvokable("PanoramicData.Blazor.PDDropZone.OnUploadEnd")]
		public void OnUploadEnd(DropZoneFileUploadOutcome file)
		{
			if (file is null)
			{
				throw new ArgumentNullException(nameof(file));
			}
			if (file.Path is null)
			{
				throw new ArgumentException("file's Path Property should not be null.", nameof(file));
			}
			if (file.Name is null)
			{
				throw new ArgumentException("file's Name Property should not be null.", nameof(file));
			}
			if (file.Success)
			{
				UploadCompleted.InvokeAsync(new DropZoneUploadCompletedEventArgs(file.Path, file.Name, file.Size, file.Key, file.SessionId));
			}
			else
			{
				UploadCompleted.InvokeAsync(new DropZoneUploadCompletedEventArgs(file.Path, file.Name, file.Size, file.Key, file.SessionId, file.Reason));
			}
		}

		[JSInvokable("PanoramicData.Blazor.PDDropZone.OnAllUploadsComplete")]
		public void OnAllUploadsComplete()
		{
			AllUploadsComplete.InvokeAsync(null);
		}

		public void Dispose()
		{
			JSRuntime.InvokeVoidAsync("panoramicData.disposeDropZone", Id);
			_dotNetReference?.Dispose();
		}
	}
}
