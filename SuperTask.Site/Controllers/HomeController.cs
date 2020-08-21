using Business.Helper;
using System.Web.Mvc;
using TheSite.Models;

namespace TheSite.Controllers
{
	public class HomeController : BaseController
	{

		public ActionResult Index()
		{
			var user = GetUserInfo();
			if (user.IsBoss)
			  return RedirectToAction("UserManage");
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

		public ActionResult ProjectManage()
		{
			var user = GetUserInfo();
			if (!user.IsBoss)
				RedirectToAction("index");

			var userId = GetUserInfo().UserId;
			var projects = ProjectHelper.All(db);
			var users = UserHelper.GetAvailableUser(db);

			return View("ProjectManageIndex", new ManagerHomeViewModel() { Users = users, Projects = projects });
		}

		public ActionResult UserManage()
		{
			var user = GetUserInfo();
			if (!user.IsBoss)
				RedirectToAction("index");

			var userId = GetUserInfo().UserId;
			var projects = ProjectHelper.All(db);
			var users = UserHelper.GetAvailableUser(db);

			return View("UserManageIndex", new ManagerHomeViewModel() { Users = users, Projects = projects });
		}

	}

}