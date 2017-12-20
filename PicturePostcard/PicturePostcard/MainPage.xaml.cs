using PicturePostcard.Shared;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xamarin.Forms;

namespace PicturePostcard
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
			_emotional = DependencyService.Get<IEmotional>();
		}

		IEmotional _emotional;

		void HandleClearButtonClicked(object sender, ClickedEventArgs args)
		{
			_padView.Clear();
			_masterLayout.BackgroundColor = Color.AntiqueWhite;
			_image.Source = "xamarin.png";
		}

		async void HandleProcessButtonClicked(object sender, ClickedEventArgs args)
		{
			// Recognize message
			Report("Recognizing text");

			// Make sure to set the fill color. The service cannot handle images with alpha transparency.
			var imageStream = await _padView.GetImageStreamAsync(SignaturePad.Forms.SignatureImageFormat.Png, strokeColor: Color.Black, fillColor: Color.White);

			// Commenting in this will save the file to disk before sending it into the cloud. Useful for debugging.
			/*
			Directory.CreateDirectory(App.UwpPath);
			var p = Path.Combine(App.UwpPath, "image.png");
			Debug.WriteLine(p);
			using (var f = File.OpenWrite(p))
			{
				imageStream.CopyTo(f);
			}
			imageStream.Seek(0, SeekOrigin.Begin);
			imageStream.Close();
			imageStream = File.OpenRead(Path.Combine(App.UwpPath, "image.png"));
			*/

			var message = await _emotional.RecognizeHandwrittenTextAsync(imageStream);

			if (string.IsNullOrWhiteSpace(message))
			{
				Report("Sorry, I could not recognize this.");
				_recognizedTextLabel.Text = "unable to recognize - try again";
				return;
			}

			Report($"You wrote: {message}");
			_recognizedTextLabel.Text = message;

			// Get sentiment
			Report("Getting sentiment");
			var sentiment = await _emotional.AnalyzeSentimentAsync(message);
			switch (sentiment)
			{
				case Shared.Sentiment.Unknown:
					Report("No feelings, Mr. Spock?");
					_masterLayout.BackgroundColor = Color.AntiqueWhite;
					break;
				case Shared.Sentiment.Normal:
					Report("Not excited?");
					_masterLayout.BackgroundColor = Color.LightSteelBlue;
					break;
				case Shared.Sentiment.Negative:
					Report("Cheer up!");
					_masterLayout.BackgroundColor = Color.DarkRed;
					break;
				case Shared.Sentiment.Positive:
					Report("Happiness all the way!");
					_masterLayout.BackgroundColor = Color.LightYellow;
					break;
			}

			// Get key content
			Report("Getting key phrases");
			var keyPhrases = await _emotional.GetKeyPhrasesAsync(message);
			string imageSearchTerm = message;
			if (keyPhrases.Count <= 0)
			{
				Report("Couldn't figure out what you mean.");
			}
			else
			{
				imageSearchTerm = string.Join(" ", keyPhrases);
				Report($"I understood: {imageSearchTerm}");
			}

			// Get a matching image.
			Report("Looking for an image");
			var imageUrl = await _emotional.GetImageUrlAsync(imageSearchTerm);
			if (imageUrl == null)
			{
				Report("Could not find an image");
				_image.Source = "xamarin.png";
			}
			else
			{
				_image.Source = ImageSource.FromUri(new Uri(imageUrl));
			}
		}

		protected override void OnSizeAllocated(double width, double height)
		{
			base.OnSizeAllocated(width, height);
			if (height > width)
			{
				// Portrait
				_masterLayout.Orientation = StackOrientation.Vertical;

				_image.WidthRequest = _masterLayout.Width;
				_image.HeightRequest = 300;
			}
			else
			{
				// Landscape
				_masterLayout.Orientation = StackOrientation.Horizontal;

				_image.WidthRequest = 300;
				_image.HeightRequest = _masterLayout.Height;
			}
		}

		void Report(string msg)
		{
			Debug.WriteLine(msg);
		}
	}
}
