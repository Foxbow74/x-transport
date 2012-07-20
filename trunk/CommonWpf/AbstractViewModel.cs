using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;

namespace ClientCommonWpf
{
	/// <summary>
	/// By Josh Smith
	/// </summary>
	public abstract class AbstractViewModel : AbstractNotifier
	{
		private string m_displayName;
		public Cursor CurrentCursor { get; private set; }

		public virtual string DisplayName
		{
			get { return m_displayName; }
			set
			{
				m_displayName = value;
				OnPropertyChanged(() => DisplayName);
				OnPropertyChanged(() => DisplayNameForSort);
			}
		}

		public virtual string DisplayNameForSort
		{
			get { return DisplayName; }
		}

		protected void SetCurrentCursor(Cursor _value)
		{
			CurrentCursor = _value;
			OnPropertyChanged(() => CurrentCursor);
		}

		#region MVVM Light properties that define if we are at design time

		private static bool? m_isInDesignMode;

		/// <summary>
		/// Gets a value indicating whether the control is in design mode
		/// (running under Blend or Visual Studio).
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
			Justification = "Non static member needed for data binding")]
		public bool IsInDesignMode
		{
			get { return IsInDesignModeStatic; }
		}

		/// <summary>
		/// Gets a value indicating whether the control is in design mode
		/// (running in Blend or Visual Studio).
		/// </summary>
		[SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands",
			Justification = "The security risk here is neglectible.")]
		public static bool IsInDesignModeStatic
		{
			get
			{
				if (!m_isInDesignMode.HasValue)
				{
					DependencyProperty prop = DesignerProperties.IsInDesignModeProperty;
					m_isInDesignMode
						= (bool) DependencyPropertyDescriptor
						         	.FromProperty(prop, typeof (FrameworkElement))
						         	.Metadata.DefaultValue;

					// Just to be sure
					if (!m_isInDesignMode.Value
					    && Process.GetCurrentProcess().ProcessName.StartsWith("devenv", StringComparison.Ordinal))
					{
						m_isInDesignMode = true;
					}
				}
				return m_isInDesignMode.Value;
			}
		}

		#endregion
	}
}