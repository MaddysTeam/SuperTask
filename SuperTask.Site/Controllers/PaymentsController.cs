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

         #region [TODO]:
         //MilestoneHelper.AddProjectMileStoneIfNotExits(
         //   project,
         //   new MileStone { StoneId = payments.PayId, StoneName = payments.PayName }, //这里的milestone 不会存入数据库，将payment 作为一个隐形的 mileStone
         //   db);
         #endregion
         var pst = APDBDef.ProjectStoneTask;
         var taskExists = db.ProjectStoneTaskDal.ConditionQueryCount(pst.PmsId == payments.PayId & pst.ProjectId == payments.ProjectId) > 0;

         if (!taskExists)
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
              project.ReviewerId
              ));
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
         var pms = APDBDef.ProjectMileStone;
         var pst = APDBDef.ProjectStoneTask;
         var p = APDBDef.Payments;

         var payments = db.PaymentsDal.PrimaryGet(id);
         var children = db.PaymentsDal.ConditionQuery(p.ParentId==id,null,null,null);
         var subQuery = APQuery.select(p.PayId).from(p).where(p.ParentId==id | p.PayId==id);

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
         Payments payment = null;
         if (id != null)
         {
            payment = db.PaymentsDal.PrimaryGet(id.Value);
            var payId = payment.PayId ;
            payment.PayId = Guid.Empty;
            payment.ParentId = payId;
         }
         else {
            var project = db.ProjectDal.PrimaryGet(projectId);
            payment = new Payments { PayId = Guid.Empty, PayName =string.Empty, ParentId=Guid.Empty, ProjectId = projectId, PayType = PaymentsKeys.InternalVenderPaymentsType, PayDate = project.EndDate };
         }


         return PartialView("_venderPayments", payment);
      }


      // Post-ajax: Payment/Details

      [HttpPost]
      public ActionResult Details(Guid projectId, string tabId)
      {
         ViewBag.Project = db.ProjectDal.PrimaryGet(projectId);
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