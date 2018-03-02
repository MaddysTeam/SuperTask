using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Helper
{

   public static class EvalKeys
   {

      public static int SubjectType = (int)EvalTableType.SubjectType;

      public static int AutoType = (int)EvalTableType.AutoType;

   }


   public enum EvalType
   {
      SubjectType=1,
      AutoType=2
   }

}
