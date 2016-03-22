using MvvmCross.Platform.Plugins;
using MvvmCross.Platform;
using MvvmCross.Plugins.DownloadCache;
using Android.Graphics;
using MvvmCross.Plugins.DownloadCache.Droid;
using System;
using Android.App;
using Android.Content;
using Android.Content.PM;

namespace MvxBitmap.Droid.Bootstrap
{
    public class DownloadCachePluginBootstrap
        : MvxPluginBootstrapAction<MvvmCross.Plugins.DownloadCache.PluginLoader>
    {
		protected override void Load(IMvxPluginManager manager)
		{
			base.Load(manager);

			//Mvx.RegisterSingleton<IMvxImageCache<Bitmap>>(() => CreateCache());
		}

		private IMvxImageCache<Bitmap> CreateCache()
		{
			var configuration = MvxDownloadCacheConfiguration.Default;
			configuration.MaxInMemoryBytes = 1 * 1024 * 1024;

			var fileDownloadCache = new MvxFileDownloadCache(configuration.CacheName,
				configuration.CacheFolderPath,
				configuration.MaxFiles,
				configuration.MaxFileAge);

			// TODO change ImageCache for different cases
			// - use MvxImageCache for default behaviour (which leads to often crashes)
			// - use ImmutableImageCache or ConcurrentImageCache to test the fix

			var fileCache = new ImmutableImageCache<Bitmap>(fileDownloadCache, configuration.MaxInMemoryFiles, 
				configuration.MaxInMemoryBytes, configuration.DisposeOnRemoveFromCache);
			return fileCache;
		}

		private static int GetCacheSizeInPercent(float percent)
		{
			if (percent < 0.01f || percent > 0.8f)
				throw new Exception("GetCacheSizeInPercent - percent must be between 0.01 and 0.8 (inclusive)");

			var context = Android.App.Application.Context.ApplicationContext;
			var am = (ActivityManager) context.GetSystemService(Context.ActivityService);
			bool largeHeap = (context.ApplicationInfo.Flags & ApplicationInfoFlags.LargeHeap) != 0;
			int memoryClass = am.MemoryClass;
			if (largeHeap)
			{
				memoryClass = am.LargeMemoryClass;
			}

			int availableMemory = 1024 * 1024 * memoryClass;
			return (int)Math.Round(percent * availableMemory);
		}				
    }
}