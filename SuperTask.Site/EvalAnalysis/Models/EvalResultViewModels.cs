using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheSite.Models
{
   public class EvalResultViewModel
   {
      public string id { get; set; }
      public string periodId { get; set; }
      public string tableId { get; set; }
      public string accessorId { get; set; }
      public string accessorRoleId { get; set; }
      public string targetId { get; set; }
      public string targetRoleId { get; set; }
      public List<EvalResultItemViewModel> items { get; set; }
   }

   public class EvalResultItemViewModel
   {
      public string id { get; set; }
      public string resultId { get; set; }
      public string value { get; set; }
      public double score { get; set; }
      public string key { get; set; }
      public string indicationId { get; set; }
   }
}
