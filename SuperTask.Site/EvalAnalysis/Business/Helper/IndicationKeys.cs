﻿using System;
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

      public static Guid SubjectType => Guid.Parse("b1c531ff-1d90-4945-9e6f-b5ac2b42cf5e");

      public static Guid AutoType => Guid.Parse("456eb9ec-17a7-47a5-a6ba-61aa29cc7b12");

      public static Guid EnabelStatus => Guid.Parse("b63aed71-0cb9-4559-86ec-481e557d68a7");

      public static Guid DisableStatus => Guid.Parse("0d6c4b2b-ffc2-4ac7-ad57-a2cef08e6af8");

      public static string GetIndicaitonTypeByValue<V>(V val)=> DictionaryHelper.GetDicByValue(IndicaitonTypeKeyGuid, val).Title;

   }

}
