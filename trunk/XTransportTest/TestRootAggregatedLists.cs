using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XTransportTest.Client;

namespace XTransportTest
{
	[TestClass]
	public class TestRootAggregatedLists : AbstractTest
	{
		[TestMethod]
		public void Test1()
		{
			var cl = new TstClient();
			var root = cl.GetRoot<Root>();
			var allItems = root.AItems.Cast<XObject>().Union(root.BItems).Union(root.ParentItems).Union(root.RefItems).ToList();
			Assert.AreEqual(allItems.Count, root.All.Count);
		}

		[TestMethod]
		public void AddToAggregated()
		{
			var cl = new TstClient();
			var root = cl.GetRoot<Root>();
			var a = new A { Value = 99 };
			var cnt = root.AItems.Count;
			root.All.Add(a);
			Assert.IsTrue(root.AItems.Contains(a));
			Assert.AreEqual(cnt + 1, root.AItems.Count);
		}

		[TestMethod]
		public void AddToSimple()
		{
			var cl = new TstClient();
			var root = cl.GetRoot<Root>();
			var a = new A { Value = 99 };
			var cnt = root.All.Count;
			root.AItems.Add(a);
			Assert.IsTrue(root.All.Contains(a));
			Assert.AreEqual(cnt + 1, root.All.Count);
		}
	}
}