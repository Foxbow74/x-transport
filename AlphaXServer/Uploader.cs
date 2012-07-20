using System;
using System.Collections.Generic;
using AlphaXTransport;
using XTransport;
using XTransport.Server;
using XTransport.Server.Storage;

namespace AlphaXServer
{
	public class Uploader
	{
		public static void Upload(string _dbName)
		{
			const string NAME = "NAME";

			var currencies = new List<UplodableObject>
			                 	{
			                 		new UplodableObject((int) EAlphaKind.CURRENCY) {new UValue<string>(NAME, "United States Dollar"), new UValue<string>("CODE", "USD")},
									new UplodableObject((int) EAlphaKind.CURRENCY) {new UValue<string>(NAME, "Great Britain Pound"), new UValue<string>("CODE", "GBP")},
									new UplodableObject((int) EAlphaKind.CURRENCY) {new UValue<string>(NAME, "Hong Kong Dollar"), new UValue<string>("CODE", "HKD")},
			                 	};
			var pairs = new List<UplodableObject>
			            	{
			                 		new UplodableObject((int) EAlphaKind.CURRENCY_PAIR) {new UValue<Guid>("LEFT_CURRENCY", currencies[0].Uid), new UValue<Guid>("RIGHT_CURRENCY", currencies[1].Uid)},
									new UplodableObject((int) EAlphaKind.CURRENCY_PAIR) {new UValue<Guid>("LEFT_CURRENCY", currencies[0].Uid), new UValue<Guid>("RIGHT_CURRENCY", currencies[2].Uid)},
									new UplodableObject((int) EAlphaKind.CURRENCY_PAIR) {new UValue<Guid>("LEFT_CURRENCY", currencies[1].Uid), new UValue<Guid>("RIGHT_CURRENCY", currencies[2].Uid)},
			            	};

			var indexes = new List<UplodableObject>
			            	{
			                 	new UplodableObject((int) EAlphaKind.INDEX) {new UValue<string>(NAME, "S&P 500 Index"), new UValue<Guid>("CURRENCY", currencies[0].Uid)},
								new UplodableObject((int) EAlphaKind.INDEX) {new UValue<string>(NAME, "NASDAQ 100 Index"), new UValue<Guid>("CURRENCY", currencies[1].Uid)},
								new UplodableObject((int) EAlphaKind.INDEX) {new UValue<string>(NAME, "Hang Seng Index"), new UValue<Guid>("CURRENCY", currencies[2].Uid)},
			            	};

			var bonds = new List<UplodableObject>
			            	{
			                 	new UplodableObject((int) EAlphaKind.BOND) {new UValue<string>(NAME, "Sweden Govt Bond 10Y Benchmark Yield"), new UValue<Guid>("CURRENCY", currencies[0].Uid)},
			                 	new UplodableObject((int) EAlphaKind.BOND) {new UValue<string>(NAME, "Aus 10yr Treasury Bond"), new UValue<Guid>("CURRENCY", currencies[0].Uid)},
			                 	new UplodableObject((int) EAlphaKind.BOND) {new UValue<string>(NAME, "US Long Bond"), new UValue<Guid>("CURRENCY", currencies[0].Uid)},
			            	};
			var derivatives = new List<UplodableObject>();

			using (var st = new SQLiteStorage(_dbName))
			{
				using (st.CreateTransaction())
				{
					foreach (var currency in currencies)
					{
						st.InsertMain(currency);
					}
					foreach (var pair in pairs)
					{
						st.InsertMain(pair);
						derivatives.Add(new UplodableObject((int)EAlphaKind.FORWARD) { new UValue<Guid>("ASSET", pair.Uid) });
					}
					foreach (var index in indexes)
					{
						st.InsertMain(index);
						derivatives.Add(new UplodableObject((int)EAlphaKind.FUTURES) { new UValue<Guid>("ASSET", index.Uid) });
					}
					foreach (var bond in bonds)
					{
						st.InsertMain(bond);
						derivatives.Add(new UplodableObject((int)EAlphaKind.FUTURES) { new UValue<Guid>("ASSET", bond.Uid) });
					}

					foreach (var derivative in derivatives)
					{
						st.InsertMain(derivative);
					}

					for (var i = 0; i < 3; ++i)
					{
						var prt = new UplodableObject((int)EAlphaKind.PORTFOLIO)
							{
								new UValue<string>(NAME, "Portfolio" + i),
								new UValue<Guid>("BASE_CURRENCY", currencies[i].Uid),
								new UValue<decimal>("NOTIONAL_AMOUNT", i*1000000)
							};
						st.InsertMain(prt); 
						for (var index = i; index < derivatives.Count; index += 3)
						{
							var pi = new UplodableObject((int)EAlphaKind.PORTFOLIO_INSTRUMENT)
							{
								new UValue<Guid>("DERIVATIVE", derivatives[index].Uid),
							};
							st.InsertMain(pi, prt.Uid, "INSTRUMENTS".GetHashCode());
						}
					}
				}
			}
		}
	}
}
