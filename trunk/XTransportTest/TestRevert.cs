using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XTransportTest.Client;

namespace XTransportTest
{
	[TestClass]
	public class TestRevert : AbstractTest
	{
		[TestMethod]
		public void IsRevertable()
		{
			var cl = new TstClient();
			var a = cl.GetRoot<Root>().AItems.First();
			a.Value = 10;
			Assert.AreEqual(true, cl.GetIsRevertEnabled(a.Uid));
		}

		[TestMethod]
		public void IsRevertable1()
		{
			var cl = new TstClient();
			var a = cl.GetRoot<Root>().AItems.First();
			a.Value = 10;
			cl.Revert(a.Uid);
			Assert.AreEqual(false, cl.GetIsRevertEnabled(a.Uid));
		}

		[TestMethod]
		public void IsRevertable2()
		{
			var cl = new TstClient();
			var a = cl.GetRoot<Root>().AItems.First();
			a.Value = 10;
			a.Value = 20;
			cl.Revert(a.Uid);
			Assert.AreEqual(false, cl.GetIsRevertEnabled(a.Uid));
		}


		[TestMethod]
		public void Revert()
		{
			var cl1 = new TstClient();
			var a = cl1.GetRoot<Root>().AItems.First();
			a.Value = 10;
			cl1.Save(a.Uid);
			a.Value = 20;
			cl1.Revert(a.Uid);
			Assert.AreEqual(10, a.Value);
		}
	}
}