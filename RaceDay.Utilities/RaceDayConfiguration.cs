using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Web;
using System.IO;

namespace RaceDay.Utilities
{
	public class RaceDayConfiguration : ConfigurationSection
	{
		private static RaceDayConfiguration instance;
		private static object lockInstance = new object();

		// Methods
		public static RaceDayConfiguration Instance
		{
			get
			{
				// Avoid claiming lock if already loaded
				if (instance == null)
				{
					lock (lockInstance)
					{
						// Do this again to make sure _provider is still null
						if (instance == null)
						{
							instance = (ConfigurationManager.GetSection("RaceDayConfiguration") as RaceDayConfiguration);

							if (instance == null)
								throw new ConfigurationErrorsException("RaceDayConfiguration section is missing");
						}
					}
				}

				return instance;
			}
		}

		[ConfigurationProperty("ApplicationPath", IsRequired = true)]
		public String ApplicationPath
		{
			get
			{
				return Convert.ToString(base["ApplicationPath"]);
			}	
		}

		[ConfigurationProperty("ApplicationId", IsRequired = true)]
		public String ApplicationId
		{
			get
			{
				return Convert.ToString(base["ApplicationId"]);
			}
		}

		[ConfigurationProperty("ApplicationSecret", IsRequired = true)]
		public String ApplicationSecret
		{
			get
			{
				return Convert.ToString(base["ApplicationSecret"]);
			}
		}

		[ConfigurationProperty("ApplicationPermissions", IsRequired = true)]
		public String ApplicationPermissions
		{
			get
			{
				return Convert.ToString(base["ApplicationPermissions"]);
			}
		}

		[ConfigurationProperty("DebugUser", IsRequired = false)]
		public String DebugUser
		{
			get
			{
				return Convert.ToString(base["DebugUser"]);
			}
		}

		[ConfigurationProperty("UseHttps", IsRequired = false)]
		public bool UseHttps
		{
			get
			{
				if (base["UseHttps"] != null)
					return Convert.ToBoolean(base["UseHttps"]);

				return false;
			}
		}

		[ConfigurationProperty("HttpsPort", IsRequired = false)]
		public Int32 HttpsPort
		{
			get
			{
				if (base["HttpsPort"] != null)
				{
					Int32 httpsPort = Convert.ToInt32(base["HttpsPort"]);
					return (httpsPort == 0 ? 443 : httpsPort);
				}

				return 443;
			}
		}

		[ConfigurationProperty("HttpPort", IsRequired = false)]
		public Int32 HttpPort
		{
			get
			{
				if (base["HttpPort"] != null)
				{
					Int32 httpPort = Convert.ToInt32(base["HttpPort"]);
					return (httpPort == 0 ? 80 : httpPort);
				}

				return 80;
			}
		}

		[ConfigurationProperty("NotifyAdminUser", IsRequired = false)]
		public String NotifyAdminUser
		{
			get
			{
				if (base["NotifyAdminUser"] != null)
					return base["NotifyAdminUser"].ToString();
				return String.Empty;
			}
		}
	}
}
