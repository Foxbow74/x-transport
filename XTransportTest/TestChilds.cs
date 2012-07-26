using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XTransportTest.Client;

namespace XTransportTest
{
	[TestClass]
	public class TestChilds : AbstractTest
	{
		[TestMethod]
		public void CheckParent()
		{
			var cl = new TstClient();
			var root = cl.GetRoot<Root>();
			var pr = root.ParentItems.First();
			var child = pr.Children.First();
			Assert.AreNotEqual(null, child);
			Assert.AreEqual(pr, child.Parent);
		}

		[TestMethod]
		public void AddNewAndCheckParent()
		{
			var cl = new TstClient();
			var pr = cl.GetRoot<Root>().ParentItems.First();
			var child = new Child {Value = 99.9};
			pr.Children.Add(child);
			Assert.AreEqual(true, pr.Children.Contains(child));
			Assert.AreEqual(pr, child.Parent);
		}
	}
}