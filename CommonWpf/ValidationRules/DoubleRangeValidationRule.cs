using System.Windows.Controls;

namespace ClientCommonWpf.ValidationRules
{
	public class DoubleRangeValidationRule : DoubleValidationRule
	{
		public double Min { get; set; }
		public double Max { get; set; }

		protected override ValidationResult GetResult(double _result)
		{
			if ((_result < Min) || (_result > Max))
			{
				return new ValidationResult(false, string.Format("Double not in range: {0} - {1}", Min, Max));
			}
			return ValidationResult.ValidResult;
		}
	}
}