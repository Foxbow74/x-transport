using System;

namespace XTransport.Client
{
	internal interface IClientXChildObject<TKind>
	{
		void SetParent(Guid _collectionOwner);
		ClientXObject<TKind> Parent { get; }
	}
}