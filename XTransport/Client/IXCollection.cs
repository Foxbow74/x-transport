using System;
using System.Collections.Generic;

namespace XTransport.Client
{
	internal interface IXCollection<TKind> : IXValueInternal, IXClientUserInternal<TKind>
	{
		IEnumerable<Guid> GetUids();
		void SetOwnerInfo(Guid _ownerUid, int _fieldId);
		void AddSilently(ClientXObject<TKind> _item);
		void RemoveSilently(Guid _uid);
	}
}