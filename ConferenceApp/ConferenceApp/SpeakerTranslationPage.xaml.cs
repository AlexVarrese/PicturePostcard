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
			AudioSilenceTimeout = TimeSpan.FromSeconds(1),
			StopRecordingAfterTimeout = true,
			TotalAudioTimeout = TimeSpan.FromSeconds(3)
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

		async void OnRecordClicked(object sender, EventArgs e)
		{
			if (!_recorder.IsRecording)
			{
				btnRecord.Text = "Stop";

				Task<string> audioRecordTask;
				try
				{
					audioRecordTask = await _recorder.StartRecording();
				}
				catch (Exception)
				{
					Debug.WriteLine("Starting recording failed.");
					throw;
				}

				var filename = await audioRecordTask;
				btnRecord.Text = "Start";

				if (filename != null)
				{
					var recognitionResult = await _bingSpeechClient.SpeechToTextSimple(filename);
					if (recognitionResult.RecognitionStatus == RecognitionStatus.Success)
					{
						txtRecognized.Text += recognitionResult.DisplayText;
					}
					else
					{
						Debug.WriteLine("Failed to process speech: " + recognitionResult.RecognitionStatus);
					}
				}
				else
				{
					throw new InvalidOperationException("No audio file stored!");
				}
			}
			else
			{
				try
				{
					await _recorder.StopRecording();

				}
				catch (Exception)
				{
					Debug.WriteLine("Stopping recording failed.");
					throw;
				}
				btnRecord.Text = "Start";
			}


		}
	}
}