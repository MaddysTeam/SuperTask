using Business.Helper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Business
{

	public partial class Publish
	{

		[Display(Name = "指派人员")]
		public string Manager { get; set; }

		public string Creator { get; set; }

		[Display(Name = "项目")]
		public string ProjectName => Project.PrimaryGet(Projectid)?.ProjectName;

		public Attachment CurrentAttachment { get; set; }

		public string Type => RequireKeys.GetTypeKeyByValue(PublishType);

		public string Status => RequireKeys.GetStatusKeyByValue(PublishStatus);

		public List<TheSite.Models.OperationHistoryViewModel> OperationHistory { get; set; }

		public List<WorkTask> RelativeTasks => new List<WorkTask>();

		public List<Bug> RelativeBugs => new List<Bug>();

		public List<Require> RelativeRequires = new List<Require>();

		[Required]
		public override string PublishName
		{
			get
			{
				return base.PublishName;
			}

			set
			{
				base.PublishName = value;
			}
		}

		public string EndDateStr => this.EndDate.ToyyMMdd();

		public string CloseDateStr => this.CloseDate.ToyyMMdd();

		public string CreateDateStr => this.CreateDate.ToyyMMdd();

	}

}