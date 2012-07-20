using System;
using System.Collections.Generic;
using AlphaStudioInterfaces;
using Common;

namespace AlphaStudioCore
{
	public class StrategyModel:IStrategyModel
	{
		public IEnumerable<IInternalInstrument> GetInternalInstruments(IEnumerable<IInstrument> _instruments)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IFactor> Factors
		{
			get { throw new NotImplementedException(); }
		}

		public EStrategyModel EStrategyModel
		{
			get { throw new NotImplementedException(); }
		}
	}
}