using System;

namespace PicturePostcard.Shared
{
	public class BingImageSearchResult
	{
		public string Name { get; set; }
		public string WebSearchUrl { get; set; }
		public string ThumbnailUrl { get; set; }
		public DateTime DatePublished { get; set; }
		public string ContentUrl { get; set; }
		public string HostPageUrl { get; set; }
		public string ContentSize { get; set; }
		public string EncodingFormat { get; set; }
		public string HostPageDisplayUrl { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public ThumbnailInfo Thumbnail { get; set; }
		public string ImageInsightsToken { get; set; }
		public InsightsSourcesSummaryInfo InsightsSourcesSummary { get; set; }
		public string ImageId { get; set; }
		public string AccentColor { get; set; }

		public class ThumbnailInfo
		{
			public int Width { get; set; }
			public int Height { get; set; }
		}

		public class InsightsSourcesSummaryInfo
		{
			public int ShoppingSourcesCount { get; set; }
			public int RecipeSourcesCount { get; set; }
		}
	}
}
