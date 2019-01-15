using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business;
using Symber.Web.Data;
using Business.Config;
using Business.Helper;
using TheSite.Models;
using Business.Roadflow;

namespace TheSite.Controllers
{

   public class ProjectMileStoneController : BaseController
   {

      static APDBDef.ProjectMileStoneTableDef pm = APDBDef.ProjectMileStone;

      // Post-Ajax: ProjectManage/AddMileStones
      // Post-Ajax: ProjectManage/RemoveMileStone
      // Post-Ajax: ProjectManage/EditStone

      [HttpPost]
      public ActionResult AddStones(Guid projectId, string mileStoneIds)
      {
         var userId = GetUserInfo().UserId;
         var project = db.ProjectDal.PrimaryGet(projectId);
         var milestones = MileStone.GetAll();
         var ids = mileStoneIds.Split(',');

         foreach (var item in ids)
         {
            var stone = milestones.FirstOrDefault(ms => ms.StoneId == item.ToGuid(Guid.Empty));
            if (stone != null)
            {
               MilestoneHelper.AddProjectMileStone(project, stone, db);
            }
         }

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Project.ADD_MILESTONE_SUCCESS
         });
      }

      [HttpPost]
      public ActionResult Remove(Guid projectId, Guid mileStoneId)
      {
         var isExists = ProjectMileStone.ConditionQueryCount(pm.Projectid == projectId & pm.StoneId == mileStoneId) > 0;
         if (isExists)
         {
            ProjectMileStone.ConditionDelete(pm.Projectid == projectId & pm.StoneId == mileStoneId);
         }

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Project.Edit_MILESTONE_SUCCESS
         });
      }

      [HttpPost]
      public ActionResult Edit(ProjectMileStone projectMileStone)
      {
         if (projectMileStone.PmsId.IsEmpty())
         {
            projectMileStone.PmsId = Guid.NewGuid();
            db.ProjectMileStoneDal.Insert(projectMileStone);
         }
         else
         {
            db.ProjectMileStoneDal.Update(projectMileStone);
         }

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Project.Edit_MILESTONE_SUCCESS
         });
      }

      [HttpPost]
      public ActionResult List(Guid projectId)
      {
         return PartialView(
            MilestoneHelper.GetProjectMileStones(projectId, db)
            );
      }

   }

}