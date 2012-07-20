using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows;

namespace ClientCommonWpf
{
	public static class FrameWorkElementHelper
	{
		public static void Cast<T>(this PropertyChangedEventHandler _propertyChanged, FrameworkElement _element, Expression<Func<T>> _property)
		{
			var handler = _propertyChanged;
			if (handler == null) return;
			var expression = (MemberExpression)_property.Body;
			handler(_element, new PropertyChangedEventArgs(expression.Member.Name));
		}
	}
}