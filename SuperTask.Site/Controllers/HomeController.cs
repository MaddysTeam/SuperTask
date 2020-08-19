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

		public ActionResult Index()
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

		public ActionResult ManagerIndex()
		{
			var userId = GetUserInfo().UserId;
			var myProjects =   ProjectHelper.All(db);
			return View("ManagerIndex", new ManagerHomeViewModel() { Porjects=myProjects });

		}


	}

}