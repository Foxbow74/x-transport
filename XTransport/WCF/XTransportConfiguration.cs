using System;
using System.Configuration;

namespace XTransport.WCF
{
	public static class XTransportConfiguration
	{
		public static Boolean UseDirectAccess = true;
		public static String ServiceName = AppDomain.CurrentDomain.FriendlyName;

		static XTransportConfiguration()
		{
			var useDirectAccessValue = ConfigurationManager.AppSettings.Get("UseDirectAccess");
			if (useDirectAccessValue == null) return;
			Boolean boolValue;
			if (Boolean.TryParse(useDirectAccessValue, out boolValue))
			{
				UseDirectAccess = boolValue;
			}

			var serviceNameValue = ConfigurationManager.AppSettings.Get("ServiceName");
			if (serviceNameValue == null) return;
			ServiceName = serviceNameValue;
		}
	}
}