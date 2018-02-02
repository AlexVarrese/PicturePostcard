using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PicturePostcard.Shared
{
	public class TestEmotionalImpl : IEmotional
	{
		public async Task<Sentiment> AnalyzeSentimentAsync(string text)
		{
			await Task.Delay(500);

			if(string.IsNullOrWhiteSpace(text))
			{
				return Sentiment.Unknown;
			}

			if(new [] { "happy", "nice", "lucky" }.Any(x => text.ToLower().Contains(x)))
			{
				return Sentiment.Positive;
			}

			if (new[] { "bad", "fail", "miserable" }.Any(x => text.ToLower().Contains(x)))
			{
				return Sentiment.Negative;
			}

			return Sentiment.Normal;
		}

		public async Task<IReadOnlyList<string>> GetKeyPhrasesAsync(string text)
		{
			await Task.Delay(500);
			var result = new List<string>();
			if(string.IsNullOrWhiteSpace(text))
			{
				return result.AsReadOnly();
			}

			result.Add(text.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault());

			return result;
		}

		public async Task<string> RecognizeHandwrittenTextAsync(Stream imageData)
		{
			await Task.Delay(500);

			return new[] {
				"don't worry, be happy",
				"The funniest people are the saddest ones",
				"Things could be worse"
			}[(_index++) % 3];
		}

		public async Task<string> GetImageUrlAsync(string description)
		{
			await Task.Delay(500);
			return "https://s3.amazonaws.com/blog.xamarin.com/wp-content/uploads/2016/02/24100131/xamarin-joins-microsoft.png";
		}

		int _index;
	}
}
