using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XTransportTest.Client;

namespace XTransportTest
{
	[TestClass]
	public class TestGetIsRedoEnabled : AbstractTest
	{
		[TestMethod]
		public void GetIsRedoEnabled0()
		{
			var cl = new TstClient();
			var a = cl.GetRoot<Root>().AItems[0];
			Wait();
			a.Value = 10;
			cl.Undo(a.Uid);
			Assert.AreEqual(true, cl.GetIsRedoEnabled(a.Uid));
			cl.Redo(a.Uid);
			Assert.AreEqual(false, cl.GetIsRedoEnabled(a.Uid));
		}
	}

	[TestClass]
	public class TestRedo : AbstractTest
	{
		[TestMethod]
		public void Redo()
		{
			var cl = new TstClient();
			var a = cl.GetRoot<Root>().AItems[0];
			Wait();
			a.Value = 10;
			Wait();
			a.Value = 20;
			Wait();
			cl.Undo(a.Uid);
			cl.Undo(a.Uid);
			cl.Redo(a.Uid);
			Assert.AreEqual(10, a.Value);
			cl.Redo(a.Uid);
			Assert.AreEqual(20, a.Value);
		}

		[TestMethod]
		public void AddUpdateAndRedo()
		{
			var cl = new TstClient();
			var root = cl.GetRoot<Root>();
			var cnt = root.BItems.Count;
			root.BItems.Add(new B { Value = "A" });
			Wait();
			root.BItems.Last().Value = "AA";
			Wait();
			cl.Undo(root.Uid);
			Assert.AreEqual("A", root.BItems.Last().Value);
			Assert.AreEqual(cnt + 1, root.BItems.Count);
			Wait();
			cl.Undo(root.Uid);
			Assert.AreEqual(cnt, root.BItems.Count);
			cl.Redo(root.Uid);
			Assert.AreEqual("A", root.BItems.Last().Value);
			Assert.AreEqual(cnt + 1, root.BItems.Count);
			cl.Redo(root.Uid);
			Assert.AreEqual("AA", root.BItems.Last().Value);
		}

		[TestMethod]
		public void ListRedo()
		{
			var cl = new TstClient();
			var root = cl.GetRoot<Root>();

			var cnt = root.AItems.Count;
			root.AItems.Add(new A());

			cl.Undo(root.Uid);
			Assert.AreEqual(cnt, root.AItems.Count);

			cl.Redo(root.Uid);
			Assert.AreEqual(cnt+1, root.AItems.Count);
		}

		[TestMethod]
		public void ListRedo1()
		{
			var cl = new TstClient();
			var root = cl.GetRoot<Root>();

			var cnt = root.AItems.Count;
			root.AItems.Add(new A {Value = 5});
			Wait();
			root.AItems.Last().Value = 10;
			cl.Undo(root.Uid);
			Assert.AreEqual(5, root.AItems.Last().Value);
			cl.Undo(root.Uid);
			Assert.AreEqual(cnt, root.AItems.Count);
			cl.Redo(root.Uid);
			Assert.AreEqual(cnt + 1, root.AItems.Count);
			Assert.AreEqual(5, root.AItems.Last().Value);
			cl.Redo(root.Uid);
			Assert.AreEqual(10, root.AItems.Last().Value);
		}
	}
}