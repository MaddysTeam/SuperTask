using Business.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Business
{

   public partial class Role
   {

      public List<App> Permissions { get; private set; }

      [Display(Name = "角色类型")]
      public string Type => RoleKeys.Types[RoleType == 0 ? RoleKeys.SystemType : RoleType];

      public void SetPermission(List<App> permissions)
      {
         this.Permissions = permissions;
      }


   }

}