using System;

namespace ClientCommonWpf
{
	public class ExtendedLazy<T>
	{
		private readonly Func<T> m_func;
		private readonly bool m_isThreadSafe;

		private Lazy<T> m_lazy;

		public ExtendedLazy(Func<T> _func)
			: this(_func, true)
		{
			m_func = _func;
		}

		public ExtendedLazy(Func<T> _func, bool _isThreadSafe)
		{
			m_func = _func;
			m_isThreadSafe = _isThreadSafe;
		}

		public T Value
		{
			get
			{
				if (m_lazy == null)
				{
					m_lazy = new Lazy<T>(m_func, m_isThreadSafe);
				}
				return m_lazy.Value;
			}
		}

		public void Reset()
		{
			if (m_lazy == null || !m_lazy.IsValueCreated) return;

			if (m_lazy.Value is IDisposable)
			{
				((IDisposable) m_lazy.Value).Dispose();
			}
			m_lazy = null;
		}
	}
}