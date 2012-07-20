using System.Windows.Input;

namespace AlphaStudioCore
{
	public static class Utils
	{
		public static string InputGestureText(ModifierKeys _modifierKeys, Key _key)
		{
			if(_key==Key.None)
			{
				return string.Empty;
			}
			if(_modifierKeys==ModifierKeys.None)
			{
				return _key.ToString();
			}
			return string.Format("{0}+{1}", _modifierKeys, _key);
		}
	}
}
