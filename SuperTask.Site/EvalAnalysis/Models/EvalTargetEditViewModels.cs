using Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheSite.Models
{
   public class EvalTargetEditViewModels
   {

      public List<EvalTable> Tables { get; set; }

      public List<EvalAccessorTarget> AccessorsAndTargets { get; set; }

      public List<EvalTargetTablePropertion> TablePropertion { get; set; }

      public EvalAccessorTarget CurrentTarget { get; set; }

      public bool TargetIsGroup { get; set; }

   }
}
