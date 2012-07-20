using System;
using System.Collections.Generic;
using System.Linq;
using AlphaStudioInterfaces;
using AlphaStudioInterfaces.Dates;
using DalInterfaces;
using FakeDal;

namespace AlphaStudioCore
{
	public static class FactorManager
	{
		public static string FactorName(this EFactorToken _factorToken)
		{
			switch (_factorToken)
			{
				case EFactorToken.CARRY:
					return "Carry";
					break;
				default:
					throw new ArgumentOutOfRangeException("_factorToken");
			}
		}

		public static IFactorReport<T> Compute<T>(IFactorConfiguration _factorConfiguration) where T : IInternalInstrument
		{
			var anchors = _factorConfiguration.Weights.Keys.ToDictionary(_instrument => _instrument.GetAnchor(_factorConfiguration.Factor.FactorIngredientAnchor));
			//var dalReport = FactorCalculator.Compute(new DalFactorComputationArgs(_factorConfiguration));

			return null;
		}

		public static int GetAnchor(this IInternalInstrument _internalInstrument, EFactorIngredientAnchor _anchor)
		{
			switch (_anchor)
			{
				case EFactorIngredientAnchor.CURRENCY:
					return _internalInstrument.Currency.Id;
				case EFactorIngredientAnchor.COUNTRY:
					return _internalInstrument.Country.Id;
				default:
					throw new ArgumentOutOfRangeException("_anchor");
			}
		}

	}

	public class FactorReport<T>:IFactorReport<T> where T : IInternalInstrument
	{
		public IDictionary<T, TimeSequence<double>> RawFinalValues
		{
			get { throw new NotImplementedException(); }
		}

		public IDictionary<T, TimeSequence<double>> StandardlyZScoredFinalValues
		{
			get { throw new NotImplementedException(); }
		}

		public IDictionary<T, TimeSequence<double>> VolatilityAdjustedRawValues
		{
			get { throw new NotImplementedException(); }
		}

		public IDictionary<T, TimeSequence<double>> VolatilityAdjustedZScoreValues
		{
			get { throw new NotImplementedException(); }
		}

		public IDictionary<T, TimeSequence<double>> ComputeStandardlyGlobalZScoredFinalValues()
		{
			throw new NotImplementedException();
		}

		public IDictionary<T, TimeSequence<double>> ComputeRelativelyGlobalZScoredFinalValues()
		{
			throw new NotImplementedException();
		}

		public IDictionary<T, TimeSequence<double>> ComputeRelativelyZScoredFinalValues()
		{
			throw new NotImplementedException();
		}

		public TimeInterval ExtractionInterval
		{
			get { throw new NotImplementedException(); }
		}

		public IDictionary<string, IDictionary<T, TimeSequence<double>>> DataIngredients
		{
			get { throw new NotImplementedException(); }
		}

		public string Name
		{
			get { throw new NotImplementedException(); }
		}
	}

	public class DalFactorComputationArgs : IDalFactorComputationArgs
	{
		public DalFactorComputationArgs(IFactorConfiguration _factorConfiguration)
		{
			Ingredients = _factorConfiguration.Factor.Ingredients;
			Anchors = _factorConfiguration.Weights.Keys.Select(_instrument => _instrument.GetAnchor(_factorConfiguration.Factor.FactorIngredientAnchor));
			TimeInterval = _factorConfiguration.TimeInterval;
		}

		public IEnumerable<EIngredientToken> Ingredients { get; private set; }

		public IEnumerable<int> Anchors{ get; private set; }

		public TimeInterval TimeInterval{ get; private set; }
	}


}
