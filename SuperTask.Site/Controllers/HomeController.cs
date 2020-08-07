using Business;
using Business.Helper;
using System;
using System.Web.Mvc;
using System.Linq;
using Business.Config;
using TheSite.Models;

namespace TheSite.Controllers
{
	public class HomeController : BaseController
	{
		static APDBDef.WorkJournalTableDef wj = APDBDef.WorkJournal;

		public ActionResult Index()
		{
			return View("EmployeeIndex");
		}


		// POST-Ajax: Statistic/PersonalJournalReport

		[HttpPost]
		public ActionResult PersonalJournal()
		{
			var start = ThisApp.StartDayPerMonth;
			var end = ThisApp.EndDayPerMonth;
			var list = db.WorkJournalDal.ConditionQuery(wj.UserId == GetUserInfo().UserId
			   & wj.RecordType == JournalKeys.ManuRecordType //手工输入
			   & wj.Status != JournalKeys.ForbiddenStatus //非删除
			   & wj.WorkHours > 0 //填写过工时
			   & wj.RecordDate >= start
			   & wj.RecordDate <= end, null, null, null);

			return Json(new
			{
				recordHours = list.Sum(j => j.WorkHours),
				recordDayCount = list.GroupBy(j => j.RecordDate.TodayStart()).Count(),
				journalQuilty = WorkJournal.CheckQuailty(list),
			});
		}

		[HttpPost]
		public ActionResult TaskEndDateAlert()
		{
			var t = APDBDef.WorkTask;
			var currentUser = GetUserInfo();
			var needAlertDate = DateTime.Now.AddDays(10);

			var allTasks = WorkTask.ConditionQuery(t.ManagerId == currentUser.UserId & t.TaskStatus != TaskKeys.DeleteStatus, null);
			var projectTasks = allTasks.FindAll(x => x.TaskType == TaskKeys.ProjectTaskType);
			var tempTasks = allTasks.FindAll(x => x.TaskType == TaskKeys.TempTaskType);
			var maintainceTasks = allTasks.FindAll(x => x.TaskType == TaskKeys.MaintainedTaskType);

			var planStatusTasks = allTasks.FindAll(x => x.TaskStatus == TaskKeys.PlanStatus);
			var processStatusTasks = allTasks.FindAll(x => x.TaskStatus == TaskKeys.ProcessStatus);
			var completeStatusTasks = allTasks.FindAll(x => x.TaskStatus == TaskKeys.CompleteStatus);

			var alertTasks = allTasks.FindAll(x => x.ManagerId == currentUser.UserId
			   & x.EndDate <= needAlertDate
			   & x.EndDate >= DateTime.Now);

			return Json(new
			{
				//data = alertTasks,
				allcount = allTasks.Count,
				projectTaskCount = projectTasks.Count,
				tempTaskCount = tempTasks.Count,
				maintainceTaskCount = maintainceTasks.Count,

				planStatusTaskCount = planStatusTasks.Count,
				processStatusTaskCount = processStatusTasks.Count,
				completeStatusTaskCount = completeStatusTasks.Count,

				alterTaskCount = alertTasks.Count
			});
		}


		//public ActionResult MyTasks()
		//{
		//	return null;
		//}

		//public ActionResult MyBugs()
		//{
		//	return null;
		//}

		//public ActionResult MyRequires()
		//{
		//	return null;
		//}

	}

}