using Business.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Helper
{

   public static class TaskKeys
   {
      /// <summary>
      /// 默认任务名
      /// </summary>
      public static string DefaultTaskName = "新建任务";

      /// <summary>
      /// 任务类型
      /// </summary>
      public static Guid TypeGuid = Guid.Parse("DD3ADC7F-A56C-3C58-9CAF-C2E3D6C9DC6B");

      /// <summary>
      /// 任务状态
      /// </summary>
      public static Guid StatusGuid = Guid.Parse("dd3adc7f-a55c-3c58-9caf-d3a2b7a9dc7b");

      /// <summary>
      /// 任务文档类型,需要Dictionary表里创建，需要用于下拉框的数据
      /// </summary>
      public static Guid FileTypeGuid = Guid.Parse("DE3ADC7F-A55C-3C58-9CAF-D3A2B7A9DC1B");

      /// <summary>
      /// 任务范围类型,需要Dictionary表里创建，需要用于下拉框的数据
      /// </summary>
      public static Guid RangeTypeGuid = Guid.Parse("94DECD1D-8C60-4C85-8F00-E635C1D5540A");


      public static Guid SelectAll = Guid.Parse("F96E81D7-5B44-A26C-BE35-45FCBA6BE8DE");


      public static string GetTypeKeyByValue(Guid val) => DictionaryHelper.GetDicById(TypeGuid, val).Title;

      public static string GetStatusKeyByValue(Guid val) => DictionaryHelper.GetDicById(StatusGuid, val).Title;

      public static string GetFileTypeKeyByValue(Guid val) => DictionaryHelper.GetDicById(FileTypeGuid, val).Title;


      public static Guid DeleteStatus => Guid.Parse("CC3CDC7F-A46D-3C58-9CAF-D3A2B7C9AB8D");

      public static Guid PlanStatus => Guid.Parse("CC3CDC7F-A46D-3C58-9CAF-D3A2B7C9AB5D");

      public static Guid CompleteStatus => Guid.Parse("CC3CDC7F-A46D-3C58-9CAF-D3A2B7C9AB7D");

      public static Guid ProcessStatus => Guid.Parse("CC3CDC7F-A46D-3C58-9CAF-D3A2B7C9AB6D");

      public static Guid TaskTempEditStatus => Guid.Parse("CC3CDC7F-A46D-3C58-9CAF-D3A2B7C9AB9D");

      public static Guid ReviewStatus => Guid.Parse("CC3CDC7F-A46D-3C58-9CAF-D3A2B7C9AC1D");


      public static Guid ProjectTaskType => Guid.Parse("DD3ABC1D-A33C-3C68-8ACF-C2E5D6C9DC8B");

      public static Guid PlanTaskTaskType = Guid.Parse("DD4BBC8D-A53C-3D68-8ACF-C2E5D7C9DC8B");

      public static Guid TempTaskType => Guid.Parse("DD3ABC1D-A33C-3C68-8ACF-C2E5D6C9DC7B");

      public static Guid MaintainedTaskType => Guid.Parse("DD3ABC1D-A33C-3C68-8ACF-C2E5D6C9DC9C");

      public static Guid DocumentTaskType = Guid.Parse("DE3ABC1D-A33C-3C68-8ACF-C2E5D6C9DC9C");

      public static Guid ManageTaskType = Guid.Parse("DF3ABC1D-A33C-3C68-8ACF-C2E5D6C9DC9C");

      public static Guid DefaultType => Guid.Parse("77AAD589-E95B-839E-C1F5-55027AE3BC14");

      public static Guid DefaultSubType => Guid.Parse("459BB5D3-1651-456C-8BAB-CEDE169EABBB");


      public static Guid TaskEditPlanType => Guid.Parse("ABA6D06D-15E8-4A33-9C02-805675D631FF"); //TODO Add in dic

      public static Guid TaskEditStartType => Guid.Parse("6937E453-15A0-F5FE-C807-DC4B12EE9580"); //TODO Add in dic

      public static Guid TaskEditCompleteType => Guid.Parse("E42E2E11-90C0-685A-C4AB-5D5D2B4E34CB"); //TODO Add in dic

      public static Guid TaskEditDeleteType => Guid.Parse("AC63467D-3632-0C38-BF39-61D23AD49D92"); //TODO Add in dic

      public static Guid TaskTransferType => Guid.Parse("8CB4510B-E443-1674-30BB-5E00C83E1E83"); //TODO Add in dic

      public static int TaskNameDisplayLength = 10;

      public static double DefaultEstimateHours = 0;


      public static Guid RequreFileType => Guid.Parse("0E559F87-3B5A-5C23-B724-EE79E7AADFFC"); //TODO Add in dic

      public static Guid DesignFileType => Guid.Parse("982973B1-33DC-F846-C6F4-504EE65DF887"); //TODO Add in dic

      public static Guid CheckAndAcceptFileType => Guid.Parse("4BA83CAC-853A-676B-2E04-896D722098AF"); //TODO Add in dic

      public static Guid DeliverFileType => Guid.Parse("729A87BE-3B00-16FD-FC5E-05046FB630BB"); //TODO Add in dic

      public static Guid DefaultFileType => Guid.Parse("729A87CE-3B01-16FD-FC5E-05046FB630BB"); //TODO Add in dic


      public static Guid SendToMeRangeType => Guid.Parse("94DECD2D-8C60-4C85-8F00-E635C1D5640C");

      public static Guid SendByMeRangeType => Guid.Parse("94DECD3D-8C60-4C85-8F00-E635C1D5640D");

      public static Guid ReviewByMeRangeType => Guid.Parse("94DECD3D-8C60-4C85-8F00-E635C1D5740E");


      public static Guid SearchByDetaultType => Guid.Parse("FEB2DF18-4A57-0BE6-B44B-1C16EE6528BC"); //TODO Add in dic

      public static Guid SearchByProject => Guid.Parse("3836EA7E-906C-5388-7D6B-274FA203C569"); //TODO Add in dic

      public static Guid SearchByPersonal => Guid.Parse("C0797B23-521C-E8B5-041A-C039ABB17C20"); //TODO Add in dic

   }

}
