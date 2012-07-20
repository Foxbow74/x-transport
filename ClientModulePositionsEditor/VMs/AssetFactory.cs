using System;
using AlphaXTransport;
using XTransport;
using XTransport.Client;

namespace ClientModulePositionsEditor.VMs
{
	class AssetFactory:IXObjectFactory<EAlphaKind>
	{
		public EAlphaKind Kind
		{
			get { return EAlphaKind.ASSET; }
		}

		public IClientXObject<EAlphaKind> CreateInstance(EAlphaKind _kind)
		{
			switch (_kind)
			{
				case EAlphaKind.BOND:
					return new BondVM();
				case EAlphaKind.INDEX:
					return new IndexVM();
				case EAlphaKind.CURRENCY_PAIR:
					return new CurrencyPairVM();
			}
			throw new ArgumentOutOfRangeException();
		}
	}
}