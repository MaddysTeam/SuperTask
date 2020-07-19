using Business;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TheSite.Models
{

   public class BugConfrimViewModel
   {

      [Display(Name="Bug ID")]
      public int SortId { get; set; }

      [Display(Name = "Bug 名称")]
      public string BugName { get; set; }

      [Required]
      public string BugIds { get; set; }

      [Required]
      public Guid ProjectId { get; set; }

      [Required]
      [Display(Name ="结果")]
      public Guid? Result { get; set; }

      [Display(Name = "备注")]
      public string Remark { get; set; }

   }


   public class BugResolveViewModel
   {

      [Display(Name = "Bug ID")]
      public int SortId { get; set; }

      [Display(Name = "Bug 名称")]
      public string BugName { get; set; }

      [Required]
      public string BugIds { get; set; }

      [Required]
      public Guid ProjectId { get; set; }

      [Required]
      [Display(Name = "状态")]
      public Guid? Status { get; set; }

      [Display(Name = "备注")]
      public string Remark { get; set; }

   }


}