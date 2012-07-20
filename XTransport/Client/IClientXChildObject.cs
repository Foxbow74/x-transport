using System;

namespace XTransport.Client
{
	internal interface IClientXChildObject<TKind>
	{
		Guid ParentUid { get; }
		void SetParent(IClientXObjectInternal<TKind> _xObject);
	}
}