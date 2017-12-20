using System.Collections.Generic;

namespace PicturePostcard.Shared
{
	public class SentimentRecognitionResult
    {
		public IList<Document> Documents { get; set; }
		public IList<object> Errors { get; set; }

		public class Document
		{
			public double Score { get; set; }
			public string Id { get; set; }
		}
	}
}
