using System.Collections.Generic;

namespace PicturePostcard.Shared
{
	public class KeyPhrasesRecognitionResult
	{
		public IList<Document> Documents { get; set; }
		public IList<object> Errors { get; set; }
	}

	public class Document
	{
		public IList<string> KeyPhrases { get; set; }
		public string Id { get; set; }
	}
}
