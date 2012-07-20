using System;
using System.Collections.Generic;
using AlphaStudioInterfaces;

namespace AlphaStudioCore
{
	public class StrategyModelConfiguration:IStrategyModelConfiguration
	{
		public IStrategyModel StrategyModel
		{
			get { throw new NotImplementedException(); }
		}

		public IEnumerable<IFactorConfiguration> FactorConfigurations
		{
			get { throw new NotImplementedException(); }
		}
	}
}