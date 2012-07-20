using AlphaStudioCore;

namespace ClientModulePositionsEditor.VMs
{
	abstract class AbstractAssetVM: AlphaVM
	{
		public abstract string Name { get; }
		public abstract string AssetTypeName { get; }
	}
}