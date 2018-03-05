using System.ComponentModel.DataAnnotations;


namespace TheSite.Models
{

	public class ChgPwdViewModel
	{

		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "旧密码")]
		public string OldPassword { get; set; }


		[Required]
		[DataType(DataType.Password)]
		[StringLength(120, ErrorMessage = "密码需满足长度满6位", MinimumLength = 6)]
		[Display(Name = "新密码")]
		public string NewPassword { get; set; }


		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "确认密码")]
		[Compare("NewPassword", ErrorMessage = "密码和确认密码不匹配。")]
		public string ConfirmPassword { get; set; } 

	}

}
