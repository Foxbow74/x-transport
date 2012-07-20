using System;
using System.Linq;
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
			var a1 = cl.GetRoot<Root>().AItems.First();
			var a2 = cl.GetRoot<Root>().AItems.ToList()[1];
			Console.WriteLine(a1);
			Console.WriteLine(a2);
		}

		[TestMethod]
		public void GetB()
		{
			var cl = new TstClient();
			Assert.AreNotEqual(null, cl.GetRoot<Root>().BItems.First());
			Assert.AreNotEqual(null, cl.GetRoot<Root>().BItems.ToList()[1]);
		}

		[TestMethod]
		public void IsDirty()
		{
			var cl = new TstClient();
			var a = cl.GetRoot<Root>().AItems.First();
			a.Value = 10;
			Assert.AreEqual(a.IsDirty, true);
		}

		[TestMethod]
		public void ChangeAMirror()
		{
			var cl = new TstClient();
			var a = cl.GetRoot<Root>().AItems.First();
			a.Value = 10;

			var am = cl.Get<AMirror>(a.Uid);
			Assert.AreEqual(a.Value, am.Value);
			Assert.AreEqual(am.IsDirty, true);
		}
	}
}