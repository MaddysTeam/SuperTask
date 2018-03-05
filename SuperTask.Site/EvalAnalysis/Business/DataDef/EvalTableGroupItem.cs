using Business.Helper;
using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Business
{

   public partial class EvalTableGroupItem
   {
      public string TableName { get; set; }

      public string PeriodName { get; set; }
   }

}