using Business.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;

namespace Business.Cache
{

	public class ThisAppCache
	{

		public static string GetCacheKey(Type contentType, string ext = null)
				=> ext == null ? contentType.ToString() : contentType.ToString() + ext;


		public static T GetCache<T>(string ext = null)
			=> (T)HttpRuntime.Cache.Get(GetCacheKey(typeof(T), ext));


		public static void SetCache<T>(T content, int absoluteExpiration = ThisApp.StableCacheMinutes, string ext = null)
			=> HttpRuntime.Cache.Insert(
				GetCacheKey(typeof(T), ext),
				content,
				null,
				DateTime.Now.AddMinutes(absoluteExpiration),
				TimeSpan.Zero,
				CacheItemPriority.High,
				null);


		public static void RemoveCache<T>(string ext = null)
			=> HttpRuntime.Cache.Remove(GetCacheKey(typeof(T), ext));


		public static void CleanCache<T>(string ext = null)
		{
			List<string> keys = new List<string>();
			IDictionaryEnumerator enumerator = HttpRuntime.Cache.GetEnumerator();
			while (enumerator.MoveNext())
			{
				keys.Add(enumerator.Key.ToString());
			}

			string key = GetCacheKey(typeof(T), ext);

			foreach (string k in keys)
			{
				if (ext != null)
				{
					if (k == key)
					{
						HttpRuntime.Cache.Remove(k);
					}
				}
				else
				{
					if (k.StartsWith(key, StringComparison.InvariantCulture))
					{
						HttpRuntime.Cache.Remove(k);
					}
				}
			}
		}

	}

}