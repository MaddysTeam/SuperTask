using Business.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Business
{

	public partial class Bug
	{

		public string Manager { get; set; }

		public string Creator { get; set; }

		[Display(Name ="项目")]
		public string ProjectName { get; set; }

		[Display(Name = "浏览器")]
		public string BrowserName { get; set; }

		[Display(Name = "操作系统")]
		public string SystemName { get; set; }

		public Attachment CurrentAttachment { get; set; }

		[Required]
		public override string BugName
		{
			get
			{
				return base.BugName;
			}

			set
			{
				base.BugName = value;
			}
		}

	}



}