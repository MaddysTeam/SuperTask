using Business;
using Business.Helper;
using Business.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TheSite.Models;


namespace Business
{

	public partial class WorkTask
	{

		[Display(Name = "创建人")]
		public string Creator { get; set; }

		[Display(Name = "负责人")]
		public string Manager { get; set; }

		[Display(Name = "审核人")]
		public string Reviewer { get; set; }

		[Display(Name = "任务状态")]
		public string Status => TaskKeys.GetStatusKeyByValue(TaskStatus);

		[Display(Name = "所属项目")]
		public string ProjectName { get; set; }

		[Display(Name = "父任务")]
		public string ParentTaskName { get; set; }

		[Display(Name = "子类型")]
		public string SubType { get; set; }


		public string V2LevelTitle => TaskKeys.GetV2LevelByValue(V2Level);

		public string Type => TaskKeys.GetTypeKeyByValue(TaskType);

		public bool IsPlanStatus => TaskStatus == TaskKeys.PlanStatus;

		public bool IsProcessStatus => TaskStatus == TaskKeys.ProcessStatus;

		public bool IsCompleteStatus => TaskStatus == TaskKeys.CompleteStatus;

		public bool IsCloseStatus => TaskStatus == TaskKeys.CloseStatus;

		public bool IsDelteStatus => TaskStatus == TaskKeys.DeleteStatus;

		public bool IsProjectTaskType => TaskType != TaskKeys.TempTaskType;

		public bool IsProjectTypeOnly => TaskType == TaskKeys.ProjectTaskType;

		public bool IsTempTaskType => !IsProjectTaskType;

		public bool HasArrangement => TaskLevel > 0 && SortId > 0;

		public bool HasPlan => EstimateWorkHours > 0;

		public bool HasSubType => TaskType != TaskKeys.ProjectTaskType && TaskType != TaskKeys.TempTaskType;

		public string DataUrl { get; set; }

		public bool IsRoot => ParentId.IsEmpty();

		public Attachment CurrentAttachment { get; set; }

		public override Guid TaskType
		{
			get
			{
				return base.TaskType;
			}
			set
			{
				base.TaskType = IsParent ? TaskKeys.ProjectTaskType : value;
			}
		}

		public override Guid SubTypeId
		{
			get
			{
				return base.SubTypeId;
			}

			set
			{
				base.SubTypeId = IsProjectTypeOnly ? Guid.Empty : value;
			}
		}

		public double SubValueScore { get; set; }

		/// <summary>
		/// 这个字段为了计算任务完成率,作为分子使用
		/// </summary>
		public double Numerator { get; set; }

		public Action<WorkTask> WhenStart { get; set; }

		/// <summary>
		/// 子任务单位名称，例如【个】，【篇】等
		/// </summary>
		public string SubTaskTypeUnitName { get; set; }

		/// <summary>
		/// 子任务具体名称
		/// </summary>
		public string SubTaskTypeName { get; set; }

		/// <summary>
		/// 子任务工作量累计得分
		/// </summary>
		[Display(Name = "累计得分")]
		public double SubTaskScore { get; set; }

		/// <summary>
		/// 标准复杂度（TODO:已停用，以后删除）
		/// </summary>
		public int StandardComplextiy { get; set; }

		/// <summary>
		/// 标准工时（TODO:已停用，以后删除）
		/// </summary>
		public double StandardWorkhours { get; set; }

		/// <summary>
		/// 相关需求id  TODO:20200727 
		/// </summary>
		public string RelativeRequireIds { get; set; }

		/// <summary>
		/// 相关bug id TODO:20200727  
		/// </summary>
		public string RelativeBugIds { get; set; }

		/// <summary>
		/// 相关发布 id TODO:20200727  
		/// </summary>
		public string RelativePublishIds { get; set; }

      /// <summary>
      /// 操作历史 TODO:20200727  
      /// </summary>
      public List<TheSite.Models.OperationHistoryViewModel> OperationHistory { get; set; }


      public string RealStartDateStr => this.RealStartDate.ToyyMMdd();

      public string RealEndDateStr => this.RealEndDate.ToyyMMdd();

   }

	public partial class WorkTask
	{

		public bool IsWorked => WorkHours > 0 || SubTypeValue > 0;

		public bool IsPlanTask => TaskType == TaskKeys.PlanTaskTaskType;

		public void SetStatus(Guid status) => this.TaskStatus = status;

		public virtual void SetEsitmateDate(DateTime start, DateTime end)
		{
			StartDate = start;
			EndDate = end;
		}

		public virtual void Start()
		{
			SetStatus(TaskKeys.ProcessStatus);
			RealStartDate = DateTime.Now;

			WhenStart?.Invoke(this);
		}

		public virtual void Complete(DateTime realEndDate)
		{
			RealEndDate = realEndDate;
			SetStatus(TaskKeys.CompleteStatus);
			RateOfProgress = 100;
		}

		public virtual void Close(DateTime closeDate)
		{
			CloseDate = closeDate;
			SetStatus(TaskKeys.CloseStatus);
		}

		public void SetParentProgress()
		{
			if (IsParent)
			{
				var denominator = EstimateWorkHours == 0 ? 1 : EstimateWorkHours;
				var progress = RateOfProgress == 100 ? 100 :
					  decimal.Round((decimal)(Numerator / denominator), 2) * 100;

				RateOfProgress = (double)progress;
			}
		}

		public Result Validate()
		{
			var message = Success.Task.EDIT_SUCCESS;
			var result = true;

			//if (TaskId.IsEmpty())
			//{
			//   message = Errors.Task.NOT_ALLOWED_ID_NULL;
			//   result = false;
			//}
			if (string.IsNullOrEmpty(TaskName))
			{
				message = Errors.Task.NOT_ALLOWED_NAME_NULL;
				result = false;
			}
			else if (ManagerId.IsEmpty())
			{
				message = Errors.Task.NOT_ALLOWED_MANAGER_NULL;
				result = false;
			}
			else if (StartDate > EndDate && EndDate > DateTime.MinValue)
			{
				message = Errors.Task.NOT_ALLOWED_DATE_INVALIDATE_RANGE;
				result = false;
			}

			else if (SecurityScenario.SpecialCharChecker.HasSpecialChar(TaskName))
			{
				message = Errors.Task.NOT_ALLOWED_SEPCIAL_CHAR;
				result = false;
			}

			return new Result { IsSuccess = result, Msg = message };
		}

		public bool IsEqualsWithViewModel(ArrangeTaskViewModel viewModel)
		{
			if (viewModel == null) return false;

			if (viewModel.start.ToDateTime() == StartDate
			 && viewModel.end.ToDateTime() == EndDate
			 && viewModel.taskType.ToGuid(Guid.Empty) == TaskType
			 && viewModel.stat.ToGuid(Guid.Empty) == TaskStatus
			 && viewModel.progress == RateOfProgress
			 && viewModel.parentId.ToGuid(Guid.Empty) == ParentId
			 && viewModel.workhours == WorkHours
			 && viewModel.reviewerId.ToGuid(Guid.Empty) == ReviewerID
			 && viewModel.subType.ToGuid(Guid.Empty) == SubTypeId
			 && viewModel.subTypeValue == SubTypeValue
			 )
			{
				return true;
			}

			return false;
		}

		public override bool Equals(object obj)
		{
			return IsEquals(obj as WorkTask);
		}

		protected virtual bool IsEquals(WorkTask task)
		{
			if (task == null) return false;

			if (task.TaskName == TaskName
			   && task.StartDate == StartDate
			   && task.EndDate == EndDate
			   && task.EstimateWorkHours == EstimateWorkHours
			   && task.TaskType == TaskType
			   && task.TaskStatus == TaskStatus
			   && task.RateOfProgress == RateOfProgress
			   && task.ParentId == ParentId
			   && task.WorkHours == WorkHours
			   && task.ManagerId == ManagerId
			   && task.ReviewerID == ReviewerID
			   && task.SubTypeId == SubTypeId
			   && task.SubTypeValue == SubTypeValue
			   )
			{
				return true;
			}


			return false;
		}

		public static List<int> StandardComplexityList => new List<int> { 1, 2, 3, 4, 5 };

		public static WorkTask Create(Guid defaultUserId, DateTime start, DateTime end, Guid status, Guid type)
		   => new WorkTask
		   {
			   TaskId = Guid.NewGuid(),
			   TaskStatus = status,
			   TaskType = type,
			   StartDate = start,
			   EndDate = end,
			   ManagerId = defaultUserId,
			   CreatorId = defaultUserId,
			   ReviewerID = defaultUserId,
		   };


		public static WorkTask Create(Guid defaultUserId, Guid projectId, string taskName, DateTime start, DateTime end, Guid status, Guid type,
		   int level, int sortId, bool isParent, Guid parentId)
		  => new WorkTask
		  {
			  TaskId = Guid.NewGuid(),
			  Projectid = projectId,
			  TaskStatus = status,
			  TaskName = taskName,
			  StartDate = start,
			  EndDate = end,
			  ManagerId = defaultUserId,
			  CreatorId = defaultUserId,
			  ReviewerID = defaultUserId,
			  EstimateWorkHours = 0,
			  IsParent = isParent,
			  ParentId = parentId,
			  SortId = sortId,
			  TaskType = type,
			  TaskLevel = level
		  };


		public static bool IsProjectTask(Guid typeId) => typeId != TaskKeys.TempTaskType;

		// 只有项目任务才能包含除了临时任务外其他类型任务
		public static bool HasSubTypeTask(Guid typeId) => typeId != TaskKeys.ProjectTaskType && typeId != TaskKeys.TempTaskType;

	}

}
