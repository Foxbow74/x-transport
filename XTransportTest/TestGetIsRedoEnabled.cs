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
			var a = cl.GetRoot<Root>().AItems.First();
			Wait();
			a.Value = 10;
			cl.Undo(a.Uid);
			Assert.AreEqual(true, cl.GetIsRedoEnabled(a.Uid));
			cl.Redo(a.Uid);
			Assert.AreEqual(false, cl.GetIsRedoEnabled(a.Uid));
		}
	}
}