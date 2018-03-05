using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Helper
{

   public static class EvalTableKeys
   {
      //指标项类型组ID

      public static Guid TableTypeKeyGuid = Guid.Parse("94decd1d-8c60-4c85-8f00-e740c1d9999b");

      public static Guid TableStatusKeyGuid = Guid.Parse("94decd1d-8c60-4c85-8f00-e735c1d8870b");

      public static int SubjectType = (int)EvalTableType.SubjectType;

      public static int AutoType = (int)EvalTableType.AutoType;

      public static Guid SubjectTypeGuid => Guid.Parse("94DECD1D-8C60-4C85-8F00-E740C1D1111B");

      public static Guid AutoTypeGuid => Guid.Parse("94DECD1D-8C60-4C85-8F00-E740C1D2222B");

      public static int ReadyStatus = (int)IndicationStatus.enable;

      public static int DisableStatus = (int)IndicationStatus.disable;

      public static int ProcessStatus = (int)EvalTableStatus.processing;

      public static int DoneStatus = (int)EvalTableStatus.done;

      public static Guid DisableStatusGuid => Guid.Parse("94DECD1D-8C60-4C85-8F00-E735C1D4430B");

      public static Guid ProcessStatusGuid => Guid.Parse("94DECD1D-8C60-4C85-8F00-E735C1D6650B");

      public static Guid ReadyStatusGuid => Guid.Parse("94DECD1D-8C60-4C85-8F00-E735C1D7760B");

      public static Guid DoneStatusGuid => Guid.Parse("94DECD1D-8C60-4C85-8F00-E735C1D5540B");


      public static string GetTableTypeByValue<V>(V val)=> DictionaryHelper.GetDicByValue(TableTypeKeyGuid, val).Title;

      public static string GetTableStatusByValue<V>(V val) => DictionaryHelper.GetDicByValue(TableStatusKeyGuid, val).Title;

   }


   public enum EvalTableType
   {
      SubjectType=1,
      AutoType=2
   }

   public enum EvalTableStatus
   {
      disable = 0,//禁用
      //ready=1,//量表准备中
      processing=2,//量表制作中
      done=3//两表制作完毕
   }



}
