using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Business
{

   public partial class EvalResultItem
   {
     
      public Guid TableId { get; set; }

      public Guid PeriodId { get; set; }

      public EvalIndication EvalIndication { get; set; }

   }

}