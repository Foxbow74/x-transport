using System;
using System.Collections.Generic;
using AlphaStudioInterfaces;
using AlphaStudioInterfaces.Dates;

namespace AlphaStudioCore
{
	public class FactorConfiguration : IFactorConfiguration
	{
		public IFactor Factor
		{
			get { throw new NotImplementedException(); }
		}

		public Dictionary<IInternalInstrument, double> Weights
		{
			get { throw new NotImplementedException(); }
		}

		public TimeInterval TimeInterval
		{
			get { throw new NotImplementedException(); }
		}
	}
}