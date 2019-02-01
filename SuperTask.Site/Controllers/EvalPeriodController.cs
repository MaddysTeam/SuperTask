//using Business;
//using Symber.Web.Data;
//using System;
//using System.Web.Mvc;
//using TheSite.EvalAnalysis;
//using TheSite.Models;
//using System.Linq;
//using Business.Helper;

//namespace TheSite.Controllers
//{

//   public class EvalPeriodController : BaseController
//   {

//      static APDBDef.EvalPeriodTableDef ep = APDBDef.EvalPeriod;


//      // GET: EvalPeriod/List

//      public ActionResult List()
//      {
//         var list = db.EvalPeriodDal.ConditionQuery(null, null, null, null);

//         return View(list);
//      }


//      //	GET: EvalPeriod/Edit
//      //	POST-Ajax: EvalPeriod/Edit

//      public ActionResult Edit(Guid? id)
//      {
//         var model = new EvalPeriod()
//         {
//            BeginDate = DateTime.Today,
//            EndDate = DateTime.Today.AddYears(3),
//            AccessBeginDate = DateTime.Today,
//            AccessEndDate = DateTime.Today.AddYears(1)
//         };

//         if (id != null)
//         {
//            model = db.EvalPeriodDal.PrimaryGet(id.Value);
//         }

//         return PartialView("Edit", model);
//      }

//      [HttpPost]
//      public ActionResult Edit(EvalPeriod model)
//      {
//         ThrowNotAjax();


//         model.AnalysisName = EvalManager.Engines[model.AnalysisType].AnalysisName;

//         if (!model.PeriodId.IsEmpty())
//         {
//            db.EvalPeriodDal.UpdatePartial(model.PeriodId, new
//            {
//               model.Name,
//               model.BeginDate,
//               model.EndDate,
//               model.AccessBeginDate,
//               model.AccessEndDate,
//               model.AnalysisName,
//               model.AnalysisType
//            });
//         }
//         else
//         {
//            model.PeriodId = Guid.NewGuid();
//            db.EvalPeriodDal.Insert(model);
//         }


//         return Json(new
//         {
//            result = AjaxResults.Success,
//            msg = "信息已保存!"
//         });
//      }


//      //	POST-Ajax: EvalPeriod/Remove		

//      [HttpPost]
//      public ActionResult Remove(Guid id)
//      {
//         ThrowNotAjax();

//         db.EvalPeriodDal.PrimaryDelete(id);

//         return Json(new
//         {
//            result = AjaxResults.Success,
//            msg = "信息已删除"
//         });
//      }


//      //	GET: EvalPeriod/BindTables
//      //	POST-Ajax: EvalPeriod/BindTables

//      public ActionResult BindTables(Guid? id)
//      {
//         //var ept = APDBDef.EvalPeriodTable;
//         //var evt = APDBDef.EvalTable;

//         //var result = APQuery.select(evt.TableId, evt.TableName, ept.TableId)
//         //   .from(evt, ept.JoinLeft(ept.TableId == evt.TableId & ept.PeriodId == id))
//         //   .where(evt.TableStatus == EvalTableKeys.DoneStatus)
//         //   .query(db, r =>
//         //   {
//         //      return new EvalTable
//         //      {
//         //         TableId = evt.TableId.GetValue(r),
//         //         TableName = evt.TableName.GetValue(r),
//         //         IsSelected = !ept.TableId.GetValue(r).IsEmpty()
//         //      };
//         //   }).ToList();

//         //ViewBag.PeriodId = id;

//         return PartialView(null);
//      }

//      [HttpPost]
//      public ActionResult BindTables(Guid id, string tableIds)
//      {
//         //var ept = APDBDef.EvalPeriodTable;

//         //db.BeginTrans();

//         //try
//         //{
//         //   db.EvalPeriodTableDal.ConditionDelete(ept.PeriodId == id);

//         //   if (!string.IsNullOrEmpty(tableIds) && !string.Equals("null", tableIds))
//         //   {
//         //      tableIds.Split(',').ToList().ForEach(tableId =>
//         //      {
//         //         db.EvalPeriodTableDal.Insert(new EvalPeriodTable
//         //         {
//         //            PeriodId = id,
//         //            PeriodTableId = Guid.NewGuid(),
//         //            TableId = tableId.ToGuid(Guid.Empty)
//         //         });
//         //      });
//         //   }

//         //   db.Commit();
//         //}
//         //catch
//         //{
//         //   db.Rollback();
//         //}

//         //EvalManager.RefreshEngines();

//         return Json(new
//         {
//            result = AjaxResults.Success,
//            msg = "操作成功！"
//         });
//      }

//   }

//}