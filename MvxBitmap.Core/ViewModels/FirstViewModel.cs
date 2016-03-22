using MvvmCross.Core.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;

namespace MvxBitmap.Core.ViewModels
{
	public class UrlEventArgs: EventArgs
	{
		public string Url { get; private set; }

		public UrlEventArgs (string url)
		{
			Url = url;
		}
	}

    public class FirstViewModel 
        : MvxViewModel
    {
		public event EventHandler<UrlEventArgs> ImagePathAdded;
		public event EventHandler NextLoopStarted;

		private bool stopped;

		public async void Init ()
		{
			var urls = await LoadUrls (30);

			int i = 0;

			while (!stopped)
			{
				await Task.Delay (50);

				i++;
				if (i >= urls.Count)
				{
					i = 0;
					NextLoopStarted?.Invoke (this, EventArgs.Empty);
					await Task.Delay (1000);
				}

				var url = urls [i];
				ImagePathAdded?.Invoke (this, new UrlEventArgs (url));
			}
		}

		private async Task<IList<string>> LoadUrls (int resultsNumber)
		{
			var client = new HttpClient ();
			var path = "http://thecatapi.com/api/images/get?format=xml&results_per_page=" + resultsNumber;
			var xml = await client.GetStringAsync (path);

			var xdoc = XDocument.Parse (xml);
			var ximages = xdoc.Element ("response").Element ("data").Element ("images");

			var urls = from e in ximages.Elements ("image")
			           select e.Element ("url").Value;
			
			return urls.ToList ();
		}

		public void Close ()
		{
			stopped = true;
			Close (this);
		}
    }
}
