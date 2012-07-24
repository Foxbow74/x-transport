using XTransport;
using XTransport.Server;
using XTransport.Server.Storage;

namespace XTransportTest.Server
{
	internal class TstServer : AbstractXServer
	{
		public static string DbName { get; set; }

		protected override bool IsAsync
		{
			get { return false; }
		}

		protected override IStorage CreateStorage()
		{
			return new SQLiteStorage(DbName);
		}
	}
}