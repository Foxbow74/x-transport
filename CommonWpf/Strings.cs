using System;
using System.ComponentModel;
using System.Linq;

namespace ClientCommonWpf
{
	public static class Strings
	{
		public static bool CheckPattern(this string _pattern, object _obj)
		{
			if (string.IsNullOrEmpty(_pattern))
			{
				return true;
			}
			var search = _pattern.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			var found = new bool[search.Length];
			foreach (var value in TypeDescriptor.GetProperties(_obj).Cast<PropertyDescriptor>().Where(_descriptor=>_descriptor.PropertyType==typeof(string)).Select(_propertyDescriptor => (string)_propertyDescriptor.GetValue(_obj)))
			{
				if (value == null) continue;
				var svalue = value.ToLower();
				for (var i = 0; i < search.Length; i++)
				{
					if (svalue.Contains(search[i]))
					{
						found[i] = true;
					}
				}
			}
			return found.All(_b => _b);
		}
	}
}
