using Business.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheSite.Models;

namespace Business
{

   public partial class EvalIndication
   {

      public List<EvalIndicationItem> Items { get; set; }

      public string IndicationName { get; set; }

      public Guid IndicationType { get; set; }

      public override bool Equals(object obj)
      {
         var other = obj as EvalIndication;

         if (other == null) return false;

         return Id == other.Id;
      }

      public override int GetHashCode()
      {
         return base.GetHashCode();
      }

      public Result Validate()
      {
         var message = string.Empty;
         var result = true;

         if (TableId.IsEmpty())
         {
            message = Errors.EvalTable.NOT_ALLOWED_ID_NULL;
            result = false;
         }

         return new Result { IsSuccess = result, Msg = message };
      }

   }

}