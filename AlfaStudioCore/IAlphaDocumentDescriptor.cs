using System;
using System.Windows;
using System.Windows.Input;
using AlphaXTransport;

namespace AlphaStudioCore
{
	public interface IAlphaDocumentDescriptor
	{
		EAlphaKind Kind { get; }
		EAlphaDocumentKind DocKind { get; }
		ModifierKeys ModifierKeys { get; }
		Key Key { get; }

		/// <summary>
		/// Generates document for defined object
		/// </summary>
		/// <param name="_id"></param>
		/// <param name="_namedPersistedViewModel"></param>
		/// <returns></returns>
		FrameworkElement Generate(Guid _id, out IAlphaNamedVM _namedPersistedViewModel);
		bool IsDefault { get; }
		string DocumentName { get; }
	}


	public interface IAlphaRootToolDescriptor
	{
		ModifierKeys ModifierKeys { get; }
		Key Key { get; }
		FrameworkElement Generate(out IAlphaVM _rootViewModel);
		string Name { get; }
		string RootToolIdentifier { get; }
	}
}