using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheSite.Models
{

   public class RegisterViewModel
   {

      [Required]
      [Display(Name = "用户名")]
      public string Username { get; set; }


      [Required]
      [DataType(DataType.Password)]
      [Display(Name = "密码")]
      public string Password { get; set; }


      [Required]
      [DataType(DataType.Password)]
      [Display(Name = "确认密码")]
      public string ConfirmPassword { get; set; }

   }

}
