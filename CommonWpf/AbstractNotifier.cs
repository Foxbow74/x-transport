using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;

namespace ClientCommonWpf
{
	/// <summary>
	/// By Josh Smith
	/// </summary>
	public abstract class AbstractNotifier : INotifyPropertyChanged, IDisposable
	{
#if DEBUG
		protected AbstractNotifier()
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
		~AbstractNotifier()
		{
			m_notifiers[GetType()] = m_notifiers[GetType()] - 1;
		}

		public static void FinalReport()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();

			int sum = 0;
			foreach (var notifier in m_notifiers)
			{
				Debug.WriteLine(notifier.Key.Name + " not destroyed\t" + notifier.Value);
				sum += notifier.Value;
			}
			Debug.WriteLine(" Total: " + sum);
		}

		#region Debugging Aides

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
			string msg = "Invalid property name: " + _propertyName;

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
			string msg = "Invalid property name: " + _propertyName;
			throw new Exception(msg);
		}

		/// <summary>
		/// Returns whether an exception is thrown, or if a Debug.Fail() is used
		/// when an invalid property name is passed to the VerifyPropertyName method.
		/// The default value is false, but subclasses used by unit tests might 
		/// override this property's getter to return true.
		/// </summary>
		protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

		#endregion // Debugging Aides

#endif

		#region INotifyPropertyChanged Members

		/// <summary>
		/// Raised when a property on this object has a new value.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged<T>(Expression<Func<T>> _property)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler == null) return;
			var expression = (MemberExpression) _property.Body;
			handler(this, new PropertyChangedEventArgs(expression.Member.Name));
		}

		#endregion // INotifyPropertyChanged Members

		private readonly Dictionary<INotifyPropertyChanged, List<Tuple<string, Action<INotifyPropertyChanged>>>> m_subscribers = new Dictionary<INotifyPropertyChanged, List<Tuple<string, Action<INotifyPropertyChanged>>>>();


		protected void Subscribe<TNotifier, T>(TNotifier _notifier, Expression<Func<T>> _property,
		                                       Action<INotifyPropertyChanged> _handler)
			where TNotifier : INotifyPropertyChanged
		{
			var expression = (MemberExpression) _property.Body;
			string name = expression.Member.Name;
#if DEBUG
			VerifyPropertyName(_notifier, name);
#endif
			var tuple = new Tuple<string, Action<INotifyPropertyChanged>>(name, _handler);
			List<Tuple<string, Action<INotifyPropertyChanged>>> list;
			if (!m_subscribers.TryGetValue(_notifier, out list))
			{
				list = new List<Tuple<string, Action<INotifyPropertyChanged>>>();
				_notifier.PropertyChanged += NotifierOnPropertyChanged;
				m_subscribers.Add(_notifier, list);
			}
			list.Add(tuple);
		}

		private void NotifierOnPropertyChanged(object _sender, PropertyChangedEventArgs _args)
		{
			var notifier = (INotifyPropertyChanged) _sender;
			List<Tuple<string, Action<INotifyPropertyChanged>>> subscribers = m_subscribers[notifier];
			foreach (var tuple in subscribers)
			{
				if (_args.PropertyName == tuple.Item1) tuple.Item2(notifier);
			}
		}

		protected void UnSubscribe<TNotifier>(TNotifier _notifier) where TNotifier : INotifyPropertyChanged
		{
			m_subscribers.Remove(_notifier);
			_notifier.PropertyChanged -= NotifierOnPropertyChanged;
		}

		public virtual void Dispose()
		{
			UnSubscribe();
			m_subscribers.Clear();
		}

		protected void UnSubscribe()
		{
			foreach (INotifyPropertyChanged notifier in m_subscribers.Keys)
			{
				notifier.PropertyChanged -= NotifierOnPropertyChanged;
			}
			m_subscribers.Clear();
		}
	}
}