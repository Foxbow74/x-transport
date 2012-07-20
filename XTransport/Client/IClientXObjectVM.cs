using System;
using System.ComponentModel;
using System.Windows.Input;

namespace XTransport.Client
{
	public interface IClientXObjectVM<TKind> : IClientXObject<TKind>, INotifyPropertyChanged, IDisposable
	{
		Cursor CurrentCursor { get; }
	}
}