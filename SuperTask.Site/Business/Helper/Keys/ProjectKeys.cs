using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheSite.Models;

namespace Business.Helper
{

   public static class ProjectKeys
   {

      public static Guid SelectAll = Guid.Parse("F96E81D7-5B44-A26C-BE35-45FCBA6BE8DE");

      //项目状态类型组ID
      public static Guid ProjectStatusKeyGuid = Guid.Parse("DF2ACD4F-A45D-4C36-9CAF-C2E3D5C9DD2C");

      //项目类型组ID
      public static Guid ProjectTypeKeyGuid = Guid.Parse("dd3adc5f-a45d-4c36-9caf-c2e3d5c9dd3c");


      public static Guid DeleteStatus = Guid.Parse("DF2ADD4A-A45C-4D57-9CCF-C2D3C5C9DD3D");

      public static Guid PlanStatus = Guid.Parse("DF2ACC5A-A35C-4D46-9CBF-C2D4C5C9DC4C");

      public static Guid CompleteStatus = Guid.Parse("DF2ADC6A-A35C-4A47-9CBF-C2D4C5C9DC5C");

      public static Guid ProcessStatus = Guid.Parse("DF2ADD5A-A45C-4D47-9CCF-C2D3C5C9DD2D");

      public static Guid EditStatus = Guid.Parse("DF2ADC6A-A35C-4A47-9CBF-C2D4C5C9DC6C");

      public static Guid ReviewStatus = Guid.Parse("DF2ADC6A-A35C-4A47-9CBF-C2D4C5C9DB6C");

      public static Guid ForceCloseStatus = Guid.Parse("DF2ADC7D-B45C-4B57-9CBF-C2D4C5C8CC5C");


      public static Guid DefaultProjectType = Guid.Parse("DF2ADC7b-A32C-4B47-9CBF-C2D4C5C9DB7C"); // TODO

      public static Guid DevelopmentProjectType = Guid.Parse("b3246e56-ccce-43f0-b379-c39cbc41eaa7");

      public const string ProjectCodePrefix = "LX_";

      public const int DefaultDateRange = 10;


      /// <summary>
      /// 获取项目状态名称
      /// </summary>
      /// <param name="val">状态值</param>
      /// <returns>数据字典</returns>
      public static string GetStatusKeyById(Guid statusId) => DictionaryHelper.GetDicById(ProjectStatusKeyGuid, statusId).Title;


      /// <summary>
      /// 获取项目状类型名称
      /// </summary>
      /// <param name="val">状态值</param>
      /// <returns>数据字典</returns>
      public static string GetTypeKeyById(Guid typeId) => DictionaryHelper.GetDicByValue(ProjectTypeKeyGuid, typeId).Title;

   }

}
