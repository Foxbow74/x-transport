using System;

namespace XTransport.Client
{
	public abstract class ClientXChildObject<TKind, TParent> : ClientXObject<TKind>, IClientXChildObject<TKind>
		where TParent : ClientXObject<TKind>
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