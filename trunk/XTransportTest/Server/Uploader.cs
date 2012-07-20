using System;
using System.Collections.Generic;
using XTransport;
using XTransport.Server;
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

			var serverSideRefs = new List<UplodableObject>
			                     	{
			                     		new UplodableObject((int) ETestKind.REF) {new UValue<Guid>("RVAL", serverSideAs[0].Uid)},
			                     		new UplodableObject((int) ETestKind.REF) {new UValue<Guid>("RVAL", serverSideAs[1].Uid)},
			                     	};

			var parents = new List<UplodableObject>
			              	{
			              		new UplodableObject((int) ETestKind.PARENT) {new UValue<string>("NAME", "P1")},
			              		new UplodableObject((int) ETestKind.PARENT) {new UValue<string>("NAME", "P2")},
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
					foreach (var index in serverSideRefs)
					{
						st.InsertMain(index);
					}
					foreach (var prt in parents)
					{
						st.InsertMain(prt);
					}
					st.InsertMain(new UplodableObject((int) ETestKind.CHILD) {new UValue<double>("CVAL", 1.0)}, parents[0].Uid,
					              "LIST".GetHashCode());
					st.InsertMain(new UplodableObject((int) ETestKind.CHILD) {new UValue<double>("CVAL", 2.0)}, parents[0].Uid,
					              "LIST".GetHashCode());
					st.InsertMain(new UplodableObject((int) ETestKind.CHILD) {new UValue<double>("CVAL", 3.0)}, parents[1].Uid,
					              "LIST".GetHashCode());
				}
			}
		}
	}
}