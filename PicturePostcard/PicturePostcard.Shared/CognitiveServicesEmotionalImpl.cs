using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PicturePostcard.Shared
{
	public class CognitiveServicesEmotionalImpl : IEmotional
	{
		// Get a trial key at https://azure.microsoft.com/en-us/try/cognitive-services/
		// or use youe Azure subscription to create a "Cognitive Services" resource.
		const string COMPUTER_VISION_API_KEY = "32b240209d704902b4047bab61846c08";
		const string TEXT_ANALYTICS_API_KEY = "d0b30a4079eb4584b085d1f695be48f3";
		const string BING_SEARCH_API_KEY = "9204b829ac5646eeb6c603d3a318e3c7";

		// Trial keys only work for the West Central US region (https://westcentralus.api.cognitive.microsoft.com/).
		// This base URL works for most of the services. Bing Search uses a different one.
		const string COGNITIVE_SERVICES_BASE_URL = "https://westeurope.api.cognitive.microsoft.com";

		// All services are RESTful.
		readonly HttpClient _client = new HttpClient
		{
			BaseAddress = new Uri(COGNITIVE_SERVICES_BASE_URL),
			Timeout = TimeSpan.FromSeconds(60),
		};
		
		public async Task<Sentiment> AnalyzeSentimentAsync(string text)
		{
			// Quickstart: https://docs.microsoft.com/en-us/azure/cognitive-services/text-analytics/how-tos/text-analytics-how-to-sentiment-analysis
			// The service supports up to 50.000 items. We only use one here.
			var requestJson = "{ \"documents\": [ { \"language\": \"en\", \"id\": \"1\", \"text\": \"" + text + "\" } ] }";

			#region REQUEST
			HttpResponseMessage response;
			using (var requestContent = new StringContent(requestJson))
			{
				requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
				requestContent.Headers.Add("Ocp-Apim-Subscription-Key", TEXT_ANALYTICS_API_KEY);

				response = await _client.PostAsync("text/analytics/v2.0/sentiment", requestContent).ConfigureAwait(false);
			}

			if(!response.IsSuccessStatusCode)
			{
				return Sentiment.Unknown;
			}
			#endregion

			#region PARSE
			var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			var recognitionResult = JsonConvert.DeserializeObject<SentimentRecognitionResult>(responseJson);

			if(recognitionResult.Errors.Count() > 0)
			{
				return Sentiment.Unknown;
			}

			var score = recognitionResult.Documents.FirstOrDefault()?.Score;

			if (score <= 0.3)
			{
				return Sentiment.Negative;
			}

			if (score <= 0.6)
			{
				return Sentiment.Normal;
			}
			#endregion

			return Sentiment.Positive;
		}

		public async Task<IReadOnlyList<string>> GetKeyPhrasesAsync(string text)
		{
			// Quickstart: https://docs.microsoft.com/en-us/azure/cognitive-services/text-analytics/how-tos/text-analytics-how-to-keyword-extraction
			// The service supports up to 50.000 items. We only use one here.
			var requestJson = "{ \"documents\": [ { \"language\": \"en\", \"id\": \"1\", \"text\": \"" + text + "\" } ] }";

			#region REQUEST
			HttpResponseMessage response;
			using (var requestContent = new StringContent(requestJson))
			{
				requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
				requestContent.Headers.Add("Ocp-Apim-Subscription-Key", TEXT_ANALYTICS_API_KEY);

				response = await _client.PostAsync("text/analytics/v2.0/keyPhrases", requestContent).ConfigureAwait(false);
			}

			if (!response.IsSuccessStatusCode)
			{
				return null;
			}
			#endregion

			#region PARSE
			var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			var recognitionResult = JsonConvert.DeserializeObject<KeyPhrasesRecognitionResult>(responseJson);

			if (recognitionResult.Errors.Count() > 0)
			{
				return null;
			}
			#endregion

			var keyPhrases = recognitionResult.Documents?.FirstOrDefault()?.KeyPhrases?.ToList();

			return keyPhrases;
		}
		

		public async Task<string> RecognizeHandwrittenTextAsync(Stream imageData)
		{
			// Quickstart: https://docs.microsoft.com/en-us/azure/cognitive-services/computer-vision/quickstarts/csharp#RecognizeText

			if (imageData == null)
			{
				return null;
			}

			HttpResponseMessage response;

			// Send image data to the recognition service.
			#region SENDIMAGE
			using (var requestContent = new StreamContent(imageData))
			{
				requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
				requestContent.Headers.Add("Ocp-Apim-Subscription-Key", COMPUTER_VISION_API_KEY);

				try
				{
					response = await _client.PostAsync("vision/v1.0/recognizeText?handwriting=true", requestContent).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"Failed to post request. Error: {ex.Message}");
					throw;
				}
			}
			#endregion

			// The response will be a URL where we can pick up the recognition result.
			#region GETRESULTLOCATION
			string operationLocation = null;
			if (response.IsSuccessStatusCode)
			{
				if (response.Headers.TryGetValues("Operation-Location", out IEnumerable<string> headerValues))
				{
					operationLocation = headerValues.FirstOrDefault();
				}
			}
			#endregion

			// Keep on polling the URL until the service has completed processing.
			#region GETRESULT
			string recognizedData = null;
			if (operationLocation != null)
			{
				while (true)
				{
					try
					{
						await Task.Delay(3000).ConfigureAwait(false);
						var requestMessage = new HttpRequestMessage(HttpMethod.Get, operationLocation);
						requestMessage.Headers.Add("Ocp-Apim-Subscription-Key", COMPUTER_VISION_API_KEY);
						response = await _client.SendAsync(requestMessage).ConfigureAwait(false);
						recognizedData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
						break;
					}
					catch (TaskCanceledException)
					{
						Debug.WriteLine("Image has not been processed yet. Retrying...");
					}
				}
			}
			#endregion
			
			var deserialized = JsonConvert.DeserializeObject<HandWritingRecognitionResult>(recognizedData);

			return deserialized.CompleteText;
		}
		
		public async Task<string> GetImageUrlAsync(string description)
		{
			// Quickstart: https://docs.microsoft.com/en-us/azure/cognitive-services/bing-image-search/quick-start

			// Bing image search is using a different base URL.
			#region REQUEST
			var requestMessage = new HttpRequestMessage(HttpMethod.Get, 
				$"https://api.cognitive.microsoft.com/bing/v7.0/images/search?q={HttpUtility.UrlEncode(description)}&license=share&safeSearch=strict");
			requestMessage.Headers.Add("Ocp-Apim-Subscription-Key", BING_SEARCH_API_KEY);
			var response = await _client.SendAsync(requestMessage).ConfigureAwait(false);
			#endregion

			#region PARSE
			var resultJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			
			var images = JsonConvert.DeserializeObject<BingImageSearchResult>(resultJson);
			var imageData = images?.Value?.FirstOrDefault();
			return imageData?.ContentUrl;
			#endregion
		}
	}
}
