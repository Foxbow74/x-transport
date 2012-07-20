namespace AlphaStudioCore
{
	public interface IAlphaNamedVM : IAlphaVM
	{
		string Name { get; }
		string NameForSort { get; }
		string DocumentTitle { get; }
	}
}