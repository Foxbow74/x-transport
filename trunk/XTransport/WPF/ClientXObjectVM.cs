using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using XTransport.Client;

namespace XTransport.WPF
{
	public abstract class ClientXObjectVM<TKind> : ClientXObject<TKind>, IClientXObjectVM<TKind>,
												   IXClientUserInternal<TKind>
	{
		private readonly Dictionary<IXValueInternal, List<PropertyChangedEventArgs>> m_propertyChangedArgs =
			new Dictionary<IXValueInternal, List<PropertyChangedEventArgs>>();

		private readonly Dictionary<IXValueInternal, Action> m_actions = new Dictionary<IXValueInternal, Action>();

		private Dispatcher m_uiDispatcher;

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region IXClientUserInternal<TKind> Members

		protected internal AbstractXClient<TKind> Client { get; private set; }

		void IXClientUserInternal<TKind>.SetClient(AbstractXClient<TKind> _client)
		{
			Client = _client;
			SetDispatcher(_client.GetUiDispatcherInternal());
		}

		#endregion

        public void SetDispatcher(Dispatcher _dispatcher)
        {
            m_uiDispatcher = _dispatcher;
        }

		public Cursor CurrentCursor { get; private set; }

		protected void SetCurrentCursor(Cursor _value)
		{
			CurrentCursor = _value;
			OnPropertyChanged(() => CurrentCursor);
		}

		protected void OnPropertyChanged<TP>(Expression<Func<TP>> _property)
		{
			var handler = PropertyChanged;
			if (handler == null) return;
			var expression = (MemberExpression) _property.Body;
			handler(this, new PropertyChangedEventArgs(expression.Member.Name));
		}

		protected void BindProperty<TF, TP>(IXValue<TF> _field, Expression<Func<TP>> _property)
		{
			var expression = (MemberExpression) _property.Body;
			List<PropertyChangedEventArgs> list;
			var name = expression.Member.Name;
			if (!m_propertyChangedArgs.TryGetValue((IXValueInternal) _field, out list))
			{
				list = new List<PropertyChangedEventArgs>();
				m_propertyChangedArgs.Add((IXValueInternal) _field, list);
			}
			if (list.Any(_args => _args.PropertyName == name)) return;
			list.Add(new PropertyChangedEventArgs(name));
		}

		protected void SubscribeHandler<TF>(IXValue<TF> _field, Action _handler)
		{
			m_actions.Add((IXValueInternal) _field, _handler);
		}

		protected ReadOnlyObservableCollection<TP> CreateObservableCollection<TP>(ICollection<TP> _field)
			where TP : ClientXObject<TKind>
		{
			var list = (XCollection<TP, TKind>) _field;
			ReadOnlyObservableCollection<TP> result = null;
			m_uiDispatcher.Invoke(DispatcherPriority.Background,
								  new ThreadStart(delegate { result = list.CreateObservableCollection(); }));
			return result;
		}

		internal override void XValueOnChanged(IXValueInternal _field)
		{
			base.XValueOnChanged(_field);
			if (PropertyChanged == null) return;

			List<PropertyChangedEventArgs> argsList;
			if (m_propertyChangedArgs.TryGetValue(_field, out argsList))
			{
				foreach (var args in argsList)
				{
					m_uiDispatcher.BeginInvoke(DispatcherPriority.Background, PropertyChanged, this, args);
				}
			}
			Action value;
			if (m_actions.TryGetValue(_field, out value))
			{
				m_uiDispatcher.BeginInvoke(DispatcherPriority.Background, value);
			}
		}

		#region Debug

#if DEBUG

		protected ClientXObjectVM()
		{
			if (!m_notifiers.ContainsKey(GetType()))
			{
				m_notifiers[GetType()] = 1;
			}
			else
			{
				m_notifiers[GetType()] = m_notifiers[GetType()] + 1;
			}

			ThrowOnInvalidPropertyName = true;
		}

		private static readonly Dictionary<Type, int> m_notifiers = new Dictionary<Type, int>();

		/// <summary>
		/// Useful for ensuring that ViewModel objects are properly garbage collected.
		/// </summary>
		~ClientXObjectVM()
		{
			m_notifiers[GetType()] = m_notifiers[GetType()] - 1;
		}

		public virtual void Dispose()
		{
			Debug.WriteLine("Dispose " + GetHashCode());
		}

		public static void FinalReport()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();

			var sum = 0;
			foreach (var notifier in m_notifiers)
			{
				Debug.WriteLine(notifier.Key.Name + " not destroyed\t" + notifier.Value);
				sum += notifier.Value;
			}
			Debug.WriteLine(" Total: " + sum);
		}


		/// <summary>
		/// Warns the developer if this object does not have
		/// a public property with the specified name. This 
		/// method does not exist in a Release build.
		/// </summary>
		[DebuggerStepThrough]
		public void VerifyPropertyName(string _propertyName)
		{
			// Verify that the property name matches a real,  
			// public, instance property on this object.
			if (TypeDescriptor.GetProperties(this)[_propertyName] != null) return;
			var msg = "Invalid property name: " + _propertyName;

			if (ThrowOnInvalidPropertyName)
				throw new Exception(msg);

			Debug.Fail(msg);
		}

		[DebuggerStepThrough]
		public static void VerifyPropertyName(INotifyPropertyChanged _notifier, string _propertyName)
		{
			// Verify that the property name matches a real,  
			// public, instance property on this object.
			if (TypeDescriptor.GetProperties(_notifier)[_propertyName] != null) return;
			var msg = "Invalid property name: " + _propertyName;
			throw new Exception(msg);
		}


		/// <summary>
		/// Returns whether an exception is thrown, or if a Debug.Fail() is used
		/// when an invalid property name is passed to the VerifyPropertyName method.
		/// The default value is false, but subclasses used by unit tests might 
		/// override this property's getter to return true.
		/// </summary>
		protected virtual bool ThrowOnInvalidPropertyName { get; private set; }
#endif

		#endregion

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
					var prop = DesignerProperties.IsInDesignModeProperty;
					m_isInDesignMode =
						(bool) DependencyPropertyDescriptor.FromProperty(prop, typeof (FrameworkElement)).Metadata.DefaultValue;

					// Just to be sure
					if (!m_isInDesignMode.Value &&
						Process.GetCurrentProcess().ProcessName.StartsWith("devenv", StringComparison.Ordinal))
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