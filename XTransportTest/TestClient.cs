using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XTransportTest.Client;

namespace XTransportTest
{
	[TestClass]
	public class TestClient : AbstractTest
	{
		[TestMethod]
		public void ClientInstantiation()
		{
			new TstClient();
		}

		[TestMethod]
		public void GetA()
		{
			var cl = new TstClient();
			var a1 = cl.GetRoot<Root>().AItems[0];
			var a2 = cl.GetRoot<Root>().AItems[1];
			Console.WriteLine(a1);
			Console.WriteLine(a2);
		}

		[TestMethod]
		public void GetB()
		{
			var cl = new TstClient();
			Assert.AreNotEqual(null,cl.GetRoot<Root>().BItems[0]);
			Assert.AreNotEqual(null,cl.GetRoot<Root>().BItems[1]);
		}

		[TestMethod]
		public void IsDirty()
		{
			var cl = new TstClient();
			var a = cl.GetRoot<Root>().AItems[0];
			a.Value = 10;
			Assert.AreEqual(a.IsDirty, true);
		}

		[TestMethod]
		public void ChangeAMirror()
		{
			var cl = new TstClient();
			var a = cl.GetRoot<Root>().AItems[0];
			a.Value = 10;

			var am = cl.Get<AMirror>(a.Uid);
			Assert.AreEqual(a.Value, am.Value);
			Assert.AreEqual(am.IsDirty, true);
		}
	}
}