using Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheSite.EvalAnalysis;

namespace TheSite.Models
{

   public class EvalViewModel
   {
      public Guid AccessorId { get; set; }

      //public Guid AccessorRoleId { get; set; }

      public Role AccessorRole { get; set; }

      public Guid TargetRoleId { get; set; }

      public Guid TargetId { get; set; }

      public string TargetName { get; set; }

      public string AccessorName { get; set; }

      public List<EvalPeriodTable> PeriodTables { get; set; }

   }

}
