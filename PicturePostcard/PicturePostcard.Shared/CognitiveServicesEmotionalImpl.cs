using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PicturePostcard.Shared
{
	public class CognitiveServicesEmotionalImpl : IEmotional
	{
		// Get a trial key at https://azure.microsoft.com/en-us/try/cognitive-services/
		const string API_KEY = "1ecf4c9e-f4ed-45ee-b47a-1227855edbef";

		// Trial keys only work for the West Central US region.
		const string COGNITIVE_SERVICES_BASE_URL = "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0/";

		// All services are RESTful.
		readonly HttpClient _client = new HttpClient
		{
			BaseAddress = new Uri(COGNITIVE_SERVICES_BASE_URL),
			Timeout = TimeSpan.FromSeconds(60),
		};

		public CognitiveServicesEmotionalImpl()
		{
			_client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", API_KEY);
		}

		public async Task<Sentiment> AnalyzeSentimentAsync(string text)
		{
			await Task.Delay(500);

			if (string.IsNullOrWhiteSpace(text))
			{
				return Sentiment.Unknown;
			}

			if (new[] { "happy", "nice", "lucky" }.Any(x => text.ToLower().Contains(x)))
			{
				return Sentiment.Positive;
			}

			if (new[] { "bad", "fail", "miserable" }.Any(x => text.ToLower().Contains(x)))
			{
				return Sentiment.Positive;
			}

			return Sentiment.Normal;
		}

		public async Task<IReadOnlyList<string>> GetKeyPhrasesAsync(string text)
		{
			await Task.Delay(500);
			var result = new List<string>();
			if (string.IsNullOrWhiteSpace(text))
			{
				return result.AsReadOnly();
			}

			result.Add(text.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault());

			return result;
		}

		public async Task<string> RecognizeHandwrittenTextAsync(Stream imageData)
		{
			var requestContent = new StreamContent(imageData);
			requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
			await _client.PostAsync("/recognizeText?handwriting=true", requestContent).ConfigureAwait(false);

			var response = await _client.PostAsync("/vision/v1.0/recognizeText?handwriting=true", requestContent);
			var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			return "";
		}

		public async Task<string> GetImageUrlAsync(string description)
		{
			await Task.Delay(500);
			return "https://s3.amazonaws.com/blog.xamarin.com/wp-content/uploads/2016/02/24100131/xamarin-joins-microsoft.png";
		}

		int _index;
	}
}
