﻿using Business;
using Business.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TheSite.Models
{

	public class OperationHistoryViewModel
	{

		public string Date { get; set; }

		public string Operator { get; set; }

		public string Content { get; set; }

		public Guid ResultId { get; set; }

		public virtual string Display => string.Format("{0}  , {1}   ,{2}", Date, Operator, BugKeys.OperationResultDic[this.ResultId]);

	}


	public class OperationViewModel
	{

		public string Id { get; set; }

		public string Name { get; set; }

		public int SortId { get; set; }

		[Required]
		public Guid ProjectId { get; set; }

		[Required]
		public Guid? Result { get; set; }

		[Display(Name = "备注")]
		public string Remark { get; set; }

		public virtual string ReusltName { get; set; } // => RequireKeys.GetReviewResultByValue(Result.Value);


		public bool IsValid()
		{
			return !string.IsNullOrEmpty(Id)  && !ProjectId.IsEmpty() && Result!=null;
		}
	}


}