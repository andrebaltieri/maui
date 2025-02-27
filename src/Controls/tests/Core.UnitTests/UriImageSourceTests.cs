using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using IOPath = System.IO.Path;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class UriImageSourceTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			networkcalls = 0;
		}

		static Random rnd = new Random();
		static int networkcalls = 0;
		static async Task<Stream> GetStreamAsync(Uri uri, CancellationToken cancellationToken)
		{
			await Task.Delay(rnd.Next(30, 2000));
			if (cancellationToken.IsCancellationRequested)
				throw new TaskCanceledException();
			networkcalls++;
			return typeof(UriImageSourceTests).Assembly.GetManifestResourceStream(uri.LocalPath.Substring(1));
		}

		[Test]
		[Ignore("LoadImageFromStream")]
		public void LoadImageFromStream()
		{
			IStreamImageSource loader = new UriImageSource
			{
				Uri = new Uri("http://foo.com/Images/crimson.jpg"),
			};
			Stream s0 = loader.GetStreamAsync().Result;

			Assert.AreEqual(79109, s0.Length);
		}

		[Test]
		[Ignore("SecondCallLoadFromCache")]
		public void SecondCallLoadFromCache()
		{
			IStreamImageSource loader = new UriImageSource
			{
				Uri = new Uri("http://foo.com/Images/crimson.jpg"),
			};
			Assert.AreEqual(0, networkcalls);

			using (var s0 = loader.GetStreamAsync().Result)
			{
				Assert.AreEqual(79109, s0.Length);
				Assert.AreEqual(1, networkcalls);
			}

			using (var s1 = loader.GetStreamAsync().Result)
			{
				Assert.AreEqual(79109, s1.Length);
				Assert.AreEqual(1, networkcalls);
			}
		}

		[Test]
		[Ignore("DoNotKeepFailedRetrieveInCache")]
		public void DoNotKeepFailedRetrieveInCache()
		{
			IStreamImageSource loader = new UriImageSource
			{
				Uri = new Uri("http://foo.com/missing.png"),
			};
			Assert.AreEqual(0, networkcalls);

			var s0 = loader.GetStreamAsync().Result;
			Assert.IsNull(s0);
			Assert.AreEqual(1, networkcalls);

			var s1 = loader.GetStreamAsync().Result;
			Assert.IsNull(s1);
			Assert.AreEqual(2, networkcalls);
		}

		[Test]
		[Ignore("ConcurrentCallsOnSameUriAreQueued")]
		public void ConcurrentCallsOnSameUriAreQueued()
		{
			IStreamImageSource loader = new UriImageSource
			{
				Uri = new Uri("http://foo.com/Images/crimson.jpg"),
			};
			Assert.AreEqual(0, networkcalls);

			var t0 = loader.GetStreamAsync();
			var t1 = loader.GetStreamAsync();

			//var s0 = t0.Result;
			using (var s1 = t1.Result)
			{
				Assert.AreEqual(1, networkcalls);
				Assert.AreEqual(79109, s1.Length);
			}
		}

		[Test]
		public void NullUriDoesNotCrash()
		{
			var loader = new UriImageSource();
			Assert.DoesNotThrow(() =>
			{
				loader.Uri = null;
			});
		}

		[Test]
		public void UrlHashKeyAreTheSame()
		{
			var urlHash1 = Crc64.ComputeHashString("http://www.optipess.com/wp-content/uploads/2010/08/02_Bad-Comics6-10.png?a=bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbasdasdasdasdasasdasdasdasdasd");
			var urlHash2 = Crc64.ComputeHashString("http://www.optipess.com/wp-content/uploads/2010/08/02_Bad-Comics6-10.png?a=bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbasdasdasdasdasasdasdasdasdasd");
			Assert.IsTrue(urlHash1 == urlHash2);
		}

		[Test]
		public void UrlHashKeyAreNotTheSame()
		{
			var urlHash1 = Crc64.ComputeHashString("http://www.optipess.com/wp-content/uploads/2010/08/02_Bad-Comics6-10.png?a=bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbasdasdasdasdasasdasdasdasdasd");
			var urlHash2 = Crc64.ComputeHashString("http://www.optipess.com/wp-content/uploads/2010/08/02_Bad-Comics6-10.png?a=bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbasdasdasdasdasasdasda");
			Assert.IsTrue(urlHash1 != urlHash2);
		}

	}
}
