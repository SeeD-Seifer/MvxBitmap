using Android.App;
using Android.OS;
using MvvmCross.Droid.Views;
using Android.Widget;
using MvxBitmap.Core.ViewModels;
using MvvmCross.Binding.Droid.Views;
using System;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Java.Interop;

namespace MvxBitmap.Droid.Views
{
    [Activity(Label = "Bitmap Stress Test")]
	public class FirstView : MvxActivity<FirstViewModel>
    {
		private static readonly Random R = new Random ();

		private RelativeLayout canvas;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.FirstView);

			canvas = FindViewById<RelativeLayout> (Resource.Id.canvas);

			ViewModel.ImagePathAdded += (sender, e) => AddImage (e.Url);
			ViewModel.NextLoopStarted += delegate { ClearCanvas (null); };
        }

		private void AddImage (string url)
		{
			var w = 200;
			var h = 200;
			var x = R.Next (canvas.Width - w);
			var y = R.Next (canvas.Height - h);

			var imageView = new MyImageView (this);
			imageView.SetScaleType (ImageView.ScaleType.FitCenter);
			imageView.SetBackgroundColor (Color.DimGray);
			imageView.LayoutParameters = new RelativeLayout.LayoutParams (w, h) {
				TopMargin = y,
				LeftMargin = x,
			};

			canvas.AddView (imageView);

			imageView.DefaultImagePath = "res:placeholder";
			imageView.ImageUrl = url;
		}

		[Export ("ClearCanvas")]
		public void ClearCanvas (View view)
		{
			for (int i = canvas.ChildCount - 1; i >= 0; i--)
			{
				var child = canvas.GetChildAt (i);
				canvas.RemoveView (child);
				child.Dispose ();
			}
		}
    }
}
