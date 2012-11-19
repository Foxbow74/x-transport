using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XTransportTest.Client;
using XTransportTest.ClientVM;

namespace XTransportTest
{
	[TestClass]
	public class TestGetIsUndoEnabled : AbstractTest
	{
		[TestMethod]
		public void GetIsUndoEnabledByDefault()
		{

			var cl = new TstClient();
			var a = cl.GetRoot<RootVM>().AItems.First();
			Assert.AreEqual(false, cl.GetIsUndoEnabled(a.Uid));
		}

		[TestMethod]
		public void GetRootIsUndoEnabledByDefault()
		{
			var cl = new TstClient();
			var a = cl.GetRoot<RootVM>().AItems.First();
			Assert.AreEqual(false, cl.GetIsUndoEnabled(a));
		}

		[TestMethod]
		public void GetIsUndoEnabledAfterChanges()
		{
			var cl = new TstClient();
			var a = cl.GetRoot<RootVM>().AItems.First();
			a.Value = 10;
			Assert.AreEqual(true, cl.GetIsUndoEnabled(a.Uid));
		}

		[TestMethod]
		public void GetIsUndoEnabledAfterChangesAndUndo()
		{
			var cl = new TstClient();
			var a = cl.GetRoot<RootVM>().AItems.First();
			a.Value = 10;
			Assert.AreEqual(true, cl.GetIsUndoEnabled(a.Uid));
			a.Value = 20;
			Assert.AreEqual(true, cl.GetIsUndoEnabled(a.Uid));
			cl.Undo(a.Uid);
			Assert.AreEqual(true, cl.GetIsUndoEnabled(a.Uid));
			cl.Undo(a.Uid);
			Assert.AreEqual(false, cl.GetIsUndoEnabled(a.Uid));
		}

		[TestMethod]
		public void GetIsUndoEnabledAfterChangesAndSave()
		{
			var cl = new TstClient();
			var a = cl.GetRoot<RootVM>().AItems.First();
			a.Value = 10;
			Assert.AreEqual(true, cl.GetIsUndoEnabled(a.Uid));
			cl.Save(a.Uid);
			Assert.AreEqual(false, cl.GetIsUndoEnabled(a.Uid));
		}

		[TestMethod]
		public void ListGetIsUndoEnabledAfterAddItemAndSave()
		{
			var cl = new TstClient();
			var root = cl.GetRoot<RootVM>();
			root.AItems.Add(new Avm {Value = 99});
			Assert.AreEqual(true, cl.GetIsUndoEnabled(root.Uid));
			cl.Save(root.Uid);
			Assert.AreEqual(false, cl.GetIsUndoEnabled(root.Uid));
		}

		[TestMethod]
		public void ListGetIsUndoEnabledAfterDeleteItemAndSave()
		{
			var cl = new TstClient();
			var root = cl.GetRoot<RootVM>();
			root.AItems.Remove(root.AItems.First());
			Assert.AreEqual(true, cl.GetIsUndoEnabled(root.Uid));
			cl.Save(root.Uid);
			Assert.AreEqual(false, cl.GetIsUndoEnabled(root.Uid));
		}
	}
}