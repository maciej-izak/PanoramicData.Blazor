﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace PanoramicData.Blazor.Demo.Data
{
	public class DirectoryEntry
	{
		public bool CanCopyMove { get; set; } = true;
		public DateTimeOffset DateCreated { get; set; } = DateTimeOffset.UtcNow;
		public DateTimeOffset DateModified { get; set; } = DateTimeOffset.UtcNow;
		public bool IsHidden { get; set; }
		public bool IsSystem { get; set; }
		public bool IsReadOnly { get; set; }
		public List<DirectoryEntry> Items { get; } = new List<DirectoryEntry>();
		public string Name { get; set; } = string.Empty;
		public DirectoryEntry? Parent { get; set; }
		public long Size { get; set; }
		public FileExplorerItemType Type { get; set; }

		public DirectoryEntry()
		{
		}

		public DirectoryEntry(FileExplorerItem item)
		{
			CanCopyMove = item.CanCopyMove;
			DateCreated = item.DateCreated ?? DateTimeOffset.UtcNow;
			DateModified = item.DateModified ?? DateTimeOffset.UtcNow;
			IsHidden = item.IsHidden;
			IsReadOnly = item.IsReadOnly;
			IsSystem = item.IsSystem;
			Name = item.Name;
			Size = item.FileSize;
			Type = item.EntryType;
		}

		public DirectoryEntry(string name, params DirectoryEntry[] items)
		{
			Name = name;
			foreach (var item in items)
			{
				item.Parent = this;
			}
			Items.AddRange(items);
		}

		public DirectoryEntry(string name, FileExplorerItemType type, int size)
		{
			Name = name;
			Type = type;
			Size = size;
		}

		public DirectoryEntry(params DirectoryEntry[] items)
		{
			foreach (var item in items)
			{
				item.Parent = this;
			}
			Items.AddRange(items);
		}

		public DirectoryEntry Clone(bool deep = true)
		{
			var clone = new DirectoryEntry
			{
				CanCopyMove = CanCopyMove,
				DateCreated = DateCreated,
				DateModified = DateModified,
				IsHidden = IsHidden,
				IsReadOnly = IsReadOnly,
				IsSystem = IsSystem,
				Name = Name,
				Size = Size,
				Type = Type
			};
			if (deep)
			{
				foreach (var item in Items)
				{
					var clonedItem = item.Clone(true);
					clonedItem.Parent = clone;
					clone.Items.Add(clonedItem);
				}
			}
			return clone;
		}

		public int Count()
		{
			return Reduce((_, pv) => pv + 1, 0);
		}

		public void ForEach(Action<DirectoryEntry> action)
		{
			action(this);
			foreach (var item in Items)
			{
				item.ForEach(action);
			}
		}

		public string Path(string separator = "/")
		{
			var stack = new Stack<string>();
			var node = this;
			while (node != null)
			{
				if (!string.IsNullOrWhiteSpace(node.Name))
				{
					stack.Push(node.Name);
				}
				node = node.Parent;
			}
			var path = string.Join(separator, stack.ToArray());
			return $"{separator}{path}";
		}

		public T Reduce<T>(Func<DirectoryEntry, T, T> func, T previousValue)
		{
			var value = func(this, previousValue);
			foreach (var item in Items)
			{
				value = item.Reduce(func, value);
			}
			return value;
		}

		public FileExplorerItem ToFileExploreritem(string pathSeparator = "/")
		{
			return new FileExplorerItem
			{
				CanCopyMove = CanCopyMove,
				DateCreated = DateCreated,
				DateModified = DateModified,
				EntryType = Type,
				FileSize = Size,
				HasSubFolders = Items.Any(x => x.Type == FileExplorerItemType.Directory),
				IsHidden = IsHidden,
				IsReadOnly = IsReadOnly,
				IsSystem = IsSystem,
				Path = Path(pathSeparator)
			};
		}

		public override string ToString()
		{
			return Path();
		}

		public IEnumerable<DirectoryEntry> Where(Predicate<DirectoryEntry> predicate)
		{
			var items = new List<DirectoryEntry>();
			ForEach((x) =>
			{
				if (predicate(x))
				{
					items.Add(x);
				}
			});
			return items;
		}
	}
}
