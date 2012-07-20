using System;
using AlphaStudioInterfaces;

namespace AlphaStudioCore
{
	public abstract class Instrument : IInstrument
	{
		public string Name
		{
			get { throw new NotImplementedException(); }
		}

		public ICurrency AccountCurrency
		{
			get { throw new NotImplementedException(); }
		}

		public ICountry Country
		{
			get { throw new NotImplementedException(); }
		}
	}
}