using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PicturePostcard.Shared
{
    public interface IEmotional
    {
        // https://westus.dev.cognitive.microsoft.com/docs/services/56f91f2d778daf23d8ec6739/operations/587f2c6a154055056008f200
        Task<string> RecognizeHandwrittenTextAsync(Stream imageData);

        // 
        Task<Sentiment> AnalyzeSentimentAsync(string text);

		// https://docs.microsoft.com/en-us/azure/cognitive-services/text-analytics/how-tos/text-analytics-how-to-keyword-extraction
		Task<IReadOnlyList<string>> GetKeyPhrasesAsync(string text);

		// https://docs.microsoft.com/en-us/azure/cognitive-services/bing-image-search/tutorial-bing-image-search-single-page-app
		// https://docs.microsoft.com/en-gb/azure/cognitive-services/computer-vision/quickstarts/csharp#GetThumbnail
		Task<string> GetImageUrlAsync(string description);
    }
}