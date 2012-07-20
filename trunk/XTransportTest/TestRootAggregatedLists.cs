using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XTransportTest.Client;

namespace XTransportTest
{
	[TestClass]
	public class TestRootAggregatedLists : AbstractTest
	{
		[TestMethod]
		public void Simple()
		{
			var cl = new TstClient();
			var root = cl.GetRoot<Root>();
			var allItems = root.AItems.Cast<XObject>().Union(root.BItems).Union(root.ParentItems).Union(root.RefItems).ToList();
			Assert.AreEqual(allItems.Count, root.All.Count);
			foreach (var xObject in allItems)
			{
				Assert.IsTrue(root.All.Contains(xObject));
			}
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
			Assert.IsTrue(root.AB.Contains(a));
			Assert.AreEqual(cnt + 1, root.AItems.Count);
		}

		[TestMethod]
		public void AddToSimple()
		{
			var cl = new TstClient();
			var root = cl.GetRoot<Root>();
			var a = new A { Value = 99 };
			var cnt = root.All.Count;
			var cntab = root.AB.Count;
			root.AItems.Add(a);
			Assert.IsTrue(root.All.Contains(a));
			Assert.IsTrue(root.AB.Contains(a));
			Assert.AreEqual(cnt + 1, root.All.Count);
			Assert.AreEqual(cntab + 1, root.AB.Count);
		}


		[TestMethod]
		public void RemoveFromAggregated()
		{
			var cl = new TstClient();
			var root = cl.GetRoot<Root>();
			var a = root.AItems[0];
			var cnt = root.AItems.Count;
			root.All.Remove(a);
			Assert.IsFalse(root.AItems.Contains(a));
			Assert.IsFalse(root.AB.Contains(a));
			Assert.AreEqual(cnt - 1, root.AItems.Count);
		}

		[TestMethod]
		public void RemoveFromSimple()
		{
			var cl = new TstClient();
			var root = cl.GetRoot<Root>();
			var a = root.AItems[0];
			var cnt = root.All.Count;
			var cntab = root.AB.Count;
			root.AItems.Remove(a);
			Assert.IsFalse(root.All.Contains(a));
			Assert.IsFalse(root.AB.Contains(a));
			Assert.AreEqual(cnt - 1, root.All.Count);
			Assert.AreEqual(cntab - 1, root.AB.Count);
		}
	}
}