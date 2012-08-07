using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XTransportTest.Client;

namespace XTransportTest
{
	[TestClass]
	public class TestRedo : AbstractTest
	{
		[TestMethod]
		public void Redo1()
		{
			var cl = new TstClient();
			var a = cl.GetRoot<Root>().AItems.First();
			var init = a.Value;
			a.Value = 10;
			a.Value = 20;
			cl.Undo(a.Uid);
			Assert.AreEqual(10, a.Value);
			cl.Undo(a.Uid);
			Assert.AreEqual(init, a.Value);
			cl.Redo(a.Uid);
			Assert.AreEqual(10, a.Value);
			cl.Redo(a.Uid);
			Assert.AreEqual(20, a.Value);
		}

		[TestMethod]
		public void Redo2()
		{
			var cl = new TstClient();
			var a = cl.GetRoot<Root>().AItems.First();
			var init = a.Value;
			a.Value = 10;
			a.Value = init;
			cl.Undo(a.Uid);
			Assert.AreEqual(10, a.Value);
			cl.Undo(a.Uid);
			Assert.AreEqual(init, a.Value);
			cl.Redo(a.Uid);
			Assert.AreEqual(10, a.Value);
			cl.Redo(a.Uid);
			Assert.AreEqual(init, a.Value);
		}

		[TestMethod]
		public void AddUpdateAndRedo()
		{
			var cl = new TstClient();
			var root = cl.GetRoot<Root>();
			var cnt = root.BItems.Count;
			root.BItems.Add(new B {Value = "A"});
			root.BItems.Last().Value = "AA";
			cl.Undo(root.Uid);
			Assert.AreEqual("A", root.BItems.Last().Value);
			Assert.AreEqual(cnt + 1, root.BItems.Count);
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
			Assert.AreEqual(cnt + 1, root.AItems.Count);
		}

		[TestMethod]
		public void ListRedo1()
		{
			var cl = new TstClient();
			var root = cl.GetRoot<Root>();

			var cnt = root.AItems.Count;
			root.AItems.Add(new A {Value = 5});
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