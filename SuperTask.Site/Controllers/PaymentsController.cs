using Business;
using Business.Config;
using Business.Helper;
using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TheSite.Models;

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

            StoneTaskHelper.CreateStoneTask(payments.ProjectId, payments.PayName, payments.InvoiceDate, payments.PayDate, db);
         }
         else
         {
            db.PaymentsDal.Update(payments);
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
         db.PaymentsDal.PrimaryDelete(id);

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Payments.EDITSUCCESS
         });
      }


      // Post-ajax: Payment/CreateTempVenderPayment

      [HttpPost]
      public ActionResult CreateTempVenderPayment(Guid id)
      {
         var project = db.ProjectDal.PrimaryGet(id);
         var payment = new Payments { ProjectId = id, PayType = PaymentsKeys.InternalVenderPaymentsType, PayDate = project.EndDate };
         return PartialView("_venderPayments", payment);
      }


      // Post-ajax: Payment/Details

      [HttpPost]
      public ActionResult Details(Guid projectId)
      {
         return PartialView("Details", PaymentsHelper.GetProjectPayments(projectId, db));
      }


   }

}