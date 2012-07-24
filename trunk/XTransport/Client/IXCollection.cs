using System;
using System.Collections.Generic;

namespace XTransport.Client
{
	internal interface IXCollection<TKind> : IXValueInternal, IXClientUserInternal<TKind>
	{
		IEnumerable<Guid> GetUids();
		void SetOwnerInfo(ClientXObject<TKind> _xObject, int _fieldId);
		void AddSilently(ClientXObject<TKind> _item);
		void RemoveSilently(Guid _uid);
	}
}