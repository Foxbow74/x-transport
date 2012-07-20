using System.Reflection;

namespace XTransport
{
	internal class XFieldInfo<TKind>
	{
		public XFieldInfo(int _memberId, FieldInfo _field)
		{
			FieldId = _memberId;
			Field = _field;
		}

		public IXObjectFactory<TKind> Factory { get; set; }

		public int FieldId { get; set; }
		public ConstructorInfo Constructor { get; set; }
		public FieldInfo Field { get; set; }
	}
}