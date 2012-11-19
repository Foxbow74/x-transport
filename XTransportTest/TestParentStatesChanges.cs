using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XTransportTest.Client;

namespace XTransportTest
{
	[TestClass]
	public class TestParentStatesChanges : AbstractTest
	{
		[TestMethod]
		public void GetParentIsDityChanged()
		{
			var cl = new TstClient();
			var root = cl.GetRoot<Root>();
			root.Revert();
			var prnt = root.ParentItems.First();
			var chld = prnt.Children.First();
			Assert.AreEqual(false, root.IsDirty);
			Assert.AreEqual(false, prnt.IsDirty);
			chld.Value = 9.9;
			Assert.AreEqual(true, prnt.IsDirty);
			Assert.AreEqual(true, root.IsDirty);
		}

		[TestMethod]
		public void GetParentIsUndoEnabledChanged()
		{
			var cl = new TstClient();
			var root = cl.GetRoot<Root>();
			root.Revert();
			var prnt = root.ParentItems.First();
			var chld = prnt.Children.First();
			Assert.AreEqual(false, cl.GetIsUndoEnabled(root));
			Assert.AreEqual(false, cl.GetIsUndoEnabled(prnt));
			chld.Value = 9.9;
			Assert.AreEqual(true, cl.GetIsUndoEnabled(prnt));
			Assert.AreEqual(true, cl.GetIsUndoEnabled(root));
		}

		[TestMethod]
		public void GetParentIsRedoEnabledChanged()
		{
			var cl = new TstClient();
			var root = cl.GetRoot<Root>();
			root.Revert();
			var prnt = root.ParentItems.First();
			var chld = prnt.Children.First();
			Assert.AreEqual(false, cl.GetIsRedoEnabled(root));
			Assert.AreEqual(false, cl.GetIsRedoEnabled(prnt));
			chld.Value = 9.9;
			cl.Undo(chld);
			Assert.AreEqual(true, cl.GetIsRedoEnabled(prnt));
			Assert.AreEqual(true, cl.GetIsRedoEnabled(root));
		}

		[TestMethod]
		public void GetRefIsDityChanged()
		{
			var cl = new TstClient();
			var root = cl.GetRoot<Root>();
			root.Revert();
			var refObject = root.RefItems.First();
			var a = refObject.Ref;
			Assert.AreEqual(false, root.IsDirty);
			Assert.AreEqual(false, refObject.IsDirty);
			a.Value = 9;
			Assert.AreEqual(false, refObject.IsDirty);
			Assert.AreEqual(true, root.IsDirty);
		}

		[TestMethod]
		public void GetRefIsUndoEnabledChanged()
		{
			var cl = new TstClient();
			var root = cl.GetRoot<Root>();
			root.Revert();
			var refObject = root.RefItems.First();
			var a = refObject.Ref;
			Assert.AreEqual(false, cl.GetIsUndoEnabled(root));
			Assert.AreEqual(false, cl.GetIsUndoEnabled(refObject));
			a.Value = 9;
			Assert.AreEqual(false, cl.GetIsUndoEnabled(refObject));
			Assert.AreEqual(true, cl.GetIsUndoEnabled(root));
		}

		[TestMethod]
		public void GetRefIsRedoEnabledChanged()
		{
			var cl = new TstClient();
			var root = cl.GetRoot<Root>();
			root.Revert();
			var refObject = root.RefItems.First();
			var a = refObject.Ref;
			Assert.AreEqual(false, cl.GetIsRedoEnabled(root));
			Assert.AreEqual(false, cl.GetIsRedoEnabled(refObject));
			a.Value = 9;
			cl.Undo(a);
			Assert.AreEqual(false, cl.GetIsRedoEnabled(refObject));
			Assert.AreEqual(true, cl.GetIsRedoEnabled(root));
		}
	}
}