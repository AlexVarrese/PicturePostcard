using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Plugin.AudioRecorder;
using System.IO;
using System.Diagnostics;

namespace AudioRecord.Forms
{
	public partial class MainPage : ContentPage
	{
		AudioRecorderService recorder;
		AudioPlayer player;

		public MainPage ()
		{
			InitializeComponent ();

			recorder = new AudioRecorderService
			{
				StopRecordingAfterTimeout = true,
				TotalAudioTimeout = TimeSpan.FromSeconds (15),
				AudioSilenceTimeout = TimeSpan.FromSeconds(2)
			};

			player = new AudioPlayer ();
			player.FinishedPlaying += Player_FinishedPlaying;
		}


		async void Record_Clicked (object sender, EventArgs e)
		{
			await RecordAudio ();
		}


		async Task RecordAudio ()
		{
			try
			{
				if (!recorder.IsRecording) //Record button clicked
				{
					recorder.StopRecordingOnSilence = TimeoutSwitch.IsToggled;

					RecordButton.IsEnabled = false;
					PlayButton.IsEnabled = false;

					//start recording audio
					var filename = Path.Combine(Path.GetTempPath(), "1.wav");
					var audioRecordTask = await recorder.StartRecording (filename);
					Debug.WriteLine(filename);

					RecordButton.Text = "Stop Recording";
					RecordButton.IsEnabled = true;

					await audioRecordTask;

					RecordButton.Text = "Record";
					PlayButton.IsEnabled = true;
				}
				else //Stop button clicked
				{
					RecordButton.IsEnabled = false;

					//stop the recording...
					_recordedFileLocation = await recorder.StopRecording ();
					Debug.WriteLine("Recorded to: " + _recordedFileLocation);

					RecordButton.IsEnabled = true;
				}
			}
			catch (Exception ex)
			{
				//blow up the app!
				throw ex;
			}
		}

		string _recordedFileLocation;


		void Play_Clicked (object sender, EventArgs e)
		{
			PlayAudio ();
		}


		void PlayAudio ()
		{
			try
			{
				var filePath = _recordedFileLocation;

				if (filePath != null)
				{
					PlayButton.IsEnabled = false;
					RecordButton.IsEnabled = false;

					player.Play (filePath);
				}
			}
			catch (Exception ex)
			{
				//blow up the app!
				throw ex;
			}
		}


		void Player_FinishedPlaying (object sender, EventArgs e)
		{
			PlayButton.IsEnabled = true;
			RecordButton.IsEnabled = true;
		}
	}
}