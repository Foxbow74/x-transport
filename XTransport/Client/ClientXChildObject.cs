namespace XTransport.Client
{
	public abstract class ClientXChildObject<TKind, TParent> : ClientXObject<TKind>, IClientXChildObject<TKind>
		where TParent : ClientXObject<TKind>
	{
		public TParent Parent { get; internal set; }

		#region IClientXChildObject<TKind> Members

		void IClientXChildObject<TKind>.SetParent(ClientXObject<TKind> _xObject)
		{
			Parent = (TParent) _xObject;
		}

		#endregion
	}
}