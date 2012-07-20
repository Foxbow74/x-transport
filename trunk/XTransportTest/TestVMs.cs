using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XTransportTest.Client;
using XTransportTest.ClientVM;

namespace XTransportTest
{
	[TestClass]
	public class TestVMs : AbstractTest
	{
		[TestMethod]
		public void Simple()
		{
			var cl = new TstClient();
			Assert.AreNotEqual(null, cl.GetRoot<RootVM>().AItems.First());
		}

		[TestMethod]
		public void Notification()
		{
			var cl = new TstClient();
			var avm = cl.GetRoot<RootVM>().AItems.First();

			var mre = new ManualResetEvent(false);
			avm.PropertyChanged += delegate(object _sender, PropertyChangedEventArgs _args)
			                       	{
			                       		Assert.AreEqual("Value", _args.PropertyName);
			                       		mre.Set();
			                       	};
			ThreadPool.QueueUserWorkItem(_state =>
			                             	{
			                             		Wait();
			                             		avm.Value = Int32.MinValue;
			                             	});
			Assert.AreEqual(true, mre.WaitOne(100));
		}

		[TestMethod]
		public void MirrorNotification()
		{
			var cl = new TstClient();

			var avm = cl.GetRoot<RootVM>().AItems.First();
			var aMirrorVM = cl.GetRoot<RootMirrorVM>().AItems.First();

			var mre = new ManualResetEvent(false);
			avm.PropertyChanged += delegate(object _sender, PropertyChangedEventArgs _args)
			                       	{
			                       		Assert.AreEqual("Value", _args.PropertyName);
			                       		mre.Set();
			                       	};
			ThreadPool.QueueUserWorkItem(_state =>
			                             	{
			                             		Wait();
			                             		aMirrorVM.Value = Int32.MaxValue;
			                             	});

			Assert.AreEqual(true, mre.WaitOne(100));
		}

		[TestMethod]
		public void TwoClientNotification()
		{
			var clVM = new TstClient();
			var client = new TstClient();
			var avm = clVM.GetRoot<RootVM>().AItems.First();
			var a = client.Get<A>(avm.Uid);

			var mre = new ManualResetEvent(false);
			avm.PropertyChanged += delegate(object _sender, PropertyChangedEventArgs _args)
			                       	{
			                       		Assert.AreEqual("Value", _args.PropertyName);
			                       		mre.Set();
			                       	};
			a.Value = Int32.MaxValue;
			client.Save(avm.Uid);
			ThreadPool.QueueUserWorkItem(_state =>
			                             	{
			                             		Wait();
			                             		clVM.Revert(avm.Uid);
			                             	});
			Assert.AreEqual(true, mre.WaitOne(1000));
		}

		[TestMethod]
		public void ObservableCollection()
		{
			var cl = new TstClient();
			var parent = cl.GetRoot<RootVM>().ParentItems.First();

			while (parent.List.Count != parent.ObsCol.Count)
			{
				Wait();
			}
			foreach (var vm in parent.List)
			{
				Assert.AreEqual(true, parent.ObsCol.Contains(vm));
			}
		}

		[TestMethod]
		public void MirrorObservableCollection()
		{
			var cl = new TstClient();
			var parent = cl.GetRoot<RootVM>().ParentItems.First();
			var parentM = cl.Get<ParentMirrorVM>(parent.Uid);
			var mre = new ManualResetEvent(false);
			ThreadPool.QueueUserWorkItem(_state =>
			                             	{
			                             		Wait();
												parentM.List.Remove(parentM.List.First());
			                             		mre.Set();
			                             	});
			Assert.AreEqual(true, mre.WaitOne(100));
			while (parentM.List.Count != parent.ObsCol.Count)
			{
				Wait();
			}
			foreach (var vm in parent.List)
			{
				Assert.AreEqual(true, parent.ObsCol.Contains(vm));
			}
		}

		[TestMethod]
		public void TwoClientObservableCollection()
		{
			var cl = new TstClient();
			var cl1 = new TstClient();
			var parent = cl.GetRoot<RootVM>().ParentItems.First();
			var parent1 = cl1.Get<ParentVM>(parent.Uid);

			var mre = new ManualResetEvent(false);
			ThreadPool.QueueUserWorkItem(_state =>
			                             	{
			                             		Wait();
			                             		parent1.List.Remove(parent1.List.First());
			                             		cl1.Save(parent.Uid);
			                             		mre.Set();
			                             	});
			Assert.AreEqual(true, mre.WaitOne(100));
			Wait(100);
			cl.Revert(parent.Uid);

			var waitCounter = 1000;
			while (parent1.List.Count != parent.ObsCol.Count && waitCounter-- > 0)
			{
				Wait();
			}
			Assert.AreEqual(parent1.List.Count, parent.ObsCol.Count);
			foreach (var vm in parent.List)
			{
				Assert.AreEqual(true, parent.ObsCol.Contains(vm));
			}
		}
	}
}