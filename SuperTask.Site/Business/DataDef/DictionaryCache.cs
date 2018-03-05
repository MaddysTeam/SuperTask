using Business.Cache;
using Business.Config;
using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static Business.DictionaryCache;

namespace Business
{

   public static class DictionaryCache
   {

      public class CacheUnit
      {

         private Guid _pkId;

         private readonly Dictionary<Guid, Dictionary> _guidDicItems = new Dictionary<Guid, Dictionary>();

         public CacheUnit(Guid id, List<Dictionary> dicList)
         {
            this._pkId = id;

            foreach (var dic in dicList)
            {
               _guidDicItems.Add(dic.ID, dic);
            }
         }

         public Guid PkId => _pkId;

         public Dictionary<Guid, Dictionary> GuidDicItems => _guidDicItems;


      }


      public static CacheUnit Cached(Guid parentId, bool getAll = false)
      {
         var unitDict = ThisAppCache.GetCache<Dictionary<Guid, CacheUnit>>();

         if (unitDict == null)
         {
            ThisAppCache.SetCache(unitDict = new Dictionary<Guid, CacheUnit>());
         }
         if (!unitDict.ContainsKey(parentId))
         {
            List<Dictionary> items;
            using (APDBDef db = new APDBDef())
            {
               var d = APDBDef.Dictionary;
               if (getAll)
                  items = db.DictionaryDal.ConditionQuery(null, null, null, null);
               else
                  items = db.DictionaryDal.ConditionQuery(d.ParentID == parentId, null, null, null);

               var cacheUnit = new CacheUnit(parentId, items);
               unitDict[parentId] = cacheUnit;
            }
         }

         return unitDict[parentId];
      }


      public static void ClearCache() => ThisAppCache.RemoveCache<Dictionary<Guid, CacheUnit>>();


      public static void RemoveCache(Guid parentId)
      {
         var cache = ThisAppCache.GetCache<Dictionary<Guid, CacheUnit>>();

         if (cache != null && cache.Count > 0 && cache.ContainsKey(parentId))
         {
            cache.Remove(parentId);
         }

      }

   }


   public static class CacheUnitExtension
   {

      public static IEnumerable<SelectListItem> GetSelectList(this CacheUnit unit, object selectValue)
      {
         foreach (var key in unit.GuidDicItems.Keys)
         {
            var value = unit.GuidDicItems[key].Value.ToString();


            yield return new SelectListItem()
            {
               Value = value,
               Text = unit.GuidDicItems[key].Title,
               Selected = selectValue != null
                          && string.Compare(selectValue.ToString(), value, true) == 0
            };
         }

      }


      public static Dictionary GetDictionaryByValue<T>(this CacheUnit unit, T val)
      {
         Dictionary dic = null;
         string value = string.Empty;

         if (val == null)
            return dic;

         foreach (var key in unit.GuidDicItems.Keys)
         {
            dic = unit.GuidDicItems[key];
            value = dic.Value == null ? null : dic.Value.ToString();

            if (string.Compare(val.ToString(), value, true) == 0)
               break;
         }

         return dic;
      }

      public static IEnumerable<SelectListItem> GetSelectListByValues<T>(this CacheUnit unit, T[] selectValue)
      {
         foreach (var key in unit.GuidDicItems.Keys)
         {
            var value = unit.GuidDicItems[key].Value.ToString();


            yield return new SelectListItem()
            {
               Value = value,
               Text = unit.GuidDicItems[key].Title,
               Selected = selectValue != null
                          && selectValue.ToList().Exists(val => string.Compare(val.ToString(), value, true) == 0)
            };
         }
      }


      public static IEnumerable<SelectListItem> GetSelectListById(this CacheUnit unit, Guid id)
      {
         foreach (var key in unit.GuidDicItems.Keys)
         {
            var dictionary = unit.GuidDicItems[key];

            yield return new SelectListItem()
            {
               Value = dictionary.ID.ToString(),
               Text = unit.GuidDicItems[key].Title,
               Selected = id == dictionary.ID
            };
         }
      }

      public static IEnumerable<SelectListItem> GetSelectListById(this CacheUnit unit, Guid id, SelectListItem defaultItem = null)
      {
         var result = GetSelectListById(unit, id);
         if (defaultItem != null && result != null && result.Count() > 0)
         {
            var list = result.ToList();
            list.Insert(0, defaultItem);

            return list;
         }

         return result;
      }


      public static Dictionary GetDictionaryById(this CacheUnit unit, Guid id)
      {
         var dic = new Dictionary();

         if (id.IsEmpty())
            return dic;

         foreach (var key in unit.GuidDicItems.Keys)
         {
            dic = unit.GuidDicItems[key];

            if (!id.IsEmpty() && id == dic.ID)
               break;
         }

         return dic;
      }

      public static List<Dictionary> GetSubDics(this CacheUnit unit, Guid typeId)
      {
         var dic = new List<Dictionary>();
         var u = Cached(typeId);
         var typesIsExist = u != null && u.GuidDicItems != null && u.GuidDicItems.Count > 0;
         if (typesIsExist)
         {
            foreach (var item in u.GuidDicItems)
            {
               dic.Add(item.Value);
            }
         }

         return dic;
      }

      public static List<Dictionary> GetAll(this CacheUnit unit)
      {
         var dic = new List<Dictionary>();
         foreach (var item in unit.GuidDicItems)
         {
            dic.Add(item.Value);
         }

         return dic;
      }

   }

}