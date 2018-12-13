using Business.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Business
{

   public partial class Advice
   {
      
      [Display(Name ="角色类别")]
      public string TypeName { get; set; }

   }

}