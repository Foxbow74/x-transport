﻿using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XTransportTest.Client;
using XTransportTest.ClientVM;

namespace XTransportTest
{
	[TestClass]
	public class TestRoots : AbstractTest
	{
		[TestMethod]
		public void Simple()
		{
			var cl = new TstClient();
			var general = cl.GetRoot<Root>();
			Assert.AreNotEqual(0, general.AItems.Count);
			Assert.AreNotEqual(0, general.BItems.Count);
			Assert.AreNotEqual(0, general.ParentItems.Count);
			Assert.AreNotEqual(0, general.RefItems.Count);
		}

		[TestMethod]
		public void Mirror()
		{
			var cl = new TstClient();
			var general = cl.GetRoot<Root>();
			var generalMirror = cl.GetRoot<RootMirror>();

			foreach (var a in general.AItems)
			{
				var a1 = a;
				Assert.AreEqual(a.Uid, generalMirror.AItems.Single(_mirror => _mirror.Uid == a1.Uid).Uid);
			}
		}

		[TestMethod]
		public void TwoClients()
		{
			var cl = new TstClient();
			var cl1 = new TstClient();
			var generalMirror = cl1.GetRoot<RootMirror>();

			var general = cl.GetRoot<RootVM>();
			general.AItems.RemoveAt(0);
			cl.Save(general.Uid);
			Wait(200);
			cl1.Revert(generalMirror.Uid);
			var waitCounter = 10;
			while (general.AItems.Count != generalMirror.AItems.Count && waitCounter-- > 0)
			{
				Wait();
			}
			Assert.AreEqual(general.AItems.Count, generalMirror.AItems.Count);
			foreach (var a in general.AItems)
			{
				var a1 = a;
				Assert.AreEqual(a.Uid, generalMirror.AItems.Single(_mirror => _mirror.Uid == a1.Uid).Uid);
			}
		}
	}
}