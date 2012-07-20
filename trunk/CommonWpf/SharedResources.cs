using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace ClientCommonWpf
{
	/// <summary>
	/// Manage a Collection of Resource Dictionaries in Code and Merge them at the Element Level
	/// </summary>
	/// <see cref="http://drwpf.com/blog/2007/10/05/managing-application-resources-when-wpf-is-hosted/"/>
	public static class SharedResources
	{
		#region MergedDictionaries

		public static readonly DependencyProperty MergedDictionariesProperty = DependencyProperty.RegisterAttached("MergedDictionaries", typeof(string), typeof(SharedResources), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnMergedDictionariesChanged)));

		public static string GetMergedDictionaries(DependencyObject _d)
		{
			return (string)_d.GetValue(MergedDictionariesProperty);
		}

		public static void SetMergedDictionaries(DependencyObject _d, string _value)
		{
			_d.SetValue(MergedDictionariesProperty, _value);
		}

		private static void OnMergedDictionariesChanged(DependencyObject _d, DependencyPropertyChangedEventArgs _e)
		{
			if (!string.IsNullOrEmpty(_e.NewValue as string))
			{
				foreach (var dictionaryName in ((string)_e.NewValue).Split(';'))
				{
					var dictionary = GetResourceDictionary(dictionaryName);
					if (dictionary != null)
					{
						if (_d is FrameworkElement)
						{
							(_d as FrameworkElement).Resources
								.MergedDictionaries.Add(dictionary);
						}
						else if (_d is FrameworkContentElement)
						{
							(_d as FrameworkContentElement).Resources
								.MergedDictionaries.Add(dictionary);
						}
					}
				}
			}
		}

		#endregion

		private static readonly Dictionary<Color, Color> m_colors = new Dictionary<Color, Color>();
		public static Dictionary<int, Color> ChangeColors = new Dictionary<int, Color>();


		public static ResourceDictionary GetResourceDictionary(string _dictionaryName)
		{
			var resDict = new ResourceDictionary();

			WeakReference reference;
			if (m_sharedDictionaries.TryGetValue(_dictionaryName, out reference))
			{
				return (ResourceDictionary)reference.Target;
			}
			// if (result == null)

			var assemblyName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().ManifestModule.Name);
			var result = Application.LoadComponent(new Uri(assemblyName + ";component/Resources/" + _dictionaryName + ".xaml", UriKind.Relative)) as ResourceDictionary;


			for (int index = 0; index < result.MergedDictionaries.Count; index++)
			{
				var resourceDictionary = GetResourceDictionary(Path.GetFileNameWithoutExtension(result.MergedDictionaries[index].Source.OriginalString));
				resDict.MergedDictionaries.Add(resourceDictionary);
			}

			foreach (var key in result.Keys)
			{
				if (result[key] is SolidColorBrush)
				{
					var br = (SolidColorBrush)result[key];
					var add = new SolidColorBrush { Opacity = br.Opacity, Color = (m_colors.ContainsKey(br.Color) ? m_colors[br.Color] : br.Color) };
					resDict.Add(key, add);
				}
				else if (result[key] is LinearGradientBrush)
				{
					var br = (LinearGradientBrush)result[key];
					var add = new LinearGradientBrush { StartPoint = br.StartPoint, EndPoint = br.EndPoint, ColorInterpolationMode = br.ColorInterpolationMode, Opacity = br.Opacity, };
					foreach (var gradientStop in br.GradientStops)
					{
						if (m_colors.ContainsKey(gradientStop.Color))
						{
							add.GradientStops.Add(new GradientStop(m_colors[gradientStop.Color], gradientStop.Offset));
						}
						else
						{
							add.GradientStops.Add(gradientStop);
						}
					}
					resDict.Add(key, add);
				}
				else if (result[key] is RadialGradientBrush)
				{
					var br = (RadialGradientBrush)result[key];
					var add = new RadialGradientBrush { Center = br.Center, ColorInterpolationMode = br.ColorInterpolationMode, GradientOrigin = br.GradientOrigin, RadiusX = br.RadiusX, RadiusY = br.RadiusY };
					foreach (var gradientStop in br.GradientStops)
					{
						if (m_colors.ContainsKey(gradientStop.Color))
						{
							add.GradientStops.Add(new GradientStop(m_colors[gradientStop.Color], gradientStop.Offset));
						}
						else
						{
							add.GradientStops.Add(gradientStop);
						}
					}
					resDict.Add(key, add);
				}
				else if (result[key] is Color)
				{
					foreach (var pair in ChangeColors)
					{
						if ((key as string) == "BCKH" + pair.Key)
						{
							var color = Color.FromArgb(0x80, pair.Value.R, pair.Value.G, pair.Value.B);
							m_colors.Add((Color)result[key], color);
							resDict.Add(key, color);
						}
						else if ((key as string) == "BCKQ" + pair.Key)
						{
							var color = Color.FromArgb(0x40, pair.Value.R, pair.Value.G, pair.Value.B);
							m_colors.Add((Color)result[key], color);
							resDict.Add(key, color);
						}
						else if ((key as string) == "BCKO" + pair.Key)
						{
							var color = Color.FromArgb(0x20, pair.Value.R, pair.Value.G, pair.Value.B);
							m_colors.Add((Color)result[key], color);
							resDict.Add(key, color);
						}
						else if ((key as string) == "BCK" + pair.Key)
						{
							m_colors.Add((Color)result[key], pair.Value);
							//m_colors.Add((Color)result[key], (Color)result[key]);

							resDict.Add(key, pair.Value);
						}
					}
				}
				else
				{
					resDict.Add(key, result[key]);
				}

			}
			m_sharedDictionaries[_dictionaryName] = new WeakReference(resDict);
			return resDict;
		}

		private static readonly Dictionary<string, WeakReference> m_sharedDictionaries
			= new Dictionary<string, WeakReference>();

		public static void ResetDictionaries()
		{
			m_sharedDictionaries.Clear();
			m_colors.Clear();
		}
	}
}
