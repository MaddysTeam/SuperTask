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

   public partial class EvalGroupMember
   {
      [Display(Name = "组成员")]
      public string MemberName { get; set; }
   }

   public partial class EvalGroupAccessor
   {
      [Display(Name="考评人")]
      public string AccessorName { get; set; }
   }


   public partial class EvalAccessorTarget
   {
      [Display(Name = "考评人")]
      public string AccessorName { get; set; }

      [Display(Name = "考评对象")]
      public string TargetName { get; set; }

      [Display(Name = "考核表")]
      public string TableName { get; set; }
   }

}