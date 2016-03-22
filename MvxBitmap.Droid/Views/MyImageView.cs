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

//		public override void SetImageBitmap (Bitmap bitmap)
//		{
//			// TODO this check avoids crashes when MvxImageView disposed
//			if (Handle == IntPtr.Zero) {
//				return;
//			}
//
//			if (bitmap != null)
//			{
//				// TODO this check avoids crashes when Bitmap disposed
//				if (bitmap.Handle == IntPtr.Zero || bitmap.IsRecycled)
//				{
//					return;
//				}
//			}
//
//			base.SetImageBitmap (bitmap);
//		}
	}
}

