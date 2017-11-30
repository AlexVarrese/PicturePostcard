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
				btnRecord.Text = "Stop recording";

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

				using (var stream = _recorder.GetAudioFileStream())
				{
					//this will begin sending the recording audio data as it continues to record
					var simpleResult = await _bingSpeechClient.SpeechToTextSimple(
						stream,
						_recorder.AudioStreamDetails.SampleRate,
						audioRecordTask);

					if (simpleResult.RecognitionStatus == RecognitionStatus.Success)
					{
						txtRecognized.Text += simpleResult.DisplayText;
					}
				}
			}
			else
			{
				btnRecord.Text = "Start recording";
				try
				{
					await _recorder.StopRecording();
				}
				catch (Exception)
				{
					Debug.WriteLine("Stopping recording failed.");
					throw;
				}
			}


		}
	}
}