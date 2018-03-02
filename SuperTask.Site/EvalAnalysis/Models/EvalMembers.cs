using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheSite.Models
{

   public class EvalMember
   {
      public Guid MemberId { get; set; }

      public Guid AccessorId { get; set; }

      public string TableName { get; set; }

      public string AccessorName { get; set; }

      public string MemberName { get; set; }

      public bool IsEvaled => EvalCount > 0;

      public string EvalStatus => IsEvaled ? "已评" : "未评";

      public int EvalCount { get; set; }

      public string TableIds { get; set; }

      public string PeriodNames { get; set; }

      public bool HasTable => !string.IsNullOrEmpty(TableIds);

      public Guid PeriodId { get; set; }

      public string TargetRoleName { get; set; }

      public Guid TargetRoleId { get; set; }


      public override bool Equals(object obj)
      {
         var em = obj as EvalMember;


         return em.AccessorId == AccessorId && em.MemberId == em.MemberId;
      }

      public override int GetHashCode()
      {
         return base.GetHashCode();
      }
   }

}