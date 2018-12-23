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

      // 项目里程碑状态（TODO:暂时固定为5个阶段，未开始，开始阶段，执行中，即将结束，已结束）
      public static Guid ProejctMilestoneReadyStatus => Guid.Parse("82bd3c82-3041-46a2-a3aa-dabba89bbf58");
      public static Guid ProejctMilestoneStartStatus => Guid.Parse("be4e03bb-cfdc-4d97-8d1b-18936f52cbe7");
      public static Guid ProejctMilestonProcessStatus => Guid.Parse("4267a68c-2678-49cb-9d6b-cec1e1c9dc9c");
      public static Guid ProejctMilestonNearlyDoneStatus => Guid.Parse("2a578d7c-9eaa-4784-b3a3-23057b324ab5");
      public static Guid ProejctMilestonCompleteStatus => Guid.Parse("9d48d041-e2ca-4303-aa53-76a50453d490");

      public static Dictionary<string, Guid> MileStoneStatusList => new Dictionary<string, Guid>()
      {
         {"未开始",ProejctMilestoneReadyStatus},
         //{"未开始",ProejctMilestoneReadyStatus},
         //{"未开始",ProejctMilestoneReadyStatus},
         //{"未开始",ProejctMilestoneReadyStatus},
         //{"未开始",ProejctMilestoneReadyStatus},
      };


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
