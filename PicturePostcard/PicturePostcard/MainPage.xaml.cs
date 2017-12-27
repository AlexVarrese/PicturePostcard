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
			_image.Source = "xamarin.png";
		}

		async void HandleProcessButtonClicked(object sender, ClickedEventArgs args)
		{
			// Recognize message
			Report("Let me try to recognize your handwriting...", clear: true);

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
				Report("Sorry, I could not recognize this. Can you maybe try to write a bit clearer?");
				return;
			}

			Report($"I think you wrote '{message}'. Does that mean you are...", newLine: false);

			// Get sentiment
			var sentiment = await _emotional.AnalyzeSentimentAsync(message);
			switch (sentiment)
			{
				case Shared.Sentiment.Unknown:
					Report("well...I don't know how you feel about this!");
					_sentimentLabel.Text = "I don't know how I feel today!";
					_sentimentLabel.TextColor = Color.Black;
					break;
				case Shared.Sentiment.Normal:
					Report("feeling indifferent about this?");
					_sentimentLabel.Text = "I'm feeling indifferent.";
					_sentimentLabel.TextColor = Color.Gray;
					break;
				case Shared.Sentiment.Negative:
					Report("concerned about this? No worries!");
					_sentimentLabel.Text = "Stay away. I'm angry.";
					_sentimentLabel.TextColor = Color.Red;
					break;
				case Shared.Sentiment.Positive:
					Report("feeling happy?");
					_sentimentLabel.Text = "I'm happy!";
					_sentimentLabel.TextColor = Color.LightGoldenrodYellow;
					break;
			}

			// Get key content
			Report("Let's see what the key phrases of your message are.");
			var keyPhrases = await _emotional.GetKeyPhrasesAsync(message);
			string imageSearchTerm = message;
			if (keyPhrases.Count <= 0)
			{
				Report("Sorry, I could not figure ot the key phrases.");
			}
			else
			{
				imageSearchTerm = string.Join(" ", keyPhrases);
				Report($"Here's the important parts of your message: '{imageSearchTerm}'");
			}

			// Get a matching image.
			Report("Based on these key phrases, let's find an image!");
			var imageUrl = await _emotional.GetImageUrlAsync(imageSearchTerm);
			if (imageUrl == null)
			{
				Report("Sorry, could not find an image. Showing you a Xamarin logo instead.");
				_image.Source = "xamarin.png";
			}
			else
			{
				Report("I found a nice image for you!");
				_image.Source = ImageSource.FromUri(new Uri(imageUrl));
			}
		}
		
		void Report(string msg, bool newLine = true, bool clear = false)
		{
			Debug.WriteLine(msg);
			if(clear)
			{
				_statusLabel.Text = string.Empty;
			}
			_statusLabel.Text += msg + (newLine ? Environment.NewLine : string.Empty);
		}
	}
}
