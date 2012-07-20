using System;
using System.Threading;

namespace XTransport
{
	internal static class Utils
	{
		public static T GetSafely<T>(this AutoResetEvent _autoResetEvent, ref T _arg, Func<T> _init) where T : class
		{
			if (_arg == null)
			{
				if (_autoResetEvent.WaitOne())
				{
					if (_arg == null)
					{
						_arg = _init();
					}
					_autoResetEvent.Set();
				}
			}
			return _arg;
		}
	}
}