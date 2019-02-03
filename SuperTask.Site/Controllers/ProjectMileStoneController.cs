﻿using System;
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
               MilestoneHelper.AddProjectMileStoneIfNotExits(project, stone, db);
            }
         }

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Project.ADD_MILESTONE_SUCCESS
         });
      }


      // Post-Ajax: ProjectManage/RemoveMileStone

      [HttpPost]
      public ActionResult Remove(Guid id)
      {
         var pst = APDBDef.ProjectStoneTask;
         var pms = db.ProjectMileStoneDal.PrimaryGet(id);
         if (pms!=null)
         {
            db.ProjectMileStoneDal.PrimaryDelete(id);
            db.ProjectStoneTaskDal.ConditionDelete(pst.PmsId == id);
         }

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Project.Edit_MILESTONE_SUCCESS
         });
      }


      // Post-Ajax: ProjectManage/EditStone

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
            var pst = APDBDef.ProjectStoneTask;
            db.ProjectMileStoneDal.Update(projectMileStone);
            APQuery.update(pst).set(pst.EndDate.SetValue(projectMileStone.EndDate), pst.StartDate.SetValue(projectMileStone.StartDate)).where(pst.PmsId== projectMileStone.PmsId).execute(db); 
         }

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Project.Edit_MILESTONE_SUCCESS
         });
      }


      // Post-Ajax: ProjectManage/List

      [HttpPost]
      public ActionResult List(Guid projectId)
      {
         ViewData["Project"] = ProjectrHelper.GetCurrentProject(projectId, db);

         return PartialView(
            MilestoneHelper.GetProjectMileStones(projectId, db)
            );
      }


      // GET: MileStone/ChooseList

      public ActionResult ChooseList(Guid projectId)
      {
         var m = APDBDef.MileStone;

         var subQuery = APQuery.select(pm.StoneId).from(pm).where(pm.Projectid == projectId);

         var result = APQuery.select(m.Asterisk)
                          .from(m)
                          .where(m.StoneId.NotIn(subQuery))
                          //.order_by(pm.CreateDate.Desc)
                          .query(db,r=> 
                          {
                             var mileStone = new MileStone();
                             m.Fullup(r, mileStone,false);
                             return mileStone;
                          }).ToList();

         return PartialView(result);
      }

   }

}