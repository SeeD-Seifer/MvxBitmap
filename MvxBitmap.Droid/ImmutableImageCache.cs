// MvxImageCache.cs
// (c) Copyright Cirrious Ltd. http://www.cirrious.com
// MvvmCross is licensed using Microsoft Public License (Ms-PL)
// Contributions and inspirations noted in readme.md and license.txt
//
// Project Lead - Stuart Lodge, @slodge, me@slodge.com

using MvvmCross.Platform;
using MvvmCross.Platform.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Plugins.DownloadCache;
using System.Collections.Immutable;
using System.Threading;

namespace MvxBitmap.Droid
{
	public class ImmutableImageCache<T>
		: MvxAllThreadDispatchingObject
	, IMvxImageCache<T>
	{
		private ImmutableDictionary<string, Entry> _entriesByHttpUrl = ImmutableDictionary.Create<string, Entry>();

		private readonly IMvxFileDownloadCache _fileDownloadCache;
		private readonly int _maxInMemoryBytes;
		private readonly int _maxInMemoryFiles;
		private readonly bool _disposeOnRemove;

		public ImmutableImageCache(IMvxFileDownloadCache fileDownloadCache, int maxInMemoryFiles, int maxInMemoryBytes, bool disposeOnRemove)
		{
			_fileDownloadCache = fileDownloadCache;
			_maxInMemoryFiles = maxInMemoryFiles;
			_maxInMemoryBytes = maxInMemoryBytes;
			_disposeOnRemove = disposeOnRemove;
		}

		#region IMvxImageCache<T> Members

		public Task<T> RequestImage(string url)
		{
			var tcs = new TaskCompletionSource<T>();

			Task.Run(() =>
				{
					Entry entry;
					if (_entriesByHttpUrl.TryGetValue(url, out entry))
					{
						entry.WhenLastAccessedUtc = DateTime.UtcNow;
						tcs.TrySetResult(entry.Image.RawImage);
						return;
					}

					try
					{
						_fileDownloadCache.RequestLocalFilePath(url,
							async s =>
							{
								var image = await Parse(s).ConfigureAwait(false);

								var entriesByUrl = _entriesByHttpUrl.SetItem (url, new Entry(url, image));
								Interlocked.Exchange (ref _entriesByHttpUrl, entriesByUrl);

								tcs.TrySetResult(image.RawImage);
							},
							exception =>
							{
								tcs.TrySetException(exception);
							});
					}
					finally
					{
						ReduceSizeIfNecessary();
					}
				});

			return tcs.Task;
		}

		#endregion IMvxImageCache<T> Members

		private void ReduceSizeIfNecessary()
		{
			RunSyncOrAsyncWithLock(() =>
				{
					var entries = _entriesByHttpUrl.Select (kvp => kvp.Value).ToList ();

					var currentSizeInBytes = entries.Sum(x => x.Image.GetSizeInBytes());
					var currentCountFiles = entries.Count;

					if (currentCountFiles <= _maxInMemoryFiles
						&& currentSizeInBytes <= _maxInMemoryBytes)
						return;

					// we don't use LINQ OrderBy here because of AOT/JIT problems on MonoTouch
					entries.Sort(new MvxImageComparer());

					var entriesToRemove = new List<Entry>();

					while (currentCountFiles > _maxInMemoryFiles
						|| currentSizeInBytes > _maxInMemoryBytes)
					{
						var toRemove = entries[0];
						entries.RemoveAt(0);

						var entriesByUrl = _entriesByHttpUrl.Remove (toRemove.Url);
						Interlocked.Exchange (ref _entriesByHttpUrl, entriesByUrl);

						entriesToRemove.Add (toRemove);

						currentSizeInBytes -= toRemove.Image.GetSizeInBytes();
						currentCountFiles--;
					}

					if (_disposeOnRemove && entriesToRemove.Count > 0)
					{
						DisposeImagesOnMainThread (entriesToRemove);
					}

				});
		}

		private void DisposeImagesOnMainThread (List<Entry> entries)
		{
			InvokeOnMainThread (() => {
				entries.ForEach (e => e.Image.RawImage.DisposeIfDisposable ());
			});
		}

		private class MvxImageComparer : IComparer<Entry>
		{
			public int Compare(Entry x, Entry y)
			{
				return x.WhenLastAccessedUtc.CompareTo(y.WhenLastAccessedUtc);
			}
		}

		protected Task<MvxImage<T>> Parse(string path)
		{
			var loader = Mvx.Resolve<IMvxLocalFileImageLoader<T>>();
			return loader.Load(path, false, 0, 0);
		}

		#region Nested type: Entry

		private class Entry
		{
			public Entry(string url, MvxImage<T> image)
			{
				Url = url;
				Image = image;
				WhenLastAccessedUtc = DateTime.UtcNow;
			}

			public string Url { get; private set; }
			public MvxImage<T> Image { get; private set; }
			public DateTime WhenLastAccessedUtc { get; set; }
		}

		#endregion Nested type: Entry
	}
}