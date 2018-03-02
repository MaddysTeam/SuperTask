using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheSite.Models
{

   public class RoleAppViewModel
   {

     public string RoleName { get; set; }
     public Guid RoleId { get; set; }
     public string AppName { get; set; }
     public Guid AppId { get; set; }
     public bool IsChecked { get; set; }

   }

}
