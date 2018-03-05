using Business.Config;
using Symber.Web.Report;
using System;
using System.Configuration;

namespace Business.Helper
{

	public static class AppConfigHelper
	{
		public static string WebSite_BBS
			=> GetAppSetting("website:BBS", "");


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

