using System;
using System.Windows;
using System.Windows.Input;
using AlphaXTransport;
using XTransport;

namespace AlphaStudioCore
{
	public class AlphaDocumentDescriptor<TVM> : IAlphaDocumentDescriptor where TVM : AlphaNamedVM
	{
		private readonly Func<TVM, FrameworkElement> m_generateFunc;
		public EAlphaKind Kind { get; private set; }
		public EAlphaDocumentKind DocKind { get; private set; }
		public string DocumentName { get; private set; }
		public ModifierKeys ModifierKeys { get; private set; }
		public Key Key { get; private set; }
		public bool IsDefault { get; set; }

		public AlphaDocumentDescriptor(string _documentName, EAlphaKind _persistedModel, EAlphaDocumentKind _kind, ModifierKeys _modifierKeys, Key _key, Func<TVM, FrameworkElement> _generateFunc)
		{
			m_generateFunc = _generateFunc;
			DocumentName = _documentName;
			ModifierKeys = _modifierKeys;
			Key = _key;
			DocKind = _kind;
			Kind = _persistedModel;
		}

		public virtual FrameworkElement Generate(Guid _id, out IAlphaNamedVM _namedPersistedViewModel)
		{
			_namedPersistedViewModel = AlphaClient.Instance.Get<TVM>(_id);
			var result = m_generateFunc((TVM)_namedPersistedViewModel);
			_namedPersistedViewModel.ViewCreated();
			return result;
		}
	}

	public class AlphaRootToolDescriptor<TVM> : IAlphaRootToolDescriptor where TVM : AlphaVM
	{
		private readonly Func<TVM, FrameworkElement> m_generateFunc;
		public string Name { get; private set; }
		public string RootToolIdentifier { get; private set; }

		public ModifierKeys ModifierKeys { get; private set; }
		public Key Key { get; private set; }

		public AlphaRootToolDescriptor(string _title, ModifierKeys _modifierKeys, Key _key, Func<TVM, FrameworkElement> _generateFunc)
		{
			RootToolIdentifier = "root_tool_" + typeof(TVM).Name;
			m_generateFunc = _generateFunc;
			Name = _title;
			ModifierKeys = _modifierKeys;
			Key = _key;
		}

		public virtual FrameworkElement Generate(out IAlphaVM _rootViewModel)
		{
			_rootViewModel = AlphaClient.Instance.GetRoot<TVM>();
			var result = m_generateFunc((TVM)_rootViewModel);
			_rootViewModel.ViewCreated();
			return result;
		}
	}
}