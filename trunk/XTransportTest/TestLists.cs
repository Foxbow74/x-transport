using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XTransportTest.Client;

namespace XTransportTest
{
	[TestClass]
	public class TestLists : AbstractTest
	{
		[TestMethod]
		public void GetList()
		{
			var cl = new TstClient();
			var pr = cl.GetRoot<Root>().ParentItems.First();
			Assert.AreEqual(2, pr.Children.Count);
		}

		[TestMethod]
		public void Parent()
		{
			var cl = new TstClient();
			var pr = cl.GetRoot<Root>().ParentItems.First();
			foreach (var child in pr.Children)
			{
				Assert.AreEqual(pr, child.Parent);
			}
		}

		[TestMethod]
		public void Add()
		{
			var cl = new TstClient();
			var pr = cl.GetRoot<Root>().ParentItems.First();
			var cnt = pr.Children.Count;
			var child = new Child {Value = 3.0};
			pr.Children.Add(child);
			Assert.AreEqual(cnt+1, pr.Children.Count);
			Assert.AreEqual(pr, child.Parent);
		}

		[TestMethod]
		public void Undo()
		{
			var cl = new TstClient();
			var pr = cl.GetRoot<Root>().ParentItems.First();
			Wait();
			var clientSideChild = new Child {Value = 3.0};
			pr.Children.Add(clientSideChild);
			cl.Undo(pr.Uid);
			Assert.AreEqual(2, pr.Children.Count);
		}

		[TestMethod]
		public void Redo()
		{
			var cl = new TstClient();
			var pr = cl.GetRoot<Root>().ParentItems.First();
			var child = new Child {Value = 3.0};
			Wait();
			var cnt = pr.Children.Count;
			pr.Children.Add(child);
			cl.Undo(pr.Uid);
			Assert.AreEqual(cnt, pr.Children.Count);
			cl.Redo(pr.Uid);
			Assert.AreEqual(true, pr.Children.SingleOrDefault(_child => _child.Uid == child.Uid) != null);
		}
	}
}