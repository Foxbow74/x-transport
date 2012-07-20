using System;
using AlphaStudioInterfaces;

namespace AlphaStudioCore
{
	public abstract class InternalInstrument:IInternalInstrument
	{
		public IInstrument BaseInstrument
		{
			get { throw new NotImplementedException(); }
		}

		public ICurrency Currency
		{
			get { throw new NotImplementedException(); }
		}

		public ICountry Country
		{
			get { throw new NotImplementedException(); }
		}
	}
}