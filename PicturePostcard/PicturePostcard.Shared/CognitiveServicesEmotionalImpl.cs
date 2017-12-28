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
		const string COMPUTER_VISION_API_KEY = "ae7d3af7d10e4bb7b0acd702c04f6d43";
		const string TEXT_ANALYTICS_API_KEY = "64cf0bd55bbe49d5bff32aa851a2d204";
		const string BING_SEARCH_API_KEY = "9401de7e550b452799264cef767219bf";
		
		// Trial keys only work for the West Central US region.
		// This base URL works for most of the services. Bing Search uses a different one.
		const string COGNITIVE_SERVICES_BASE_URL = "https://westcentralus.api.cognitive.microsoft.com/";

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

			return Sentiment.Positive;
		}

		public async Task<IReadOnlyList<string>> GetKeyPhrasesAsync(string text)
		{
			// Quickstart: https://docs.microsoft.com/en-us/azure/cognitive-services/text-analytics/how-tos/text-analytics-how-to-keyword-extraction
			// The service supports up to 50.000 items. We only use one here.
			var requestJson = "{ \"documents\": [ { \"language\": \"en\", \"id\": \"1\", \"text\": \"" + text + "\" } ] }";

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

			var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			var recognitionResult = JsonConvert.DeserializeObject<KeyPhrasesRecognitionResult>(responseJson);

			if (recognitionResult.Errors.Count() > 0)
			{
				return null;
			}

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
			using (var requestContent = new StreamContent(imageData))
			{
				requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
				requestContent.Headers.Add("Ocp-Apim-Subscription-Key", COMPUTER_VISION_API_KEY);
				response = await _client.PostAsync("vision/v1.0/recognizeText?handwriting=true", requestContent).ConfigureAwait(false);
			}
			
			// The response will be a URL where we can pick up the recognition result.
			string operationLocation = null;
			if (response.IsSuccessStatusCode)
			{
				if (response.Headers.TryGetValues("Operation-Location", out IEnumerable<string> headerValues))
				{
					operationLocation = headerValues.FirstOrDefault();
				}
			}

			// Keep on polling the URL until the service has completed processing.
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

			// Deserialize the returned JSON.
			// Example format of successful result:
			/*
			{
				"status": "Succeeded",
				"recognitionResult":
				{
					"lines":
					[
						{
							"boundingBox": [84, 119, 280, 122, 279, 174, 83, 171],
							"text": "HELLO",
							"words":
							[
								{
									"boundingBox": [97, 120, 273, 120, 271, 172, 95, 172],
									"text": "HELLO"
								}]
						}
					]
				}
			}
			*/
			var deserialized = JsonConvert.DeserializeObject<HandWritingRecognitionResult>(recognizedData);

			return deserialized.CompleteText;
		}

		public async Task<string> GetImageUrlAsync(string description)
		{
			// Quickstart: https://docs.microsoft.com/en-us/azure/cognitive-services/bing-image-search/quick-start

			// Bing image search is using a different base URL.
			var requestMessage = new HttpRequestMessage(HttpMethod.Get, 
				$"https://api.cognitive.microsoft.com/bing/v7.0/images/search?q={HttpUtility.UrlEncode(description)}&license=share&safeSearch=strict");
			requestMessage.Headers.Add("Ocp-Apim-Subscription-Key", BING_SEARCH_API_KEY);
			var response = await _client.SendAsync(requestMessage).ConfigureAwait(false);
			var resultJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			// Example response:
			//{
			//	"_type":"Images",
			//	"instrumentation":{},
			//	"readLink":"https:\/\/api.cognitive.microsoft.com\/api\/v7\/images\/search?q=HALLO",
			//	"webSearchUrl":"https:\/\/www.bing.com\/images\/search?q=HALLO&FORM=OIIARP",
			//	"totalEstimatedMatches":879,
			//	"nextOffset":38,
			//	"value":[
			//		{
			//			"webSearchUrl":"https:\/\/www.bing.com\/images\/search?view=detailv2&FORM=OIIRPO&q=HALLO&id=12C4A2B0BDC20F6FE47062CDE8748E16BD00C7FE&simid=608016987088225815",
			//			"name":"Hallo UV Licht - Design Nation",
			//			"thumbnailUrl":"https:\/\/tse3.mm.bing.net\/th?id=OIP.plfFGnf0n4BO7jJlSeIffwHaLH&pid=Api",
			//			"datePublished":"2011-10-11T03:59:00.0000000Z",
			//			"contentUrl":"http:\/\/www.designnation.de\/Media\/Galerie\/4d6eb4545d7e9,Hallo-UV-Licht.jpg",
			//			"hostPageUrl":"http:\/\/galerie.designnation.de\/bild\/52838",
			//			"contentSize":"271464 B",
			//			"encodingFormat":"jpeg",
			//			"hostPageDisplayUrl":"galerie.designnation.de\/bild\/52838",
			//			"width":1245,
			//			"height":830,
			//			"thumbnail":{
			//				"width":474,
			//				"height":711
			//			},
			//			"imageInsightsToken":"ccid_plfFGnf0*mid_12C4A2B0BDC20F6FE47062CDE8748E16BD00C7FE*simid_608016987088225815*thid_OIP.plfFGnf0n4BO7jJlSeIffwHaLH",
			//			"insightsMetadata":{
			//				"pagesIncludingCount":4,
			//				"availableSizesCount":1
			//			},
			//			"imageId":"12C4A2B0BDC20F6FE47062CDE8748E16BD00C7FE",
			//			"accentColor":"4A00CC"
			//		},
			//		[......]
			//	]
			//}
			 

			var images = JsonConvert.DeserializeObject<BingImageSearchResult>(resultJson);
			var imageData = images?.Value?.FirstOrDefault();
			return imageData?.ContentUrl;
		}
	}
}
