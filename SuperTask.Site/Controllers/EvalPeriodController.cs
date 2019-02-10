using Business;
using Symber.Web.Data;
using System;
using System.Web.Mvc;
using TheSite.EvalAnalysis;
using TheSite.Models;
using System.Linq;
using Business.Helper;

namespace TheSite.Controllers
{

   public class EvalPeriodController : BaseController
   {

      static APDBDef.EvalPeriodTableDef ep = APDBDef.EvalPeriod;


      // GET: EvalPeriod/List

      public ActionResult List()
      {
         var list = db.EvalPeriodDal.ConditionQuery(null, null, null, null);

         return View(list);
      }


      //	GET: EvalPeriod/Edit
      //	POST-Ajax: EvalPeriod/Edit

      public ActionResult Edit(Guid? id)
      {
         var model = new EvalPeriod()
         {
            BeginDate = DateTime.Today,
            EndDate = DateTime.Today.AddYears(3),
            AccessBeginDate = DateTime.Today,
            AccessEndDate = DateTime.Today.AddYears(1)
         };

         if (id != null)
         {
            model = db.EvalPeriodDal.PrimaryGet(id.Value);
         }

         return PartialView("Edit", model);
      }

      [HttpPost]
      public ActionResult Edit(EvalPeriod model)
      {
         ThrowNotAjax();

         if (!model.PeriodId.IsEmpty())
         {
            db.EvalPeriodDal.UpdatePartial(model.PeriodId, new
            {
               model.Name,
               model.BeginDate,
               model.EndDate,
               model.AccessBeginDate,
               model.AccessEndDate,
               model.AnalysisName,
               model.AnalysisType
            });
         }
         else
         {
            var eat = APDBDef.EvalAccessorTarget;
            var ettp = APDBDef.EvalTargetTablePropertion;

            model.PeriodId = Guid.NewGuid();
            model.CreateDate = DateTime.Now;
           
            var periods = EvalPeriod.GetAll();
            if (periods.Count > 0)
            {
               var lastPeriod = periods.OrderByDescending(x => x.CreateDate).First();
               var periodAccessorTargets = db.EvalAccessorTargetDal.ConditionQuery(eat.PeriodId==lastPeriod.PeriodId,null,null,null);
               var evalTablePropertions = db.EvalTargetTablePropertionDal.ConditionQuery(ettp.PeriodId == lastPeriod.PeriodId, null, null, null);
               foreach(var item in periodAccessorTargets)
               {
                  item.AccessorTargetId = Guid.NewGuid();
                  item.PeriodId = model.PeriodId;
                  db.EvalAccessorTargetDal.Insert(item);
               }
               foreach (var item in evalTablePropertions)
               {
                  item.PropertionID = Guid.NewGuid();
                  item.PeriodId = model.PeriodId;
                  db.EvalTargetTablePropertionDal.Insert(item);
               }
            }

            db.EvalPeriodDal.Insert(model);
         }

         return Json(new
         {
            result = AjaxResults.Success,
            msg = "信息已保存!"
         });
      }


      //	POST-Ajax: EvalPeriod/Remove		

      [HttpPost]
      public ActionResult Remove(Guid id)
      {
         ThrowNotAjax();

         db.EvalPeriodDal.PrimaryDelete(id);

         return Json(new
         {
            result = AjaxResults.Success,
            msg = "信息已删除"
         });
      }

   }

}