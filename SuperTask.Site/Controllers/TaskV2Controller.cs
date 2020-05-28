using Business;
using Business.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TheSite.Controllers
{

   public class TaskV2Controller : BaseController
   {
		
		public ActionResult List()
		{
			return View();
		}


		[HttpPost]
		public ActionResult List(int current, int rowCount, AjaxOrder sort, string searchPhrase)
		{
			return Json(new { });
		}


		public ActionResult Edit()
		{
			return View();
		}


		[HttpPost]
		public ActionResult Delete(Guid id)
		{
			return Json(new { });
		}


		[HttpPost]
		public ActionResult EditChild()
		{
			return Json(new { });
		}


		[HttpPost]
		public ActionResult Start()
		{
			return Json(new { });
		}

		[HttpPost]
		public ActionResult Complete()
		{
			return Json(new { });
		}


		[HttpPost]
		public ActionResult Close()
		{
			return Json(new { });
		}
		
	
   }

}