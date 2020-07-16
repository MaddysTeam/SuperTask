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

		public Attachment CurrentAttachment { get; set; }

	}



}