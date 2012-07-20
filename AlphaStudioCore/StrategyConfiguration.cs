using System;
using System.Collections.Generic;
using AlphaStudioInterfaces;

namespace AlphaStudioCore
{
	public class StrategyConfiguration : IStrategyConfiguration
	{
		public string Name
		{
			get { throw new NotImplementedException(); }
		}

		public IStrategy Strategy
		{
			get { throw new NotImplementedException(); }
		}

		public IEnumerable<IStrategyModelConfiguration> StrategyModelConfiguration
		{
			get { throw new NotImplementedException(); }
		}
	}

	public class ResearchStrategyConfiguration : StrategyConfiguration, IResearchStrategyConfiguration
	{
	}

	public class ProductionStrategyConfiguration : StrategyConfiguration, IProductionStrategyConfiguration
	{
	}

}