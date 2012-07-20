using System.Globalization;
using System.Windows.Controls;

namespace ClientCommonWpf.ValidationRules
{
	public class IntegerValidationRule : ValidationRule
	{
		public override ValidationResult Validate(object _value, CultureInfo _cultureInfo)
		{
			var strValue = _value as string;
			if (strValue == null)
			{
				return new ValidationResult(false, "Invalid integer - Value is not a string");
			}

			int result;
			if (!int.TryParse(strValue, NumberStyles.Any, _cultureInfo, out result))
			{
				return new ValidationResult(false, "Invalid integer - Please type a valid number");
			}
			return ValidationResult.ValidResult;
		}
	}
}