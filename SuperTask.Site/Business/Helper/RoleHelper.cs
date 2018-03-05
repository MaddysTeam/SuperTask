using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using TheSite.Models;

namespace Business.Helper
{

   public class RoleHelper
   {
      static APDBDef.RoleTableDef r = APDBDef.Role;
      static APDBDef.UserRoleTableDef ur = APDBDef.UserRole;

      public static List<Role> GetUserRoles(Guid userId,APDBDef db)
      {
         var result = APQuery.select(r.RoleName,r.RoleId)
                            .from(ur,r.JoinInner(ur.RoleId==r.RoleId))
                            .where(ur.UserId==userId)
                            .query(db,rd=> 
                            {
                               return new Role
                               {
                                  RoleId = r.RoleId.GetValue(rd),
                                  RoleName = r.RoleName.GetValue(rd)
                               };
                            }).ToList();
         

         return result;
      }

   }

}