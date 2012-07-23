using System;
using System.Collections.Generic;
using XTransport;
using XTransport.Server;
using XTransport.Server.Storage;
using XTransportTest.Client;

namespace XTransportTest.Server
{
	internal class TstServer : AbstractXServer
	{
		public static string DbName { get; set; }

		protected override IStorage CreateStorage()
		{
			return new SQLiteStorage(DbName);
		}

		protected override bool IsAsync
		{
			get { return false; }
		}
	}
}