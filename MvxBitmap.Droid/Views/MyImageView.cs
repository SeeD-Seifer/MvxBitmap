using System;
using MvvmCross.Binding.Droid.Views;
using Android.Content;
using Android.Graphics;

namespace MvxBitmap.Droid
{
	public class MyImageView: MvxImageView
	{
		public MyImageView (Context context)
			: base (context)
		{
		}

		public override void SetImageBitmap (Bitmap bitmap)
		{
			if (Handle == IntPtr.Zero) {
				return;
			}

			if (bitmap != null)
			{
				if (bitmap.Handle == IntPtr.Zero)
				{
					//throw new InvalidOperationException ("Bitmap destroyed");
					return;
				}
				if (bitmap.IsRecycled)
				{
					throw new InvalidOperationException ("Bitmap recycled");
				}
			}

			base.SetImageBitmap (bitmap);
		}
	}
}

