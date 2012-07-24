using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XTransport;
using XTransportTest.Client;

namespace XTransportTest
{
	[TestClass]
	public class TestSerialization : AbstractTest
	{
		[TestMethod]
		public void Serialize()
		{
			var cl = new TstClient();
			var a = cl.GetRoot<Root>().AItems.First();
			a.Value = 10;
			var report = new XReport(a.Uid, a.GetChanges(), a.Stored, (int) a.Kind);

			var ser = new DataContractSerializer(typeof (XReport)
				//, new Type[] { typeof(XReportItem<Int32>)}
				);
			var sb = new StringBuilder();
			using (var writer = XmlWriter.Create(sb, new XmlWriterSettings {OmitXmlDeclaration = true}))
			{
				ser.WriteObject(writer, report);
			}
			var xml = sb.ToString();
			var resultSerializer = new DataContractSerializer(typeof (XReport)
				//, new Type[] { typeof(XReportItem<Int32>) }
				);
			XReport deserializedReport;
			var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
			{
				deserializedReport = (XReport) resultSerializer.ReadObject(stream);
			}
			var reportHC = report;
			var deserializedHC = deserializedReport.GetHashCode();

			Assert.AreEqual(report.Uid, deserializedReport.Uid);
		}
	}
}