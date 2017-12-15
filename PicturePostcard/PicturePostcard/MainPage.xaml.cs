﻿using PicturePostcard.Shared;
using System;
using System.Diagnostics;
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

		async void HandleProcessButtonClicked(object sender, ClickedEventArgs args)
		{
			// Recognize message
			Report("Recognizing text");
			var imageStream = await _padView.GetImageStreamAsync(SignaturePad.Forms.SignatureImageFormat.Jpeg);
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
			switch(sentiment)
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
			if(keyPhrases.Count <= 0)
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
			if(imageUrl == null)
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
			if(height > width)
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