using Business;
using Business.Helper;
using Symber.Web.Data;
using System;
using System.Linq;
using System.Web.Mvc;
using TheSite.Models;

namespace TheSite.Controllers
{

   public class ProjectStoneTaskController : BaseController
   {

      [HttpPost]
      public ActionResult List(Guid projectId, int current, int rowCount, AjaxOrder sort, string searchPhrase)
      {
         var pst = APDBDef.ProjectStoneTask;

         var query = APQuery.select(pst.Asterisk)
                        .from(pst)
                        .where(pst.ProjectId == projectId);

         if (!string.IsNullOrEmpty(searchPhrase))
         {
            query.where_and(pst.TaskName.Match(searchPhrase));
         }

         query.primary(pst.PstId)
          .order_by(pst.CreateDate.Desc)
          .skip((current - 1) * rowCount)
          .take(rowCount);


         //获得查询的总数量

         var total = db.ExecuteSizeOfSelect(query);

         var rows = query.query(db, r =>
         {
            return new
            {
               id = pst.PstId.GetValue(r),
               name = pst.TaskName.GetValue(r),
               start = pst.StartDate.GetValue(r),
               end = pst.EndDate.GetValue(r),
               realStart = pst.RealStartDate.GetValue(r),
               realEnd = pst.RealEndDate.GetValue(r)
            };
         }).ToList();

         return Json(new
         {
            rows,
            current,
            rowCount,
            total
         });
      }


      // GET: ProjectStoneTask/Edit
      // POST-Ajax: ProjectStoneTask/Edit

      public ActionResult Edit(Guid id)
      {
         var stoneTask = db.ProjectStoneTaskDal.PrimaryGet(id);

         return PartialView(stoneTask);
      }

      [HttpPost]
      public ActionResult Edit(ProjectStoneTask task)
      {
         if (task.ProjectId.IsEmpty())
         {
            return Json(new
            {
               result = AjaxResults.Error,
               msg = Errors.StoneTask.EDIT_FAIL
            });
         }
         if (task.PstId.IsEmpty())
         {
            db.ProjectStoneTaskDal.Insert(task);
         }
         else
         {
            db.ProjectStoneTaskDal.Update(task);
         }

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.StoneTask.EDITSUCCESS
         });
      }


   }

}