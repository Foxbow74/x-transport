using System;
using AlphaXTransport;
using XTransport;
using XTransport.Client;

namespace ClientModulePositionsEditor.VMs
{
	class DerivativeFactory:IXObjectFactory<EAlphaKind>
	{
		public EAlphaKind Kind
		{
			get { return EAlphaKind.DERIVATIVE; }
		}

		public IClientXObject<EAlphaKind> CreateInstance(EAlphaKind _kind)
		{
			switch (_kind)
			{
				case EAlphaKind.FORWARD:
					return new ForwardVM();
				case EAlphaKind.FUTURES:
					return new FuturesVM();
			}
			throw new ArgumentOutOfRangeException();
		}
	}
}