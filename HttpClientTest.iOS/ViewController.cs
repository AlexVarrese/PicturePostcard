using System;

using UIKit;
using PicturePostcard.Shared;
using System.Reflection;

namespace HttpClientTest.iOS
{
	public partial class ViewController : UIViewController
	{
		IEmotional _emotional = new CognitiveServicesEmotionalImpl();

		async partial void HandleButtonClick(UIButton sender)
		{
			var assembly = Assembly.GetExecutingAssembly();
			var imageStream = assembly.GetManifestResourceStream("HttpClientTest.iOS.testhandwriting.jpg");

			var text = await _emotional.RecognizeHandwrittenTextAsync(imageStream);
			imageStream.Dispose();
			Console.WriteLine("Done processing image");
		}

		protected ViewController(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			// Perform any additional setup after loading the view, typically from a nib.
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}
	}
}
