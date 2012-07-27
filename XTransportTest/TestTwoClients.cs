using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XTransportTest.Client;

namespace XTransportTest
{
	[TestClass]
	public class TestTwoClients : AbstractTest
	{
		[TestMethod]
		public void Simple()
		{
			var cl1 = new TstClient();
			var cl2 = new TstClient();
			var a = cl1.GetRoot<Root>().AItems.First();
			a.Value = 10;
			var am = cl2.Get<AMirror>(a.Uid);
			Assert.AreNotEqual(a.Value, am.Value);
		}

		[TestMethod]
		public void Saving1()
		{
			var cl1 = new TstClient();
			var cl2 = new TstClient();
			var a = cl1.GetRoot<Root>().AItems.First();
			a.Value = 10;
			cl1.Save(a.Uid);
			var am = cl2.Get<AMirror>(a.Uid);
			Assert.AreEqual(a.Value, am.Value);
		}

		[TestMethod]
		public void SaveAndRevert()
		{
			var cl1 = new TstClient();
			var cl2 = new TstClient();
			var a = cl1.GetRoot<Root>().AItems.First();
			a.Value = 10;
			var am = cl2.Get<AMirror>(a.Uid);
			cl1.Save(a.Uid);
			Assert.AreNotEqual(a.Value, am.Value);
			cl2.Revert(a.Uid);
			Assert.AreEqual(a.Value, am.Value);
		}

		[TestMethod]
		public void SaveAndUndo()
		{
			var cl1 = new TstClient();
			var rt1 = cl1.GetRoot<Root>();

			var cl2 = new TstClient();
			var rt2 = cl2.GetRoot<Root>();

			Assert.AreEqual(rt1.RefItems.First().Ref, rt1.AItems.First());
			Assert.AreNotEqual(rt1.RefItems.First().Ref, rt1.AItems.ToList()[1]);
			rt1.RefItems.First().Ref = rt1.AItems.ToList()[1];
			rt2.RefItems.First().Ref = rt2.AItems.ToList()[1];
			cl1.Save(rt1.Uid);
			cl2.Undo(rt2.Uid);
			Assert.AreEqual(rt2.RefItems.First().Ref.Uid, rt2.AItems.First().Uid);
			Assert.AreEqual(rt2.RefItems.First().Ref, rt2.AItems.First());
		}

		[TestMethod]
		public void RemoveAndUndoList()
		{
			var cl1 = new TstClient();
			var rt1 = cl1.GetRoot<Root>();

			var cl2 = new TstClient();
			var rt2 = cl2.GetRoot<Root>();

			var cnt = rt1.AItems.Count;

			rt1.AItems.Remove(rt1.AItems.First());
			Wait(1);
			rt2.AItems.Remove(rt2.AItems.First());
			Wait(1);
			cl1.Save(rt1.Uid);
			Wait(1000, () => rt1.IsDirty);
			cl2.Undo(rt2.Uid);
			Assert.AreEqual(cnt, rt2.AItems.Count);
			cl2.Undo(rt2.Uid);
			Assert.AreEqual(rt1.AItems.Count, rt2.AItems.Count);
		}

		[TestMethod]
		public void AddAndUndoList()
		{
			var cl1 = new TstClient();
			var rt1 = cl1.GetRoot<Root>();

			var cl2 = new TstClient();
			var rt2 = cl2.GetRoot<Root>();

			var aValue = rt1.AItems.Last().Value;

			rt1.AItems.Add(new A {Value = 100});
			Wait(1);
			rt2.AItems.Add(new A {Value = 200});
			Wait(1);
			cl1.Save(rt1.Uid);
			Wait(1000, () => rt1.IsDirty);
			cl2.Undo(rt2.Uid);
			Assert.AreEqual(aValue, rt2.AItems.Last().Value);
			cl2.Undo(rt2.Uid);
			Assert.AreEqual(rt1.AItems.Last().Value, rt2.AItems.Last().Value);
		}
	}
}