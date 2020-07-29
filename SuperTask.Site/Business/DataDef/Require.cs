﻿using Business.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Business
{

	public partial class Require
	{
		[Display(Name = "指派人员")]
		public string Manager { get; set; }

		public string Creator { get; set; }

		[Display(Name = "项目")]
		public string ProjectName => Project.PrimaryGet(Projectid)?.ProjectName;

		public Attachment CurrentAttachment { get; set; }

		public string Level => RequireKeys.GetLevelByValue(RequireLevel);

		public string Type => RequireKeys.GetTypeKeyByValue(RequireType);

		public string Status => RequireKeys.GetStatusKeyByValue(RequireStatus);

		public List<TheSite.Models.OperationHistoryViewModel> OperationHistory { get; set; }

		public List<WorkTask> RelativeTasks { get; set; } = new List<WorkTask>();

		public List<Bug> RelativeBugs { get; set; } = new List<Bug>();

		public List<Publish> RelativePublishs { get; set; } = new List<Publish>();

		[Required]
		public override string RequireName
		{
			get
			{
				return base.RequireName;
			}

			set
			{
				base.RequireName = value;
			}
		}


		//public string ReviewResult { get; set; } = string.Empty;


		public string EvalDateStr => this.ReviewDate.ToyyMMdd();

		public string EstimateEndDateStr => this.EstimateEndDate.ToyyMMdd();

		public string StartDateStr => this.StartDate.ToyyMMdd();

		public string EndDateStr => this.EndDate.ToyyMMdd();

		public string CloseDateStr => this.CloseDate.ToyyMMdd();

		public string CreateDateStr => this.CreateDate.ToyyMMdd();

	}



}