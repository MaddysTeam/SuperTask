using Business.Config;
using Symber.Web.Report;
using System;
using System.Collections.Generic;

namespace Business.Helper
{

	public static class DictionaryHelper
   {

      public static Dictionary GetDicByValue<V>(Guid typeId,V val) 
         => DictionaryCache.Cached(typeId).GetDictionaryByValue(val);

      public static Dictionary GetDicById(Guid parentId, Guid id) 
         => DictionaryCache.Cached(parentId).GetDictionaryById(id);

      public static List<Dictionary> GetSubTypeDics(Guid typeId)
         => DictionaryCache.Cached(typeId).GetSubDics(typeId);

      public static List<Dictionary> GetAll()
        => DictionaryCache.Cached(Guid.Empty, true).GetAll();

   }
}

