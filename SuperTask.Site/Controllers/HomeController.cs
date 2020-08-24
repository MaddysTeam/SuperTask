using Business;
using Business.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TheSite.Models;

namespace TheSite.Controllers
{
	public class HomeController : BaseController
	{

		public ActionResult Index()
		{
			var user = GetUserInfo();
			if (user.IsBoss || user.IsManager)
				return RedirectToAction("ProjectManage");
			else
			{
				var userId = GetUserInfo().UserId;
				var myTasks = TaskHelper.GetUserTasks(userId, db);
				var myProjects = ProjectHelper.UserJoinedAvailableProject(userId, db);
				var myRequires = RequireHelper.GetRequiresByManager(userId, db);
				var reviewRequires = RequireHelper.GetRequiresByReviewer(userId, db);
				var myBugs = BugHelper.GetBugsByManagerId(userId, db);
				var users = UserHelper.GetAvailableUser(db);

				return View("EmployeeIndex", new EmployeeHomeViewModel
				{
					Tasks = myTasks,
					Porjects = myProjects,
					Requires = myRequires,
					ReviewRequires = reviewRequires,
					Bugs = myBugs,
					Users = users
				});
			}
		}

		public ActionResult ProjectManage(Guid? id)
		{
			var user = GetUserInfo();

			if (id == null)
			{
				var projects = user.IsBoss ? ProjectHelper.All(db) : ProjectHelper.UserJoinedProjects(GetUserInfo().UserId);
				id = projects[0].ProjectId;
			}
			return View("ProjectManageIndex", id);
		}

		[HttpPost]
		public ActionResult ProjectList()
		{
			var user = GetUserInfo();
			var projects = user.IsBoss ? ProjectHelper.All(db) : ProjectHelper.UserJoinedProjects(GetUserInfo().UserId);
			return PartialView("_projectLeftList", projects);
		}

		[HttpPost]
		public ActionResult ProjectInfo(Guid projectId)
		{
			var project = db.ProjectDal.PrimaryGet(projectId);
			project = project ?? new Project();
			project.Manager = db.UserInfoDal.PrimaryGet(project.ManagerId)?.UserName;
			project.Header = db.UserInfoDal.PrimaryGet(project.PMId)?.UserName;

			return PartialView("_projectInfo", project);
		}

		[HttpPost]
		public ActionResult ResourceList(Guid projectId)
		{
			var resources = ResourceHelper.GetCurrentProjectResources(projectId, db);

			return PartialView("_resourcesList", resources);
		}

		[HttpPost]
		public ActionResult ResourceDropdownList(Guid projectId)
		{
			var resources = ResourceHelper.GetCurrentProjectResources(projectId, db);
			var users = new List<UserInfo>();
			if (resources.Count > 0)
				users = resources.Select(r => new UserInfo { UserId = r.UserId, UserName = r.ResourceName }).ToList();
			return PartialView("_userDropdownList", users);
		}

		[HttpPost]
		public ActionResult WorkHours(Guid projectId)
		{
			var tasks = TaskHelper.GetProjectTasks(projectId, db);
			var users = UserHelper.GetAvailableUser(db);

			var workhours = tasks.GroupBy(x => x.DefaultExecutorId)
			   .Where(x => users.Exists(z => z.UserId == x.Key))
			   .Select(x => new { name = users.Find(z => z.UserId == x.Key)?.RealName, value = x.Sum(y => y.WorkHours).ToString() });
			List<string> names = new List<string>(), values = new List<string>();
			foreach (var item in workhours)
			{
				names.Add(item.name);
				values.Add(item.value);
			}

			return Json(new
			{
				names,
				values
			});
		}

		[HttpPost]
		public ActionResult StoneTaskGantte(Guid projectId)
		{
			var pst = Business.APDBDef.ProjectStoneTask;
			var tasks = db.ProjectStoneTaskDal.ConditionQuery(pst.ProjectId == projectId, null, null, null);
			List<GanttViewModel> result = new List<GanttViewModel>();
			foreach (var item in tasks)
			{
				DateTime start = item.RealStartDate == DateTime.MinValue ? item.StartDate : item.RealStartDate;
				string color = item.IsCompleteStatus ? "#e0e0e0" : "";
				result.Add(new GanttViewModel(1, item.TaskName, start, item.EndDate, color));
			}

			return Json(new
			{
				rows = result
			});
		}


		public ActionResult UserManage(Guid? id)
		{
			return View("UserManageIndex", id);
		}


		[HttpPost]
		public ActionResult UserList()
		{
			var users = UserHelper.GetAvailableUser(db);
			return PartialView("_userLeftList", users);
		}

		[HttpPost]
		public ActionResult UserInfo(Guid userId)
		{
			var user = db.UserInfoDal.PrimaryGet(userId);
			user.Tasks = TaskHelper.GetUserTasks(userId, db);
			user.Projects = ProjectHelper.UserJoinedProjects(userId, db);

			return PartialView("_userInfo", user);
		}

		[HttpPost]
		public ActionResult JoinedProjectList(Guid userId)
		{
			var projects = ProjectHelper.UserJoinedProjects(userId, db);

			return PartialView("_joinedProjectList", projects);
		}

		[HttpPost]
		public ActionResult JoinedProjectDropdownList(Guid userId)
		{
			var projects = ProjectHelper.UserJoinedProjects(userId, db);

			return PartialView("_projectDropdownList", projects);
		}

		[HttpPost]
		public ActionResult UserTasksGantte(Guid? userId, Guid? projectId, int current, int rowCount)
		{
			var tasks = projectId == null || projectId == Guid.Empty || projectId == ProjectKeys.SelectAll ?
			   TaskHelper.GetUserTasks(userId.Value, db).ToList() :
			   userId == null || userId == Guid.Empty || userId == AppKeys.SelectAll ?
			   TaskHelper.GetProjectTasks(projectId.Value) :
			   TaskHelper.GetProjectUserTasks(projectId.Value, userId.Value, db).ToList();

			var parents = tasks.FindAll(x => x.IsParent);
			if (parents.Count <= 0)
				parents.AddRange(tasks);

			var results = AddParentAndTheirChildren(tasks, parents);
			parents.AddRange(tasks.Except(results)); // 包含没有父节点的叶子任务

			var result = new List<GanttViewModel>();
			GanttViewModel ganttModel = null;
			int pageIndex = current * rowCount, max = pageIndex + rowCount;
			foreach (var item in parents)
			{
				if (pageIndex > max) break;

				DateTime start = item.RealStartDate == DateTime.MinValue ? item.StartDate : item.RealStartDate;
				string color = item.IsCloseStatus || item.IsCompleteStatus ? "#e0e0e0" : "";
				ganttModel = new GanttViewModel(item.SortId, item.TaskName, start, item.EndDate, color);
				result.Add(ganttModel);
				pageIndex++;
				foreach (var subItem in tasks.FindAll(x => x.ParentId == item.TaskId))
				{
					start = subItem.RealStartDate == DateTime.MinValue ? subItem.StartDate : subItem.RealStartDate;
					color = subItem.IsCloseStatus || subItem.IsCompleteStatus ? "#e0e0e0" : "";
					ganttModel.series.Add(new GanttItemViewModel(subItem.TaskName, start, subItem.EndDate, color));
					pageIndex++;
				}
			}

			return Json(new
			{
				rows = result
			});
		}


		[HttpPost]
		public ActionResult PersonalWorkHours(Guid userId)
		{
			var tasks = TaskHelper.GetUserTasks(userId, db);
			var projects = ProjectHelper.UserJoinedProjects(userId);

			var workhours = tasks.GroupBy(x => x.DefaultExecutorId)
			   .Select(x => new { name = tasks.Find(z => z.DefaultExecutorId == x.Key)?.ProjectName, value = x.Sum(y => y.WorkHours).ToString() });
			List<string> names = new List<string>(), values = new List<string>();
			foreach (var item in workhours)
			{
				names.Add(item.name);
				values.Add(item.value);
			}

			return Json(new
			{
				names,
				values
			});
		}

		private List<WorkTask> AddParentAndTheirChildren(List<WorkTask> source, List<WorkTask> parents)
		{
			if (parents == null || parents.Count <= 0)
				parents = source.FindAll(x => x.IsParent);
			List<WorkTask> results = new List<WorkTask>();
			foreach (var item in parents)
			{
				results.Add(item);
				results.AddRange(source.FindAll(x => x.ParentId == item.TaskId));
			}

			return results;
		}

	}

}