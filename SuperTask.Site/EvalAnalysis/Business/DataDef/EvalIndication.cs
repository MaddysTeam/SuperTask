using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Business
{

   public partial class EvalIndication
   {

      public List<EvalIndicationItem> Items { get; set; }

      public string IndicationName { get; set; }

      public string IndicationDescription { get; set; }

      public string AccessorRoleName { get; set; }

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

   }

}