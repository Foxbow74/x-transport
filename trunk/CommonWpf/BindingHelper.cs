using System;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Data;
using ClientCommonWpf.ValidationRules;

namespace ClientCommonWpf
{
	public static class BindingHelper
	{
		public static FrameworkElement Bind<T>(this FrameworkElement _frameworkElement, DependencyProperty _dependencyProperty, Expression<Func<T>> _expression, bool _isReadOnly, string _stringFormat, object _defaultValue, object _source = null)
		{
			if (_expression == null)
			{
				_frameworkElement.SetValue(_dependencyProperty, _defaultValue);
			}
			else
			{
				_frameworkElement.SetBinding(_dependencyProperty, _expression.GetBinding(_isReadOnly, _stringFormat, _source));
			}
			return _frameworkElement;
		}

		public static FrameworkContentElement Bind<T>(this FrameworkContentElement _element, DependencyProperty _dependencyProperty, Expression<Func<T>> _expression, bool _isReadOnly, IValueConverter _converter)
		{
			if (_expression == null)
			{
				_element.SetValue(_dependencyProperty, default(T));
			}
			else
			{
				_element.SetBinding(_dependencyProperty, _expression.GetBinding(_isReadOnly, _converter));
			}
			return _element;
		}

		public static Binding GetBinding<T>(this Expression<Func<T>> _property, bool _isReadOnly, string _stringFormat, object _source = null)
		{
			var expression = (MemberExpression)_property.Body;
			var binding = new Binding(expression.Member.Name) { StringFormat = _stringFormat, Mode = _isReadOnly ? BindingMode.OneWay : BindingMode.TwoWay };
			if (_source != null)
			{
				binding.Source = _source;
			}
			if (typeof(T) == typeof(Double))
			{
				binding.ValidationRules.Add(new DoubleValidationRule());
			}
			return binding;
		}

		public static Binding GetBinding<T>(this Expression<Func<T>> _property, bool _isReadOnly, IValueConverter _converter)
		{
			var binding = new Binding(_property.GetName())
			{
				Mode = _isReadOnly ? BindingMode.OneWay : BindingMode.TwoWay,
				Converter = _converter
			};
			return binding;
		}

		public static string GetName<T>(this Expression<Func<T>> _property)
		{
			var expression = (MemberExpression)_property.Body;
			return expression.Member.Name;
		}
	}
}
