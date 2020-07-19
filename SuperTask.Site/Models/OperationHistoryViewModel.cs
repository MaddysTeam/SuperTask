using Business;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TheSite.Models
{

   public class OperationHistoryViewModel
   {

      public string Date { get; set; }

      public string Operator { get; set; }

      public string Content { get; set; }

      public string Display => string.Format("{0}  , {1}   ,{2}",Date,Operator,Content);

   }

}