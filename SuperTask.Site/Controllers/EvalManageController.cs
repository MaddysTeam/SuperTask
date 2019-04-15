using Business;
using Business.Helper;
using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TheSite.EvalAnalysis;
using TheSite.Models;

namespace TheSite.Controllers
{

   public class EvalManageController : BaseController
   {

      static APDBDef.ResourceTableDef r = APDBDef.Resource;
      static APDBDef.ProjectTableDef p = APDBDef.Project;
      static APDBDef.UserInfoTableDef u = APDBDef.UserInfo;
      static APDBDef.EvalResultTableDef er = APDBDef.EvalResult;
      static APDBDef.RoleTableDef ro = APDBDef.Role;
      static APDBDef.EvalAccessorTargetTableDef ett = APDBDef.EvalAccessorTarget;
      static APDBDef.EvalTableTableDef et = APDBDef.EvalTable;

      //	GET: EvalManage/Index

      public ActionResult Index()
      {
         var currentPeriods = EvalPeriod.GetCurrentPeriod(db);

         if (currentPeriods.Count <= 0)
         {
            throw new ApplicationException("当前不再考核期");
         }

         return RedirectToAction("EvalMemberList");
      }


      //	GET: EvalManage/EvalMemberList
      //	POST-Ajax: EvalManage/EvalMemberList

      public ActionResult EvalMemberList()
      {
         return View();
      }

      [HttpPost]
      public ActionResult EvalMemberList(Guid evalType,int current, int rowCount, AjaxOrder sort, string searchPhrase)
      {
         var period = EvalPeriod.GetCurrentPeriod(db).FirstOrDefault();
         var accessor = GetUserInfo();
         var results = APQuery.select(ett.TargetId.As("TargetId"), u.UserName, er.ResultId.Count().As("EvalCount"),er.Score)
                       .from(ett,
                             et.JoinInner(et.TableId==ett.TableId),
                             u.JoinInner(ett.TargetId == u.UserId),
                             er.JoinLeft(ett.TargetId == er.TargetId
                                         & er.AccesserId == accessor.UserId
                                         & er.EvalType == evalType
                                         & er.PeriodId == period.PeriodId
                                         )
                             )
                        .where(ett.AccessorId == accessor.UserId & et.TableType== evalType)
                        .group_by(ett.TargetId, u.UserName,er.Score)
                        .query(db, r =>
                        {
                           return new EvalMember
                           {
                              MemberId = ett.TargetId.GetValue(r, "TargetId"),
                              MemberName = u.UserName.GetValue(r),
                              AccessorId = accessor.UserId,
                              AccessorName = accessor.UserName,
                              EvalCount = (int)(r["EvalCount"]),
                              PeriodNames = period.Name,
                              Score=er.Score.GetValue(r) 
                           };
                        }).ToList();

         var total = results.Count();

         if (total > 0)
            results = results
               .Skip(rowCount * current - rowCount)
               .Take(rowCount)
               .ToList();


         return Json(new
         {
            rows = results,
            current,
            rowCount,
            total
         });
      }


      //      //	GET: EvalManage/PeriodEvalMemberList
      //      //	POST-Ajax: EvalManage/PeriodEvalMemberList

      //      public ActionResult PeriodEvalMemberList()
      //      {
      //         ViewBag.AccessorRoles = RoleHelper.GetUserRoles(GetUserInfo().UserId, db);

      //         ViewBag.AllPeriods = EvalPeriod.GetAll();

      //         return View();
      //      }

      //      [HttpPost]
      //      public ActionResult PeriodEvalMemberList(Guid periodId, Guid roleId, int current, int rowCount, AjaxOrder sort, string searchPhrase)
      //      {
      //         var memberViewModels = new AccessorTargetsHandler().GetTargetMembers(GetUserInfo().UserId, roleId, periodId, db);

      //         var total = memberViewModels.Count();

      //         if (total > 0)
      //            memberViewModels = memberViewModels
      //               .Skip(rowCount * current - rowCount)
      //               .Take(rowCount)
      //               .ToList();

      //         return Json(new
      //         {
      //            rows = memberViewModels,
      //            current,
      //            rowCount,
      //            total
      //         });
      //      }


      //	GET: EvalManage/AutoEvalMemberList
      //	POST-Ajax: EvalManage/AutoEvalMemberList

      public ActionResult AutoEvalMemberList()
      {
         return View();
      }

      //[HttpPost]
      //public ActionResult AutoEvalMemberList(int current, int rowCount, AjaxOrder sort, string searchPhrase)
      //{
      //   var memberViewModels = new GroupMemberHandler().GetTargetMembers(GetUserInfo().UserId, Guid.Empty, db);

      //   var total = memberViewModels.Count();

      //   if (total > 0)
      //      memberViewModels = memberViewModels
      //         .Skip(rowCount * current - rowCount)
      //         .Take(rowCount)
      //         .ToList();


      //   return Json(new
      //   {
      //      rows = memberViewModels,
      //      current,
      //      rowCount,
      //      total
      //   });
      //}


      //      public ActionResult PeriodAutoEvalMemberList()
      //      {
      //         ViewBag.AllPeriods = EvalPeriod.GetAll();

      //         return View();
      //      }

      //      [HttpPost]
      //      public ActionResult PeriodAutoEvalMemberList(Guid periodId, int current, int rowCount, AjaxOrder sort, string searchPhrase)
      //      {
      //         var memberViewModels = new GroupMemberHandler().GetTargetMembers(GetUserInfo().UserId, Guid.Empty, periodId, db);

      //         var total = memberViewModels.Count();

      //         if (total > 0)
      //            memberViewModels = memberViewModels.Skip(rowCount * current - rowCount).Take(rowCount).ToList();


      //         return Json(new
      //         {
      //            rows = memberViewModels,
      //            current,
      //            rowCount,
      //            total
      //         });
      //      }


      //	GET: EvalManage/SubjectEvalView
      //	POST-Ajax: EvalManage/SubjectEval

      public ActionResult SubjectEvalView(SubjectEvalParams parms)
      {
         parms.TableType = EvalTableKeys.SubjectType;

         return View(
                    GetEvalViewModel(parms)
                    );
      }

      [HttpPost]
      public ActionResult SubjectEval(List<EvalResultViewModel> evalResults)
      {
         foreach (var item in evalResults)
         {
            var evalResult = new EvalResult
            {
               ResultId = item.id.ToGuid(Guid.NewGuid()),
               AccesserId = item.accessorId.ToGuid(Guid.Empty),
               AccessDate = DateTime.Now,
               PeriodId = item.periodId.ToGuid(Guid.Empty),
               TableId = item.tableId.ToGuid(),
               TargetId = item.targetId.ToGuid(),
               EvalType = EvalTableKeys.SubjectType,
               Items = item.items.Select(i => new EvalResultItem
               {
                  ResultItemId = i.id.ToGuid(Guid.Empty),
                  ResultId = i.resultId.ToGuid(Guid.Empty),
                  EvalItemKey = i.key,
                  ResultValue = i.value,
                  Score = i.score,
                  IndicationId = i.indicationId.ToGuid(Guid.Empty)
               }).ToList()
            };

            var paras = new SubjectEvalParams
            {
               AccessorId = item.targetId.ToGuid(Guid.Empty),
               TargetId = item.targetId.ToGuid(Guid.Empty),
               CurrentTableId = item.tableId.ToGuid(Guid.Empty),
               db = db,
               PeriodId = item.periodId.ToGuid(Guid.Empty),
            };

            SubjectEvalAction(paras, evalResult);
         }

         return Json(new
         {
            result = AjaxResults.Success,
            msg = "操作成功"
         });
      }

      [HttpPost]
      public ActionResult AdjustScore(Guid resultId, double adjustScore)
      {
         var esr = APDBDef.EvalSubmitResult;

         var result = db.EvalSubmitResultDal.PrimaryGet(resultId);
         if (result != null)
         {
            APQuery.update(esr)
               .set(esr.AdjustScore, result.AdjustScore + adjustScore)
               .where(esr.SubmitResultId == resultId)
               .execute(db);
         }

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.EvalResult.Adjust_SUCCESS
         });
      }


      //	POST-Ajax: EvalManage/AutoEval

      [HttpPost]
      public ActionResult AutoEval(AutoEvalParams parms)
      {
         parms.TableType = EvalTableKeys.AutoType;
         var model = GetEvalViewModel(parms);
         var evalTables = model.EvalTables.Where(x=>x.TableType==EvalTableKeys.AutoType);
         AutoEvalParams param = null;

         db.BeginTrans();

         try
         {
            foreach (var item in evalTables)
            {
               param = new AutoEvalParams
               {
                  AccessorId = model.Accessor.UserId,
                  CurrentTableId = item.TableId,
                  PeriodId = model.Period.PeriodId,
                  db = db,
                  TargetId = model.Target.UserId,
                  Result = Result.Initial(),
               };

               AutoEvalAction(param);
            }

            db.Commit();
         }
         catch
         {
            db.Rollback();
         }


         if (param == null)
            param = new AutoEvalParams { Result = new Result { IsSuccess = false, Msg = "该考核对象没有自动考核表" } };

         return Json(new
         {
            result = param.Result.IsSuccess ? AjaxResults.Success : AjaxResults.Error,
            msg = param.Result.Msg
         });
      }


      //GET: EvalManage/AutoEvalResult
      //GET: EvalManage/SubjectEvalResult

      public ActionResult AutoEvalResult(AutoEvalParams parms)
      {
         parms.TableType = EvalTableKeys.AutoType;

         return View(
            GetEvalViewModel(parms)
            );
      }

      //      public ActionResult SubjectEvalResult(SubjectEvalResultParams parms)
      //      {
      //         return View(
      //            GetEvalViewModel(parms, EvalKeys.SubjectType)
      //            );
      //      }

      private void SubjectEvalAction(SubjectEvalParams paras, EvalResult result)
      {
         var evalPeriod = db.EvalPeriodDal.PrimaryGet(paras.PeriodId);
         var engin = EvalManager.Engines[evalPeriod.AnalysisType]
                                .SubjectEvals[paras.CurrentTableId] as SubjectEvalUnit;

         engin.Eval(paras, result);
      }

      private void AutoEvalAction(AutoEvalParams paras)
      {
         var evalPeriod = db.EvalPeriodDal.PrimaryGet(paras.PeriodId);
         var engin = EvalManager.Engines[evalPeriod.AnalysisType]
                                .AutoEvals[paras.CurrentTableId] as AutoEvalUnit;

         engin.Eval(paras);
      }


      private EvalViewModel GetEvalViewModel(EvalParams parms)
      {
         if (parms.TargetId.IsEmpty()) throw new ApplicationException("没有被考核人！");

         var period = parms.PeriodId.IsEmpty() ? EvalPeriod.GetCurrentPeriod(db).First() : EvalPeriod.PrimaryGet(parms.PeriodId);
         var target = UserInfo.PrimaryGet(parms.TargetId);
         var accessor = UserInfo.PrimaryGet(parms.AccessorId.IsEmpty() ? GetUserInfo().UserId : parms.AccessorId);
         var tableType = parms.TableType;

         var subquery = APQuery.select(ett.TableId).from(ett).where(ett.TargetId == target.UserId & ett.AccessorId == accessor.UserId & ett.PeriodId == period.PeriodId).group_by(ett.TableId);
         var evalTables = db.EvalTableDal.ConditionQuery(et.TableId.In(subquery), null, null, null);

         Dictionary<Guid, List<EvalResultItem>> results = new Dictionary<Guid, List<EvalResultItem>>();
         foreach (var item in evalTables)
         {
            if (item.TableType == tableType)
            {
               var resultItems = EvalManager.GetEvalReultItems(
                new EvalParams
                {
                   AccessorId = accessor.UserId,
                   CurrentTableId = item.TableId,
                   PeriodId = period.PeriodId,
                   TargetId = target.UserId,
                   db = db,
                });
               results.Add(item.TableId, resultItems);
            }
         }

         return new EvalViewModel
         {
            Accessor = accessor,
            Target = target,
            EvalTables = evalTables,
            Period = period,
            EvalResultItems = results
         };
      }

   }
}