using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Helper
{

   public static class AdviceKeys
   {

      public static Guid AdviceStatusTypeId => Guid.Parse("40604f69-28f9-4265-9213-6866cd190a64");

      public static Guid AdviceTypeId => Guid.Parse("1902bf04-b160-4f27-b599-fc86a08ae5df");

      //未启动
      public static Guid AdviceReadyGuid => Guid.Parse("555789da-2939-4b72-a78b-247761ab9af0");
      //已启动
      public static Guid AdviceStartGuid => Guid.Parse("a8d42b7a-35c0-4a61-add5-75057b5434b6");
      //已结束
      public static Guid AdvicetEndGuid => Guid.Parse("3fd17b46-f56f-43cf-91a9-dbe36253f6d4");


      //项目管理
      public static Guid ThisProjectType => Guid.Parse("2390f64b-36c3-4fc6-a8a2-d162bd8e6bde");


      public static string GetStatusKeyByValue<V>(V val) => DictionaryHelper.GetDicByValue(AdviceStatusTypeId, val).Title;

      public static string GetTypeKeyByValue<V>(V val) => DictionaryHelper.GetDicByValue(AdviceTypeId, val).Title;

   }

}
