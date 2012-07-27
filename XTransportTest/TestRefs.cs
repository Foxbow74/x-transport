using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XTransportTest.Client;

namespace XTransportTest
{
	[TestClass]
	public class TestRefs : AbstractTest
	{
		[TestMethod]
		public void GetRef()
		{
			var cl = new TstClient();
			var root = cl.GetRoot<Root>();
			var rf = root.RefItems.First();
			var a = rf.Ref;
			Assert.AreNotEqual(null, a);
			Assert.IsTrue(root.AItems.Contains(a));
		}

		[TestMethod]
		public void GetChildRef()
		{
			var cl = new TstClient();
			var root = cl.GetRoot<Root>();
			var rf = root.RefItems.First();
			var cr = rf.ChildRef;
			Assert.AreNotEqual(null, cr);
			Assert.IsTrue(root.ParentItems.Any(_parent => _parent.Children.Contains(cr)));
		}


		[TestMethod]
		public void ChangeAMirror()
		{
			var cl = new TstClient();
			var rf = cl.GetRoot<Root>().RefItems.First();
			Assert.AreEqual(cl.GetRoot<Root>().AItems.First(), rf.Ref);


			var a2 = cl.GetRoot<Root>().AItems.ToList()[1];
			rf.Ref = a2;
			var rfm = cl.Get<RefMirror>(rf.Uid);
			Assert.AreEqual(rfm.Ref, rf.Ref);
			Assert.AreEqual(rfm.IsDirty, true);

			cl.Undo(rf.Uid);
			Assert.AreEqual(cl.GetRoot<Root>().AItems.First(), rf.Ref);
			Assert.AreEqual(cl.GetRoot<Root>().AItems.First(), rfm.Ref);
		}
	}
}