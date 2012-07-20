using System.Collections.Generic;
using AlphaStudioInterfaces;
using FakeDal;

namespace AlphaStudioCore
{
	public abstract class Factor:IFactor
	{
		public EFactorToken Token
		{
			get { throw new System.NotImplementedException(); }
		}

		public EFactorIngredientAnchor FactorIngredientAnchor
		{
			get { throw new System.NotImplementedException(); }
		}

		public IEnumerable<EIngredientToken> Ingredients
		{
			get { throw new System.NotImplementedException(); }
		}
	}
}