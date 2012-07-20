using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ClientCommonWpf.Converters
{
	[ValueConversion(typeof(object), typeof(Visibility))]
	public class BoolToVisibilityConverter : IValueConverter
	{
		public object Convert(object _value, Type _targetType, object _parameter, CultureInfo _culture)
		{
			return System.Convert.ToBoolean(_value) ? Visibility.Visible :
			(_parameter != null && ((string)_parameter) == "Hidden" ? Visibility.Hidden : Visibility.Collapsed);
		}

		public object ConvertBack(object _value, Type _targetType, object _parameter, CultureInfo _culture)
		{
			throw new NotImplementedException();
		}
	}
}
