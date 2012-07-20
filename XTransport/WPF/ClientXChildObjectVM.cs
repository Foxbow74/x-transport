using System;
using XTransport.Client;

namespace XTransport.WPF
{
	public abstract class ClientXChildObjectVM<TKind, TParent> : ClientXObjectVM<TKind>, IClientXChildObject<TKind>
		where TParent : ClientXObjectVM<TKind>
	{
		public TParent Parent { get; internal set; }

		#region IClientXChildObject<TKind> Members

		void IClientXChildObject<TKind>.SetParent(IClientXObjectInternal<TKind> _xObject)
		{
			Parent = (TParent) _xObject;
		}

		Guid IClientXChildObject<TKind>.ParentUid
		{
			get { return Parent.Uid; }
		}

		#endregion
	}
}