using System;
using System.Collections.Generic;
using AlphaStudioInterfaces;

namespace AlphaStudioCore
{
	public class Portfolio:IPortfolio
	{
		public string Name
		{
			get { throw new NotImplementedException(); }
		}

		public ICurrency BaseCurrency
		{
			get { throw new NotImplementedException(); }
		}

		public IStrategyConfiguration StrategyConfiguration
		{
			get { throw new NotImplementedException(); }
		}

		public IEnumerable<IPosition> OpenPositions
		{
			get { throw new NotImplementedException(); }
		}
	}
}
