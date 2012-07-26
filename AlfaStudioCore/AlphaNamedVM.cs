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
			BindProperty(m_name, () => Name);
			BindProperty(m_name, () => NameForSort);
			BindProperty(m_name, () => DocumentTitle);
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