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
			var rf = cl.GetRoot<Root>().RefItems[0];
			var a = rf.Ref;
			Assert.AreNotEqual(null, a);
		}


		[TestMethod]
		public void ChangeAMirror()
		{
			var cl = new TstClient();
			var rf = cl.GetRoot<Root>().RefItems[0];
			Assert.AreEqual(cl.GetRoot<Root>().AItems[0], rf.Ref);


			var a2 = cl.GetRoot<Root>().AItems[1];
			rf.Ref = a2;
			var rfm = cl.Get<RefMirror>(rf.Uid);
			Assert.AreEqual(rfm.Ref, rf.Ref);
			Assert.AreEqual(rfm.IsDirty, true);

			cl.Undo(rf.Uid);
			Assert.AreEqual(cl.GetRoot<Root>().AItems[0], rf.Ref);
			Assert.AreEqual(cl.GetRoot<Root>().AItems[0], rfm.Ref);
		}
	}
}