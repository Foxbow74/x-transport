using System.Collections.Generic;
using AlphaXTransport;
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

		protected override IEnumerable<int> KindAlsoKnownAs(int _kind)
		{
			switch ((EAlphaKind)_kind)
			{
				case EAlphaKind.CURRENCY_PAIR:
				case EAlphaKind.BOND:
				case EAlphaKind.INDEX:
					yield return (int)EAlphaKind.ASSET;
					break;
				case EAlphaKind.FORWARD:
				case EAlphaKind.FUTURES:
					yield return (int)EAlphaKind.DERIVATIVE;
					break;
			}
		}
	}
}
