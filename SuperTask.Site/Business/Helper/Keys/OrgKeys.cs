using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Business.Helper
{

   public static class OrgKeys
   {

      #region 【TODO: 预留】


      public static Guid OrgKey = Guid.Parse("04F12BEB-D99D-43DF-AC9A-3042957D6BDA");

      //TODO: will delete later
      public static Guid TempKey = Guid.Parse("475F4D51-1A10-4A52-8443-BB4F285513B8");

      /// <summary>
      /// 获取部门名称
      /// </summary>
      /// <param name="val">状态值</param>
      /// <returns>数据字典</returns>
      public static string GetOrgKeyByValue<V>(V val) => DictionaryHelper.GetDicByValue(OrgKey, val).Title;


      #endregion

   }

}