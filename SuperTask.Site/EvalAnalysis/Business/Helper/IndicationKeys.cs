using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Helper
{

   public static class IndicationKeys
   {
      //指标项类型组ID
      public static Guid IndicaitonTypeKeyGuid = Guid.Parse("94decd1d-8c60-4c85-8f00-e740c1d6869b");

      public static Guid SubjectTypeGuid => Guid.Parse("94DECD1D-8C60-4C85-8F00-E740C1D7980B");

      public static Guid AutoTypeGuid => Guid.Parse("94DECD1D-8C60-4C85-8F00-E740C1D9960C");

      public static int SubjectType = 1;

      public static int AutoType = 2;

      public static int EnabelStatus = (int)IndicationStatus.enable;

      public static int DisableStatus = (int)IndicationStatus.disable;

      public static string GetIndicaitonTypeByValue<V>(V val)=> DictionaryHelper.GetDicByValue(IndicaitonTypeKeyGuid, val).Title;

   }


   public enum IndicationStatus
   {
      enable=1,
      disable=0
   }

}
