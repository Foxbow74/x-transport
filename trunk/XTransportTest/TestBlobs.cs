using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XTransportTest.Client;
using XTransportTest.Server;

namespace XTransportTest
{
	[TestClass]
	public class TestBlobs : AbstractTest
	{
		[TestMethod]
		public void SaveLoad()
		{
			var cl = new TstClient();
			var a = cl.GetRoot<Root>().AItems.First();
			a.Bytes = new byte[]{1,2,3};
			cl.Save(a);

			TstServer.Instance.Reset();

			var cl1 = new TstClient();
			var rt1 = cl1.GetRoot<Root>();
			var a1 = cl1.GetRoot<Root>().AItems.First();
			Assert.AreEqual(3, a1.Bytes.Length);
		}
	}
}