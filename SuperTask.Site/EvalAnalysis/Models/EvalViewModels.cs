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
      public EvalPeriod Period { get; set; }

      public UserInfo Accessor { get; set; }

      public UserInfo Target { get; set; }

      public List<EvalTable> EvalTables { get; set; }

      public Dictionary<Guid,List<EvalResultItem>> EvalResultItems { get; set; }

   }

}
