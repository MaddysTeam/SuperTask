using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheSite.Models;

namespace Business.Helper
{

   public static class MilestoneKeys
   {

      // 里程碑类型
      public static Guid MileStoneTypeKeyGuid = Guid.Parse("06f9c37f-aa87-44ab-8c8a-3572fe2231f9");
      public static Guid DefaultType = Guid.Parse("ac18dd29-e5c2-4715-ba91-fdf2c6d70e38");
      public static Guid BusinessType = Guid.Parse("ac18dd29-e5c2-4715-ba91-fdf2c6d70e38");
      public static Guid DevelopType = Guid.Parse("ac18dd29-e5c2-4715-ba91-fdf2c6d70e38");
      public static Guid MaintainceType = Guid.Parse("ac18dd29-e5c2-4715-ba91-fdf2c6d70e38");

      // 里程碑状态
      public static Guid MileStoneStatusKeyGuid = Guid.Parse("8a923d41-d253-4cb2-90f9-e308499dcb97");
      public static Guid EnableStatus = Guid.Parse("2447a71f-5922-432f-a343-59dbcf7ad282");
      public static Guid DisableStatus = Guid.Parse("8a19003e-0dfe-4825-a34b-c7434e81798f");


      /// <summary>
      /// 获取项目状态名称
      /// </summary>
      /// <param name="val">状态值</param>
      /// <returns>数据字典</returns>
      public static string GetStatusKeyById(Guid statusId) => DictionaryHelper.GetDicById(MileStoneStatusKeyGuid, statusId).Title;


      /// <summary>
      /// 获取项目状类型名称
      /// </summary>
      /// <param name="val">状态值</param>
      /// <returns>数据字典</returns>
      public static string GetTypeKeyById(Guid typeId) => DictionaryHelper.GetDicByValue(MileStoneTypeKeyGuid, typeId).Title;

   }

}
