using System;
using System.Collections.Generic;
using System.Linq;

namespace PicturePostcard.Shared
{
    public class HandWritingRecognitionResult
    {
		/// <summary>
		/// Helper property to return all of the recognized text.
		/// </summary>
		public string CompleteText => string.Join(" ", RecognitionResult?.Lines?.Select(l => l.Text));

		public string Status { get; set; }
		public RecognizedLines RecognitionResult { get; set; }

		public class Word
		{
			public IList<int> BoundingBox { get; set; }
			public string Text { get; set; }
		}

		public class Line
		{
			public IList<int> BoundingBox { get; set; }
			public string Text { get; set; }
			public IList<Word> Words { get; set; }
		}

		public class RecognizedLines
		{
			public IList<Line> Lines { get; set; }
		}
		
	}
}
