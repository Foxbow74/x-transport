namespace XTransport.Client
{
	internal interface IClientXChildObject<TKind>
	{
		void SetParent(ClientXObject<TKind> _xObject);
	}
}