using XTransport;
using XTransport.Server;
using XTransport.Server.Storage;

namespace AlphaXServer
{
	public class Server : AbstractXServer
	{
		public const string DB_NAME = "alpha.db";

		protected override IStorage CreateStorage()
		{
			return new SQLiteStorage(DB_NAME);
		}
	}
}
