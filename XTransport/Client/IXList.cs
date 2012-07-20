using System;
using System.Collections.Generic;

namespace XTransport.Client
{
	internal interface IXList<TKind> : IXValueInternal, IXClientUserInternal<TKind>
	{
		void SetOwnerInfo(IClientXObjectInternal<TKind> _xObject, int _fieldId);
		IEnumerable<Guid> GetUids();
		void AddSilently(IXObject<TKind> _item);
		void RemoveSilently(ClientXObject<TKind> _item);
	}
}