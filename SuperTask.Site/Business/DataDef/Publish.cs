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

		public string Type => PublishKeys.GetTypeKeyByValue(PublishType);

		public string Status => PublishKeys.GetStatusKeyByValue(PublishStatus);

		public List<TheSite.Models.OperationHistoryViewModel> OperationHistory { get; set; }

		public List<WorkTask> RelativeTasks { get; set; } = new List<WorkTask>();

		public List<Bug> RelativeBugs => new List<Bug>();

		public List<Require> RelativeRequires = new List<Require>();

      public string RelativeTaskIds { get; set; }

      public string RelativeRequireIds { get; set; }

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

      [Display(Name = "完成日期")]
      public string EndDateStr => this.EndDate.ToyyMMdd();

      [Display(Name ="关闭日期")]
		public string CloseDateStr => this.CloseDate.ToyyMMdd();

      [Display(Name = "创建日期")]
      public string CreateDateStr => this.CreateDate.ToyyMMdd();


   }

}