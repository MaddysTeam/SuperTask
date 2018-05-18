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
         ViewBag.AccessorRoles = RoleHelper.GetUserRoles(GetUserInfo().UserId, db);

         return View();
      }

      [HttpPost]
      public ActionResult EvalMemberList(Guid roleId, int current, int rowCount, AjaxOrder sort, string searchPhrase)
      {
         //var period = EvalPeriod.GetCurrentPeriod(db);

         var memberViewModels = new AccessorTargetsHandler().GetTargetMembers(GetUserInfo().UserId, roleId, db);

         var total = memberViewModels.Count();

         if (total > 0)
            memberViewModels = memberViewModels
               .Skip(rowCount * current - rowCount)
               .Take(rowCount)
               .ToList();


         return Json(new
         {
            rows = memberViewModels,
            current,
            rowCount,
            total
         });
      }


      //	GET: EvalManage/PeriodEvalMemberList
      //	POST-Ajax: EvalManage/PeriodEvalMemberList

      public ActionResult PeriodEvalMemberList()
      {
         ViewBag.AccessorRoles = RoleHelper.GetUserRoles(GetUserInfo().UserId, db);

         ViewBag.AllPeriods = EvalPeriod.GetAll();

         return View();
      }

      [HttpPost]
      public ActionResult PeriodEvalMemberList(Guid periodId, Guid roleId, int current, int rowCount, AjaxOrder sort, string searchPhrase)
      {
         var memberViewModels = new AccessorTargetsHandler().GetTargetMembers(GetUserInfo().UserId, roleId, periodId, db);

         var total = memberViewModels.Count();

         if (total > 0)
            memberViewModels = memberViewModels
               .Skip(rowCount * current - rowCount)
               .Take(rowCount)
               .ToList();

         return Json(new
         {
            rows = memberViewModels,
            current,
            rowCount,
            total
         });
      }


      //	GET: EvalManage/AutoEvalMemberList
      //	POST-Ajax: EvalManage/AutoEvalMemberList

      public ActionResult AutoEvalMemberList()
      {
         return View();
      }

      [HttpPost]
      public ActionResult AutoEvalMemberList(int current, int rowCount, AjaxOrder sort, string searchPhrase)
      {
         var memberViewModels = new GroupMemberHandler().GetTargetMembers(GetUserInfo().UserId, Guid.Empty, db);

         var total = memberViewModels.Count();

         if (total > 0)
            memberViewModels = memberViewModels
               .Skip(rowCount * current - rowCount)
               .Take(rowCount)
               .ToList();


         return Json(new
         {
            rows = memberViewModels,
            current,
            rowCount,
            total
         });
      }


      public ActionResult PeriodAutoEvalMemberList()
      {
         ViewBag.AllPeriods = EvalPeriod.GetAll();

         return View();
      }

      [HttpPost]
      public ActionResult PeriodAutoEvalMemberList(Guid periodId, int current, int rowCount, AjaxOrder sort, string searchPhrase)
      {
         var memberViewModels = new GroupMemberHandler().GetTargetMembers(GetUserInfo().UserId, Guid.Empty, periodId, db);

         var total = memberViewModels.Count();

         if (total > 0)
            memberViewModels = memberViewModels.Skip(rowCount * current - rowCount).Take(rowCount).ToList();


         return Json(new
         {
            rows = memberViewModels,
            current,
            rowCount,
            total
         });
      }


      //	GET: EvalManage/SubjectEvalView
      //	POST-Ajax: EvalManage/SubjectEval

      public ActionResult SubjectEvalView(SubjectEvalParams parms)
      {
         return View(
                    GetEvalViewModel(parms, EvalKeys.SubjectType)
                    );
      }

      [HttpPost]
      public ActionResult SubjectEval(List<EvalResultViewModel> evalResults)
      {
         foreach (var item in evalResults)
         {
            var evalResult = new EvalResult
            {
               ResultId = item.id.ToGuid(Guid.Empty),
               AccesserId = item.accessorId.ToGuid(Guid.Empty),
               AccesserRoleId = item.accessorRoleId.ToGuid(Guid.Empty),
               AccessDate = DateTime.Now,
               PeriodId = item.periodId.ToGuid(Guid.Empty),
               TableId = item.tableId.ToGuid(),
               TargetId = item.targetId.ToGuid(),
               EvalType = EvalTableKeys.SubjectType,
               TargetRoleId = item.targetRoleId.ToGuid(Guid.Empty),
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


      //	POST-Ajax: EvalManage/AutoEval

      [HttpPost]
      public ActionResult AutoEval(AutoEvalParams parms)
      {
         var model = GetEvalViewModel(parms, EvalKeys.AutoType);
         var periodTables = model.PeriodTables;
         AutoEvalParams param = null;

         db.BeginTrans();

         try
         {
            foreach (var item in periodTables)
            {
               param = new AutoEvalParams
               {
                  AccessorId = model.AccessorId,
                  CurrentTableId = item.TableId,
                  PeriodId = item.PeriodId,
                  db = db,
                  TargetId = model.TargetId,
                  Result = Result.Initial(),
                  TargetRoleId = parms.TargetRoleId
               };

               AutoEvalAction(param);
            }

            db.Commit();
         }
         catch(Exception e)
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
         return View(
            GetEvalViewModel(parms, EvalKeys.AutoType)
            );
      }

      public ActionResult SubjectEvalResult(SubjectEvalResultParams parms)
      {
         return View(
            GetEvalViewModel(parms, EvalKeys.SubjectType)
            );
      }


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


      private EvalViewModel GetEvalViewModel(EvalParams parms, int evalType)
      {
         var targetId = parms.TargetId;
         var accessorId = parms.AccessorId.IsEmpty() ? GetUserInfo().UserId : parms.AccessorId;
         var accessorRoleId = parms.AccessorRoleId;
         var targetRoleId = parms.TargetRoleId;

         if (targetId.IsEmpty()) throw new ApplicationException("没有考核对象！");
         if (accessorRoleId.IsEmpty()) throw new ApplicationException("必须选择一个考核角色！");

         var target = UserInfo.PrimaryGet(parms.TargetId);
         var evalResuls = EvalResult.GetTargetEvalResult(accessorId, accessorRoleId, targetId, targetRoleId, evalType);
         var resultTables = parms.PeriodId.IsEmpty() ?
            EvalPeriodTable.GetAvaliableEvalTables(targetId, accessorRoleId,parms.TargetRoleId, evalType, evalResuls, db) :
            EvalPeriodTable.GetPeriodEvaledTables(parms.PeriodId, evalType, evalResuls, db);
         var accessor = UserInfo.PrimaryGet(accessorId);

         return new EvalViewModel
         {
            AccessorId = accessor.UserId,
            AccessorName = accessor.UserName,
            TargetId = target.UserId,
            TargetName = target.UserName,
            AccessorRole = new Role { RoleId = parms.AccessorRoleId },
            PeriodTables = resultTables,
            TargetRoleId=targetRoleId
         };
      }

   }
}