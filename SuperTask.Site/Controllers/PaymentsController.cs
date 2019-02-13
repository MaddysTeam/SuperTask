﻿using Business;
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
         var t = APDBDef.WorkTask;

         var validateResult = payments.Valiedate();
         if (!validateResult.IsSuccess)
            return Json(new
            {
               result = AjaxResults.Error,
               msg = validateResult.Msg
            });

         var project = db.ProjectDal.PrimaryGet(payments.ProjectId);
         if (payments.PayId.IsEmpty())
         {
            payments.PayId = Guid.NewGuid();
            db.PaymentsDal.Insert(payments);
         }
         else
         {
            db.PaymentsDal.Update(payments);
         }

         //新增项目节点任务
         var pst = APDBDef.ProjectStoneTask;
         var stonetaskExists = db.ProjectStoneTaskDal.ConditionQueryCount(pst.PmsId == payments.PayId & pst.ProjectId == payments.ProjectId) > 0;
         if (!stonetaskExists)
         {
            db.ProjectStoneTaskDal.Insert(new ProjectStoneTask(
              Guid.NewGuid(),
              payments.PayId,
              project.ProjectId,
              payments.PayName,
              project.StartDate,
              project.EndDate,
              DateTime.MinValue,
              DateTime.MinValue,
              TaskKeys.PlanStatus,
              DateTime.Now,
              TaskKeys.NodeTaskType,
              project.ManagerId,
              project.ReviewerId,
              DateTime.MinValue
              ));
         }
         else
         {
            APQuery
              .update(pst)
              .set(pst.EndDate.SetValue(payments.InvoiceDate), pst.StartDate.SetValue(payments.InvoiceDate), pst.TaskName.SetValue(payments.PayName))
              .where(pst.PmsId == payments.PayId).execute(db);
         }

         //新增项目任务
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
               StartDate = project.StartDate,
               EndDate = project.EndDate,
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

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Payments.EDITSUCCESS
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
         ViewBag.TabId = tabId;

         return PartialView("Details", PaymentsHelper.GetProjectPayments(projectId, db));
      }

      // Post-ajax: Payment/Clare

      [HttpPost]
      public ActionResult Clare(Guid id, Guid projectId)
      {
         var p = APDBDef.Payments;
         var pst = APDBDef.ProjectStoneTask;

         APQuery.update(p).set(
            p.Money.SetValue(0),
            p.PayDate.SetValue(DateTime.MinValue),
            p.InvoiceDate.SetValue(DateTime.MinValue)
            )
            .where(p.PayId == id)
            .execute(db);

         db.ProjectStoneTaskDal.ConditionDelete(pst.PmsId == id & pst.ProjectId == projectId);

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Payments.EDITSUCCESS
         });
      }

   }

}