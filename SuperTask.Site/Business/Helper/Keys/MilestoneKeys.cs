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
      public static Guid BusinessType = Guid.Parse("d8a25e1d-13e2-4ced-8a52-fc90e2272155");
      public static Guid DevelopType = Guid.Parse("fd732f82-b77c-4377-9f9d-14f2c14eb8a1");
      public static Guid MaintainceType = Guid.Parse("acfd0142-63d3-41e4-ab39-b0f089baf347");

      // 里程碑状态
      public static Guid MileStoneStatusKeyGuid = Guid.Parse("8a923d41-d253-4cb2-90f9-e308499dcb97");
      //public static Guid EnableStatus = Guid.Parse("2447a71f-5922-432f-a343-59dbcf7ad282");
      //public static Guid DisableStatus = Guid.Parse("8a19003e-0dfe-4825-a34b-c7434e81798f");

      public static Guid ReadyStatus => Guid.Parse("82bd3c82-3041-46a2-a3aa-dabba89bbf58");
      public static Guid StartStatus => Guid.Parse("be4e03bb-cfdc-4d97-8d1b-18936f52cbe7");
      public static Guid ProcessStatus => Guid.Parse("4267a68c-2678-49cb-9d6b-cec1e1c9dc9c");
      public static Guid NearlyDoneStatus => Guid.Parse("2a578d7c-9eaa-4784-b3a3-23057b324ab5");
      public static Guid CompleteStatus => Guid.Parse("9d48d041-e2ca-4303-aa53-76a50453d490");


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
