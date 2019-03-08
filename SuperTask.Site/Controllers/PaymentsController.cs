using Business;
using Business.Helper;
using System;
using System.Web.Mvc;
using TheSite.Models;
using System.Linq;
using Symber.Web.Data;

namespace TheSite.Controllers
{

   public class PaymentsController : BaseController
   {

      // Post-Ajax: Payment/Edit

      [HttpPost]
      public ActionResult Edit(Payments payments)
      {
         var pst = APDBDef.ProjectStoneTask;
         var t = APDBDef.WorkTask;
         var project = db.ProjectDal.PrimaryGet(payments.ProjectId);

         var validateResult = payments.Valiedate();
         if (!validateResult.IsSuccess)
            return Json(new
            {
               result = AjaxResults.Error,
               msg = validateResult.Msg
            });

         payments.Money = payments.PayType == PaymentsKeys.NothingType ? 0 : payments.Money;

         if (payments.PayId.IsEmpty())
         {
            payments.PayId = Guid.NewGuid();
            db.PaymentsDal.Insert(payments);
         }
         else
         {
            var cmoney = project.CMoney;
            var ratio = cmoney <= 0 ? 0 : (double)(payments.Money / cmoney).Round(2);
            payments.Ratio = ratio.ToString("P");

            db.PaymentsDal.Update(payments);
         }


         // 如果是nothingType 则 删除节点任务
         if (payments.PayType == PaymentsKeys.NothingType)
         {
            db.ProjectStoneTaskDal.ConditionDelete(pst.PmsId == payments.PayId & pst.ProjectId == payments.ProjectId);
            return Json(new
            {
               result = AjaxResults.Success,
               msg = Success.Payments.EDITSUCCESS
            });
         }

         var startDate = payments.InvoiceDate.IsEmpty() ? project.StartDate : payments.InvoiceDate;
         var endDate = payments.PayDate.IsEmpty() ? project.EndDate : payments.PayDate;

         //新增项目节点任务      
         var stonetaskExists = db.ProjectStoneTaskDal.ConditionQueryCount(pst.PmsId == payments.PayId & pst.ProjectId == payments.ProjectId) > 0;
         if (!stonetaskExists)
         {
            db.ProjectStoneTaskDal.Insert(new ProjectStoneTask(
              Guid.NewGuid(),
              payments.PayId,
              project.ProjectId,
              payments.PayName,
              startDate,
              endDate,
              DateTime.MinValue,
              DateTime.MinValue,
              TaskKeys.PlanStatus,
              DateTime.Now,
              TaskKeys.NodeTaskType,
              project.ManagerId,
              project.ReviewerId,
              payments.Sort,
              DateTime.MinValue
              ));
         }
         else
         {
            APQuery
              .update(pst)
              .set(pst.EndDate.SetValue(endDate), pst.StartDate.SetValue(startDate), pst.TaskName.SetValue(payments.PayName))
              .where(pst.PmsId == payments.PayId).execute(db);
         }

         // 只有在【最终确认】后而且填写过【预估金额】的款项才会添加实际项目任务
         if (payments.IsConfirm && payments.Money > 0)
         {
            var taskIsExists = db.WorkTaskDal.ConditionQueryCount(t.Projectid == project.ProjectId & t.TaskName == payments.PayName) > 0;
            if (!taskIsExists)
            {
               var tasks = db.WorkTaskDal.ConditionQuery(t.Projectid == project.ProjectId, null, null, null);
               var root = tasks.Find(x => x.ParentId == Guid.Empty);
               db.WorkTaskDal.Insert(new WorkTask
               {
                  TaskId = Guid.NewGuid(),
                  Projectid = project.ProjectId,
                  CreateDate = DateTime.Now,
                  CreatorId = project.ManagerId,
                  StartDate = startDate,
                  EndDate = endDate,
                  ManagerId = project.ManagerId,
                  ReviewerID = project.ReviewerId,
                  TaskStatus = project.IsPlanStatus ? TaskKeys.PlanStatus : TaskKeys.ProcessStatus,
                  IsParent = false,
                  ParentId = root.TaskId,
                  TaskName = payments.PayName,
                  TaskType = TaskKeys.ProjectTaskType,
                  TaskLevel = 2,
                  SortId = tasks.Count + 1
               });
            }
         }

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Payments.EDITSUCCESS,
            payments
         });
      }


      // Post-Ajax: Payment/Remove

      [HttpPost]
      public ActionResult Remove(Guid id)
      {
         var pst = APDBDef.ProjectStoneTask;
         var p = APDBDef.Payments;

         var payments = db.PaymentsDal.PrimaryGet(id);
         var children = db.PaymentsDal.ConditionQuery(p.ParentId == id, null, null, null);
         var subQuery = APQuery.select(p.PayId).from(p).where(p.ParentId == id | p.PayId == id);

         db.BeginTrans();

         try
         {
            db.ProjectStoneTaskDal.ConditionDelete(pst.PmsId.In(subQuery) & pst.ProjectId == payments.ProjectId);
            db.PaymentsDal.ConditionDelete(p.PayId.In(subQuery));

            db.Commit();
         }
         catch
         {
            db.Rollback();
         }

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Payments.EDITSUCCESS
         });
      }


      // Post-ajax: Payment/CreateTempVenderPayment

      [HttpPost]
      public ActionResult CreateTempVenderPayment(Guid? id, Guid projectId)
      {
         var p = APDBDef.Payments;
         var payments = db.PaymentsDal.ConditionQuery(p.ProjectId == projectId & p.PayType == PaymentsKeys.InternalVenderPaymentsType, null, null, null);

         Payments payment = null;
         if (id != null)
         {
            var childCount = payments.Count(x => x.ParentId == id);
            payment = db.PaymentsDal.PrimaryGet(id.Value);
            var payId = payment.PayId;
            payment.PayId = Guid.NewGuid();
            payment.ParentId = payId;
            payment.Sort = childCount;
            payment.PayName += $"({++childCount})";
            payment.Money = 0;
         }
         else
         {
            var parentCount = payments.Count(x => x.ParentId == Guid.Empty);
            var project = db.ProjectDal.PrimaryGet(projectId);
            payment = new Payments { PayId = Guid.NewGuid(), PayName = PaymentsKeys.DefaultVenderName, ParentId = Guid.Empty, ProjectId = projectId, PayType = PaymentsKeys.InternalVenderPaymentsType, PayDate = project.EndDate, Sort = parentCount };
         }

         db.PaymentsDal.Insert(payment);

         ViewData["project"] = ProjectrHelper.GetCurrentProject(projectId);

         return PartialView("_venderPayments", payment);
      }


      // Post-ajax: Payment/Details

      [HttpPost]
      public ActionResult Details(Guid projectId, string tabId)
      {
         ViewData["project"] = ProjectrHelper.GetCurrentProject(projectId);
         ViewBag.TabId = string.IsNullOrEmpty(tabId) ? "moneyTab" : tabId;

         return PartialView("Details", PaymentsHelper.GetProjectPayments(projectId, db));
      }

      // Post-ajax: Payment/Clare

      [HttpPost]
      public ActionResult Clare(Guid id, Guid projectId)
      {
         var p = APDBDef.Payments;
         var pst = APDBDef.ProjectStoneTask;

         APQuery.update(p).set(
            p.Money.SetValue(0)
            )
            .where(p.PayId == id)
            .execute(db);

         db.ProjectStoneTaskDal.ConditionDelete(pst.PmsId == id & pst.ProjectId == projectId);

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Payments.EDITSUCCESS,
         });
      }

   }

}