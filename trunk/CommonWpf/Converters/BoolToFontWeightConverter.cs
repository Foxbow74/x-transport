using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ClientCommonWpf.Converters
{
	[ValueConversion(typeof(object), typeof(Visibility))]
	public class BoolToFontWeightConverter : IValueConverter
	{
		public object Convert(object _value, Type _targetType, object _parameter, CultureInfo _culture)
		{
			return System.Convert.ToBoolean(_value) ? FontWeights.Bold : FontWeights.Normal;
		}

		public object ConvertBack(object _value, Type _targetType, object _parameter, CultureInfo _culture)
		{
			throw new NotImplementedException();
		}
	}
}