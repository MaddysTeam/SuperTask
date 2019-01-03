using Business;
using Business.Helper;
using System;
using System.Web.Mvc;
using System.Linq;
using Business.Config;
using TheSite.Models;

namespace TheSite.Controllers
{
   public class PaymentsController : BaseController
   {
      static APDBDef.PaymentsTableDef p = APDBDef.Payments;

      [HttpPost]
      public ActionResult Edit(Payments payments)
      {
         if (payments.PayId.IsEmpty())
         {
            payments.PayId = Guid.NewGuid();
            db.PaymentsDal.Insert(payments);
         }
         else
         {
            db.PaymentsDal.Update(payments);
         }

         return Json(new { 
            result = AjaxResults.Success,
            msg = Success.Payments.EDITSUCCESS
         });
      }


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

   }

}