using PicturePostcard.Shared;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace PicturePostcard
{
    public partial class App : Application
	{
		public static string UwpPath;
		public App ()
		{
			InitializeComponent();

			DependencyService.Register<IEmotional, CognitiveServicesEmotionalImpl>();
			MainPage = new MainPage();
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
