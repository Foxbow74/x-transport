using System;
using AlphaStudioInterfaces;

namespace AlphaStudioCore
{
	public class Contract:IContract
	{
		public IInstrument Instrument
		{
			get { throw new NotImplementedException(); }
		}

		public DateTime ExpirationDate
		{
			get { throw new NotImplementedException(); }
		}

		public DateTime FirstNoticeDate
		{
			get { throw new NotImplementedException(); }
		}

		public string Ticker
		{
			get { throw new NotImplementedException(); }
		}
	}
}