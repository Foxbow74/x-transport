using System;
using System.Windows;
using System.Windows.Input;

namespace AlphaStudioCore
{
	public class AlphaToolDescriptor
	{
		public EAlphaToolKind Kind { get; private set; }
		public ModifierKeys ModifierKeys { get; private set; }
		public Key Key { get; private set; }
		public Func<FrameworkElement> GenerateFunc { get; set; }

		public string Name { get; private set; }

		public bool AllowMultiple { get; private set; }

		public AlphaToolDescriptor(string _name, EAlphaToolKind _kind, ModifierKeys _modifierKeys, Key _key, Func<FrameworkElement> _generateFunc)
		{
			Key = _key;
			GenerateFunc = _generateFunc;
			ModifierKeys = _modifierKeys;
			Kind = _kind;
			Name = _name;
		}

		public AlphaToolDescriptor(string _name, EAlphaToolKind _kind, ModifierKeys _modifierKeys, Key _key, Func<FrameworkElement> _generateFunc, bool _allowMultiple):this(_name, _kind, _modifierKeys, _key, _generateFunc)
		{
			AllowMultiple = _allowMultiple;
		}
	}
}