using System.Globalization;
using System.Windows.Controls;

namespace ClientCommonWpf.ValidationRules
{
	public class DoubleValidationRule : ValidationRule
	{
		public override ValidationResult Validate(object _value, CultureInfo _cultureInfo)
		{
			var strValue = _value as string;
			if (strValue == null)
			{
				return new ValidationResult(false, "Invalid double - Value is not a string");
			}

			double result;
			if (!double.TryParse(strValue, NumberStyles.Any, _cultureInfo, out result))
			{
				return new ValidationResult(false, "Invalid double - Please type a valid number");
			}

			return GetResult(result);
		}

		protected virtual ValidationResult GetResult(double _result)
		{
			return ValidationResult.ValidResult;
		}
	}
}