using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheSite.Models
{

   public class UserRoleViewModel
   {

     public string RoleName { get; set; }
     public Guid RoleId { get; set; }
     public string UserName { get; set; }
     public Guid UserId { get; set; }
     public bool IsChecked { get; set; }

   }

}
