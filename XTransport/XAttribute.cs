using System;

namespace XTransport
{
	public class XAttribute : Attribute
	{
		public XAttribute(string _fieldName)
		{
			FieldName = _fieldName;
			HashCode = _fieldName.GetHashCode();
		}

		public XAttribute(int _kind)
		{
			FieldName = null;
			HashCode = _kind;
		}

		protected string FieldName { get; private set; }
		public int HashCode { get; private set; }
	}

	public class XFactoryAttribute : Attribute
	{
		public XFactoryAttribute(Type _factoryType)
		{
			FactoryType = _factoryType;
		}

		public Type FactoryType { get; private set; }
	}
}