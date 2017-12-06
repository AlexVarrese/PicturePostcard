using Plugin.AudioRecorder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Cognitive.BingSpeech;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.IO;
using System.Threading;

namespace ConferenceApp
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SpeakerTranslationPage : ContentPage
	{
		public SpeakerTranslationPage()
		{
			InitializeComponent();
		}

		AudioRecorderService _recorder = new AudioRecorderService
		{
			StopRecordingOnSilence = true,
			AudioSilenceTimeout = TimeSpan.FromSeconds(2),
			StopRecordingAfterTimeout = true,
			TotalAudioTimeout = TimeSpan.FromSeconds(30)
		};

		BingSpeechApiClient _bingSpeechClient = new BingSpeechApiClient("a82fc42937ae4f46a8e2138408c413e6");


		protected override void OnAppearing()
		{
			base.OnAppearing();
			btnRecord.Clicked += OnRecordClicked;
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			btnRecord.Clicked -= OnRecordClicked;
		}

		async Task RecognizeAndTranslateAsync()
		{
			Task<string> audioRecordTask;
			string targetFilename = null;

			// Start recording audio.
			try
			{
				audioRecordTask = await _recorder.StartRecording();
			}
			catch (Exception ex)
			{
				txtRecognized.Text = $"Error recording audio: {ex}";
				return;
			}

			// Wait for recording to complete.
			try
			{
				targetFilename = await audioRecordTask;
				if (targetFilename == null)
				{
					return;
				}
			}
			catch (Exception ex)
			{
				txtRecognized.Text = $"Error getting recorded audio file: {ex}";
				return;
			}

			// Send recorded audio to service for speech to text conversion.
			try
			{
				var recognitionResult = await _bingSpeechClient.SpeechToTextSimple(targetFilename);

				if (recognitionResult.RecognitionStatus == RecognitionStatus.Success)
				{
					txtRecognized.Text = recognitionResult.DisplayText;
				}
				else
				{
					txtRecognized.Text = $"Failed to process speech. Error: {recognitionResult.RecognitionStatus}";
				}
			}
			catch (Exception ex)
			{
				txtRecognized.Text = $"Error converting speech to text: {ex}";
				return;
			}

			_fileIndex++;
		}

		int _fileIndex;
		string _tempPath;

		async void OnRecordClicked(object sender, EventArgs e)
		{
			if (_recorder.IsRecording)
			{
				// If button is pressed and we are recording, stop recording and update UI.
				btnRecord.IsEnabled = false;
				await _recorder.StopRecording();
				btnRecord.Text = "Start";
				btnRecord.IsEnabled = true;
			}
			else
			{
				// If we are not recording, start recording and change UI to allow user to stop.
				btnRecord.Text = "Stop";

				// Store recorded data into a temp folder.
				_fileIndex = 0;
				_tempPath = Path.Combine(Path.GetTempPath(), "queued_recordings");
				try
				{
					Directory.Delete(_tempPath, true);
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"No success deleting '{_tempPath}': {ex}");
				}
				if (!Directory.Exists(_tempPath))
				{
					Directory.CreateDirectory(_tempPath);
				}

				await RecognizeAndTranslateAsync();
			}
		}
	}
}