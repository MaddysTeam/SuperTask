using System;
using System.Collections.Generic;
using System.Linq;

namespace Business.Helper
{

	public static class WorkJournalHelper
	{
		static APDBDef.WorkJournalTableDef wj = APDBDef.WorkJournal;

		/// <summary>
		/// 通过任务创建或修改日志
		/// </summary>
		/// <param name="tk">任务实体</param>
		/// <param name="db">APDBDef</param>
		public static void CreateOrUpdateJournalByTask(WorkTask tk, APDBDef db)
		{
			if (tk == null || db == null) return;

			var jounal = db.WorkJournalDal
			   .ConditionQuery(wj.TaskId == tk.TaskId
							 & wj.UserId == tk.DefaultExecutorId
							 & wj.RecordDate > DateTime.Now.TodayStart()
							 & wj.RecordDate < DateTime.Now.TodayEnd(), null, null, null)
			   .FirstOrDefault();

			if (jounal == null)
				CreateByTask(tk, db);
			else
				db.WorkJournalDal.UpdatePartial(jounal.JournalId,
				   new
				   {
					   UserId = tk.DefaultExecutorId,
					   Projectid = tk.Projectid,
					   TaskId = tk.TaskId,
					   Status = JournalKeys.SaveStatus,
					   RecordType = JournalKeys.ManuRecordType,
					   TaskStatus = tk.TaskStatus,
					   TaskType = tk.TaskType,
					   //TaskSubType = tk.SubType,
					   //Progress = tk.RateOfProgress,
				   });
		}


		/// <summary>
		/// 通过任务集合创建日志
		/// </summary>
		/// <param name="tasks">任务集合</param>
		/// <param name="db">APDBDef</param>
		public static void CreateOrUpdateJournalByTasks(List<WorkTask> tasks, APDBDef db)
		{
			if (tasks == null || tasks.Count <= 0 || db == null)
				return;

			foreach (var tk in tasks)
			{
				CreateOrUpdateJournalByTask(tk, db);
			}
		}


		public static WorkJournal CreateByTask(WorkTask tk, APDBDef db)
		{
			var journal = new WorkJournal
			{
				JournalId = Guid.NewGuid(),
				UserId = tk.DefaultExecutorId,
				Projectid = tk.Projectid,
				TaskId = tk.TaskId,
				TaskEstimateWorkHours = tk.EstimateWorkHours,
				WorkHours = 0,
				Progress = tk.RateOfProgress,//default value is 0
				Comment = string.Empty,
				CreateDate = DateTime.Now,
				RecordDate = DateTime.Now,
				Status = JournalKeys.SaveStatus,
				RecordType = GetRecordType(tk),
				TaskStatus = tk.TaskStatus,
				TaskType = tk.TaskType,
				TaskSubType = tk.SubTypeId
			};

			db.WorkJournalDal.Insert(journal);

			return journal;
		}

		public static Guid GetRecordType(WorkTask tk)
		=> tk.IsParent ? JournalKeys.AutoRecordType : JournalKeys.ManuRecordType;

		public static WorkJournal GetJournal(Guid taskId, Guid userId, APDBDef db)
			=> db
		   .WorkJournalDal
		   .ConditionQuery(wj.TaskId == taskId & wj.UserId == userId & wj.RecordDate.Between(DateTime.Now.TodayStart(), DateTime.Now.TodayEnd()), null, null, null)
		   .FirstOrDefault();


	}

}