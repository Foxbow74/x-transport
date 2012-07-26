using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XTransportTest.Client;
using XTransportTest.ClientVM;

namespace XTransportTest
{
	[TestClass]
	public class TestUndo : AbstractTest
	{
		[TestMethod]
		public void Undo0()
		{
			var cl = new TstClient();
			var a = cl.GetRoot<RootVM>().AItems.First();
			var init = a.Value;
			Wait(1);
			a.Value = 10;
			Wait(1);
			a.Value = 20;
			cl.Undo(a.Uid);
			Assert.AreEqual(10, a.Value);
			cl.Undo(a.Uid);
			Assert.AreEqual(init, a.Value);
		}

		[TestMethod]
		public void Undo1()
		{
			var cl = new TstClient();
			var rt = cl.GetRoot<Root>();
			var cnt = rt.AItems.Count;
			rt.AItems.Add(new A {Value = 1});
			Assert.IsTrue(cl.GetIsUndoEnabled(rt.Uid));
			cl.Undo(rt.Uid);
			Assert.AreEqual(cnt, rt.AItems.Count);
			Assert.IsFalse(cl.GetIsUndoEnabled(rt.Uid));
		}

		[TestMethod]
		public void Undo2()
		{
			var cl = new TstClient();
			var rt = cl.GetRoot<Root>();
			var cnt = rt.AItems.Count;
			var a = new A {Value = 1};
			rt.AItems.Add(a);
			Wait(1);
			a.Value = 2;
			cl.Undo(rt.Uid);
			Assert.AreEqual(1, a.Value);
			cl.Undo(rt.Uid);
			Assert.AreEqual(cnt, rt.AItems.Count);
		}

		[TestMethod]
		public void Undo3()
		{
			var cl = new TstClient();
			var rt = cl.GetRoot<Root>();
			var cnt = rt.AItems.Count;
			rt.AItems.Add(new A {Value = 99});
			Wait(1);
			rt.AItems.Last().Value = 100;
			Wait(1);
			rt.AItems.Add(new A {Value = 101});
			cl.Undo(rt.Uid);
			Assert.AreEqual(cnt + 1, rt.AItems.Count);
			Assert.AreEqual(100, rt.AItems.Last().Value);
			cl.Undo(rt.Uid);
			Assert.AreEqual(99, rt.AItems.Last().Value);
			cl.Undo(rt.Uid);
			Assert.AreEqual(cnt, rt.AItems.Count);
		}
	}
}