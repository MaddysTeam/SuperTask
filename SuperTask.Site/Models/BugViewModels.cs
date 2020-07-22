using Business;
using Business.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TheSite.Models
{

	public class BugConfrimViewModel: OperationViewModel
	{

		public override string ReusltName => BugKeys.GetResultByValue(Result.ToGuid(Guid.Empty));

		public string UserRealName { get; set; }

	}


	public class BugResolveViewModel : OperationViewModel
   {

	}


}