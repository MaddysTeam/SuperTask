using Business.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheSite.Models;

namespace Business
{
   public partial class UserInfo
   {

      public List<Role> Roles { get;  set; }

      public List<App> Permissions { get; set; }

      public List<WorkTask> Tasks { get; set; }

      public List<Project> Projects { get; set; }


      public void SetRoles(List<Role> roles)
      {
         Roles = roles;
      }


      public void SetPermissions(List<App> permissions)
      {
         Permissions = permissions;
      }


      public bool HasRole(Guid roleId)
      {
         return Roles.Exists(x => x.RoleId == roleId);
      }


      public bool HasPermission(string permission)
         => Permissions.Exists(p => 
         p.Address.IndexOf(permission) >= 0);



      public static UserInfo Initial(Guid id, string name)
      {
         var user = new UserInfo();
         user.UserId = id;
         user.UserName = name;
         user.Birthday = DateTime.MinValue;

         return user;
      }

      public bool IsBoss => UserId.ToString() == "D1E6E02A-40FF-4F5A-80C3-24710996B9AE"; // HasRole(RoleKeys.LEADER.ToGuid(Guid.Empty));
      public bool IsManager => HasRole(RoleKeys.ProjectManagerId.ToGuid(Guid.Empty));

    
      public Result Validate()
      {
         var result = Result.Initial();

         if (string.IsNullOrEmpty(UserName))
         {
            result.Msg = Errors.User.NOT_ALLOWED_NAME_NULL;
            result.IsSuccess= false;
         }

         if(Roles == null || Roles.Count <= 0)
         {
            result.Msg = Errors.User.NOT_BINDING_ROLES;
            result.IsSuccess = false;
         }

         return result;
      }


   }

}
