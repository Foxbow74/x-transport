using System;
using System.Collections.Generic;
using XTransport;
using XTransport.Server.Storage;
using XTransportTest.Client;

namespace XTransportTest.Server
{
	public class Uploader
	{
		public static void Upload(string _dbName)
		{
			var serverSideAs = new List<UplodableObject>
			                   	{
			                   		new UplodableObject((int) ETestKind.A) {new UValue<int>("IVAL", 1)},
			                   		new UplodableObject((int) ETestKind.A) {new UValue<int>("IVAL", 2)},
			                   	};
			var serverSideBs = new List<UplodableObject>
			                   	{
			                   		new UplodableObject((int) ETestKind.B) {new UValue<string>("SVAL", "B1")},
			                   		new UplodableObject((int) ETestKind.B) {new UValue<string>("SVAL", "B2")},
			                   	};

			var parents = new List<UplodableObject>
			              	{
			              		new UplodableObject((int) ETestKind.PARENT) {new UValue<string>("NAME", "P1")},
			              		new UplodableObject((int) ETestKind.PARENT) {new UValue<string>("NAME", "P2")},
			              	};

			var childs = new List<UplodableObject>
					             	{
										new UplodableObject((int) ETestKind.CHILD) {new UValue<double>("CVAL", 1.0)},
										new UplodableObject((int) ETestKind.CHILD) {new UValue<double>("CVAL", 2.0)},
										new UplodableObject((int) ETestKind.CHILD) {new UValue<double>("CVAL", 3.0)}
					             	};

			var serverSideRefs = new List<UplodableObject>
			                     	{
			                     		new UplodableObject((int) ETestKind.REF) {new UValue<Guid>("RVAL", serverSideAs[0].Uid), new UValue<Guid>("CHILD_REF", childs[1].Uid)},
			                     		new UplodableObject((int) ETestKind.REF) {new UValue<Guid>("RVAL", serverSideAs[1].Uid), new UValue<Guid>("CHILD_REF", childs[2].Uid)},
			                     	};

			using (var st = new SQLiteStorage(_dbName))
			{
				using (st.CreateTransaction())
				{
					foreach (var currency in serverSideAs)
					{
						st.InsertMain(currency);
					}
					foreach (var pair in serverSideBs)
					{
						st.InsertMain(pair);
					}
		
					st.InsertMain(childs[0], parents[0].Uid, "LIST".GetHashCode());
					st.InsertMain(childs[1], parents[0].Uid, "LIST".GetHashCode());
					st.InsertMain(childs[2], parents[1].Uid, "LIST".GetHashCode());

					foreach (var index in serverSideRefs)
					{
						st.InsertMain(index);
					}
					foreach (var prt in parents)
					{
						st.InsertMain(prt);
					}
				}
			}
		}
	}
}