using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XTransport.Server;
using XTransportTest.Server;

namespace XTransportTest
{
	[TestClass]
	public abstract class AbstractTest
	{
		private static int m_test = 0;

		static AbstractTest()
		{
			new TstServer();
		}

		[TestInitialize]
		public void Initialize()
		{
			TstServer.DbName = "tst" + (++m_test) + ".db";
			Uploader.Upload(TstServer.DbName);
			AbstractXServer.Instance.Reset();
		}

		[TestCleanupAttribute]
		public void Cleanup()
		{
		}

		protected static void Wait(int _times)
		{
			Thread.Sleep(_times);
		}

		protected static void Wait(int _maxTimes, Func<bool> _while)
		{
			var i = 0;
			while (_while())
			{
				Thread.Sleep(1);
				if (i++ == _maxTimes) break;
			}
			if (i > 0)
			{
				Console.WriteLine("Wait takes " + new TimeSpan(0, 0, 0, 0, i));
			}
		}
	}
}