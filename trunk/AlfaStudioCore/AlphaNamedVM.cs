using XTransport;

namespace AlphaStudioCore
{
	public abstract class AlphaNamedVM : AlphaVM, IAlphaNamedVM 
	{
		[X("NAME")]
		private IXValue<string> m_name;

		protected override void InstantiationFinished()
		{
			base.InstantiationFinished();
			LinkProperty(m_name, () => Name);
			LinkProperty(m_name, () => NameForSort);
			LinkProperty(m_name, () => DocumentTitle);
		}

		public virtual string Name
		{
			get { return m_name.Value; }
			set { m_name.Value = value; }
		}

		public virtual string NameForSort
		{
			get { return Name; }
		}

		public virtual string DocumentTitle
		{
			get { return Name; }
		}
	}
}