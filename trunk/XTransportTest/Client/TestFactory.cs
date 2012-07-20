using System;
using XTransport;
using XTransport.Client;

namespace XTransportTest.Client
{
	class TestFactory : IXObjectFactory<ETestKind>
	{
		public ETestKind Kind
		{
			get { return ETestKind.ALL; }
		}

		public IClientXObject<ETestKind> CreateInstance(ETestKind _kind)
		{
			switch (_kind)
			{
				case ETestKind.A:
					return new A();
				case ETestKind.B:
					return new B();
				case ETestKind.REF:
					return new RefObject();
				case ETestKind.PARENT:
					return new Parent();
				case ETestKind.CHILD:
					return new Child();
			}
			throw new ArgumentOutOfRangeException();
		}
	}
}