using NUnit.Framework;
using PicturePostcard.Shared;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PicturePostCard.Tests
{
	/// <summary>
	/// Component tests for the Azure Cognitive Services implementation of the
	/// IEmotional interface.
	/// </summary>
	[TestFixture]
	public class TestCognitiveServicesImpl
	{
		IEmotional _emotional = new CognitiveServicesEmotionalImpl();

		[Test]
		public async Task RecognizingHandWrittenText_ShouldSucceed()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var imageStream = assembly.GetManifestResourceStream("PicturePostcard.UnitTests.testhandwriting.jpg");

			var text = await _emotional.RecognizeHandwrittenTextAsync(imageStream);
			imageStream.Dispose();

			// Yeah...I know...my handwriting :-)
			Assert.AreEqual("you if worry be happy", text.ToLowerInvariant());
		}

		[Test]
		public async Task AnalyzeSentiment_ShouldReturnPositive()
		{
			var sentiment = await _emotional.AnalyzeSentimentAsync("don't worry,  be happy");
			Assert.AreEqual(Sentiment.Positive, sentiment);
		}

		[Test]
		public async Task GetKeyPhrases_ShouldReturnHappy()
		{
			var keyPhrases = await _emotional.GetKeyPhrasesAsync("Happiness is what makes the world go round.");
			var mainKeyPhrase = keyPhrases.FirstOrDefault();
			Assert.AreEqual("Happiness", mainKeyPhrase);
		}

		[Test]
		public async Task GetImage_ShouldReturnUrl()
		{
			var imageUrl = await _emotional.GetImageUrlAsync("happiness");
			Assert.IsNotNull(imageUrl);
		}

	}
}
