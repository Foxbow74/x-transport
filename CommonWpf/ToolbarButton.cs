using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ClientCommonWpf
{
	public class ButtonProperties
	{
		public static ImageSource GetImage(DependencyObject _obj)
		{
			return (ImageSource)_obj.GetValue(ImageProperty);
		}

		public static void SetImage(DependencyObject _obj, ImageSource _value)
		{
			_obj.SetValue(ImageProperty, _value);
		}

		public static ImageSource GetDisabledImage(DependencyObject _obj)
		{
			return (ImageSource)_obj.GetValue(ImageProperty);
		}

		public static void SetDisabledImage(DependencyObject _obj, ImageSource _value)
		{
			_obj.SetValue(ImageProperty, _value);
		}

		public static readonly DependencyProperty ImageProperty = DependencyProperty.RegisterAttached("Image", typeof(ImageSource), typeof(ButtonProperties), new UIPropertyMetadata((ImageSource)null));
		public static readonly DependencyProperty DisabledImageProperty = DependencyProperty.RegisterAttached("DisabledImage", typeof(ImageSource), typeof(ButtonProperties), new UIPropertyMetadata((ImageSource)null));
	}

}
