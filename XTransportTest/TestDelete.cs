using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XTransport.Server;
using XTransportTest.Client;
using XTransportTest.Server;

namespace XTransportTest
{
	[TestClass]
	public class TestDelete : AbstractTest
	{
		[TestMethod]
		public void Delete0()
		{
			var cl = new TstClient();
			var rt = cl.GetRoot<Root>();
			var rf = rt.RefItems.First();
			var chd = rf.ChildRef;
			var parent = rt.ParentItems.Single(_parent => _parent.Children.Contains(chd));
			parent.Children.Remove(chd);
			cl.Save(rt);
			Assert.IsFalse(parent.IsDirty);
			Assert.IsFalse(rf.IsDirty);
			Assert.IsFalse(rt.IsDirty);

			var cl2 = new TstClient();
			var rt2 = cl2.GetRoot<Root>();
			var rf2 = rt2.RefItems.First();
			var chd2 = rf2.ChildRef;
			Assert.AreEqual(chd.Uid, chd2.Uid);
			var parent2 = rt2.ParentItems.SingleOrDefault(_parent => _parent.Children.Contains(chd));
			Assert.IsNull(parent2);
			parent2 = rt2.ParentItems.Single(_parent => _parent.Uid == parent.Uid);
			Assert.IsNotNull(parent2);
			parent2.Children.Add(chd2);
			cl2.Save(parent2);
			TstServer.Instance.Reset();

			var cl3 = new TstClient();
			var rt3 = cl3.GetRoot<Root>();
			var rf3 = rt3.RefItems.First();
			var chd3 = rf3.ChildRef;
		}
	}
}