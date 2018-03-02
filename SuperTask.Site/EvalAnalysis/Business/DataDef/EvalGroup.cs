using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Business
{

   public partial class EvalGroup
   {
      public List<UserInfo> Accessors { get; set; }
      public List<UserInfo> TargetMembers { get; set; }
   }


   public partial class EvalGroupAccessor
   {
      [Display(Name="考评人")]
      public string AccessorName { get; set; }
   }


   public partial class EvalGroupTarget
   {
      [Display(Name = "考评对象")]
      public string TargetName { get; set; }
   }

}