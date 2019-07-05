using System;
using System.Configuration;

namespace Business.Utilities
{

	public static class AppConfigHelper
	{

		public static string K12ProxyLoginUrl
			=> GetAppSetting("K12ProxyLoginUrl", "");


		public static string K12ProxyLogoutUrl
			=> GetAppSetting("K12ProxyLogoutUrl", "");


		public static long SchoolId
			=> GetAppSetting("SchoolId", 0);


		public static string SchoolName
			=> GetAppSetting("SchoolName", "");


		public static T GetAppSetting<T>(string key, T defaultValue)
		{
			if (!string.IsNullOrEmpty(key))
			{
				string value = ConfigurationManager.AppSettings[key];
				try
				{
					if (value != null)
					{
						var theType = typeof(T);
						if (theType.IsEnum)
							return (T)Enum.Parse(theType, value.ToString(), true);

						return (T)Convert.ChangeType(value, theType);
					}

					return default(T);
				}
				catch { }
			}

			return defaultValue;
		}

	}

}