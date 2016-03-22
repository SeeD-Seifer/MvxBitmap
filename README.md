Simple project to test MvvmCross's DownloadCache pluging for Android (for now).

### Application flow
- Load list of image urls on startup
- In a loop request to show a new image (with a very short dealy between requests)
- After all images shown, clean up the screen and start over again after larger delay

On the first loop the images should be loaded to cache.
On all the other loops the images will be used from the cache.

The speed of image loading should lead to in-memory-cache invalidation, which often leads to crashes,
because of race between Bitmap disposal and it's render in MvxImageView.

Based on this project there are found few fixes for the issues in MvvmCross version 4.0.0:
https://github.com/MvvmCross/MvvmCross-Plugins/issues/31
https://github.com/MvvmCross/MvvmCross-Plugins/issues/41

```ConcurrentImageCache``` and ```ImmutableImageCache``` are two options of solution.
```ConcurrentImageCahce``` looks a better solution for me, but PCL does not have ConcurrentDictionary support.

This project for sure will be usefull for futher Bitmap issues investgation in Xamarin + MvvmCross projects.
