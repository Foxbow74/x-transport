namespace XTransport.Client
{
	internal interface IXClientUserInternal<TKind>
	{
		void SetClient(AbstractXClient<TKind> _client);
	}
}