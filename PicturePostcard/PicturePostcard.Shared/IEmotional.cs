using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PicturePostcard.Shared
{
	/// <summary>
	/// Interface declaration for demonstration of Azure Cognitive Services.
	/// </summary>
	public interface IEmotional
	{
		/// <summary>
		/// Recognizes handwritten text.
		/// </summary>
		/// <param name="imageData">image data as a stream</param>
		/// <returns>recognied text or null if nothing was recognized</returns>
		Task<string> RecognizeHandwrittenTextAsync(Stream imageData);

		
		/// <summary>
		/// Analyzes the sentiment of a given text.
		/// </summary>
		/// <param name="text">text to analyze</param>
		/// <returns>the sentiment</returns>
		Task<Sentiment> AnalyzeSentimentAsync(string text);

		/// <summary>
		/// Extracts the key phrases of a text.
		/// </summary>
		/// <param name="text">text to analyze</param>
		/// <returns>list of key phrases</returns>
		Task<IReadOnlyList<string>> GetKeyPhrasesAsync(string text);

		/// <summary>
		/// Returns the url of an image matching the given description.
		/// </summary>
		/// <param name="description">search term</param>
		/// <returns>URL of image or null if no image was found</returns>
		Task<string> GetImageUrlAsync(string description);
	}
}