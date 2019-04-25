﻿using Business;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using TheSite.Models;
using System.Linq;
using Business.Helper;

namespace TheSite.EvalAnalysis
{

   public abstract class AnalysisUnit
   {
      public virtual string ViewPath { get; set; }

      public abstract string RoleView { get; }

      public abstract string EvalView { get; }

      public abstract string ResultView { get; }

      public Guid TableId { get; }

      public abstract List<EvalResultItem> GetEvalResultItems(EvalParams paras);
   }


   public class SubjectEvalUnit : AnalysisUnit
   {
      public override string RoleView => "SubjectRoleView";

      public override string EvalView => "SubjectEvalView";

      public override string ResultView => "SubjectResultView";

      public virtual void Eval(EvalParams paras, EvalResult result)
      {
         if (result == null) { throw new ApplicationException($"考核表结果为空"); }
         if (result.Items == null || result.Items.Count <= 0) { throw new ApplicationException($"考核表id{paras.TargetId}的考核项为空"); }

         var re = APDBDef.EvalResult;
         var rei = APDBDef.EvalResultItem;
         var db = paras.db;

         var periods = EvalPeriod.GetCurrentPeriod(paras.db);
         if (periods.Count <= 0) throw new ApplicationException();

         var currentPeriodIds = periods.Select(x => x.PeriodId).ToArray();
         // 由于会有多个考核周期，所以依据当前周期的id 考评着（角色）和被考评着（角色）进行查询
         var evalResults = EvalResult.ConditionQuery(
             re.PeriodId.In(currentPeriodIds) &
             re.AccesserId == result.AccesserId &
             re.TargetId == result.TargetId &
             re.TableId == paras.CurrentTableId
            , null);

         db.BeginTrans();

         try
         {
            if (evalResults.Count > 0)
            {
               var resultIds = evalResults.Select(x => x.ResultId).ToArray();
               db.EvalResultItemDal.ConditionDelete(rei.ResultId.In(resultIds));
               db.EvalResultDal.ConditionDelete(re.ResultId.In(resultIds));
            }
            else
            {
               result.ResultId = Guid.NewGuid();
            }

            foreach (var item in result.Items)
            {
               result.Score += item.Score;

               item.ResultItemId = Guid.NewGuid();
               item.ResultId = result.ResultId;
               db.EvalResultItemDal.Insert(item);
            }

            result.GroupId = paras.GroupId;

            db.EvalResultDal.Insert(result);


            db.Commit();
         }
         catch
         {
            db.Rollback();
         }
      }

      public override List<EvalResultItem> GetEvalResultItems(EvalParams paras)
      {
         return EvalManager.GetEvalReultItems(paras);
      }

   }


   public class AutoEvalUnit : AnalysisUnit
   {
      static APDBDef.EvalResultTableDef er = APDBDef.EvalResult;
      static APDBDef.EvalResultItemTableDef eri = APDBDef.EvalResultItem;

      public override string RoleView => "AutoRoleView";

      public override string ResultView => "AutoResultView";

      public override string EvalView => string.Empty;

      AutoEvalBuilder _builder;

      public AutoEvalUnit()
      {
         _builder = new AutoEvalBuilder();
         BuildAlgorithms();
      }

      public virtual void Eval(AutoEvalParams paras)
      {
         var db = paras.db;

         var results = new List<EvalResultItem>();
         var algorithmns = _builder.Algorithmns;
         var evalIndications = EvalManager.GetEvalIndications(paras);
         if (evalIndications.Count <= 0) return;

         foreach (var item in evalIndications)
         {
            if (algorithmns.ContainsKey(item.IndicationId))
            {
               paras.EvalIndication = item;

               results.Add(
                  algorithmns[item.IndicationId].Algorithmn.Invoke(paras)
                  );
            }
         }

         if (results.Count <= 0) return;

         RemoveExistsEvalResult(paras);

         var evalResult = new EvalResult
         {
            ResultId = Guid.NewGuid(),
            AccesserId = paras.AccessorId,
            AccessDate = DateTime.Now,
            PeriodId = paras.PeriodId,
            TargetId = paras.TargetId,
            TableId = paras.CurrentTableId,
            GroupId = paras.GroupId,
            EvalType = EvalTableKeys.AutoType,
         };

         foreach (var item in results)
         {
            evalResult.Score += item.Score;

            item.ResultId = evalResult.ResultId;
            db.EvalResultItemDal.Insert(item);
         }

         if (evalResult.Score < 0)
            evalResult.Score = 0;

         db.EvalResultDal.Insert(evalResult);
      }


      public override List<EvalResultItem> GetEvalResultItems(EvalParams paras)
      {
         return EvalManager.GetEvalReultItems(paras);
      }


      protected virtual void BuildAlgorithms()
      {
         _builder.BuildAutoEvalProcess(DefaultAlgorithms.WorkQuantityId, new DefaultAlgorithms.WorkQuantity());
         _builder.BuildAutoEvalProcess(DefaultAlgorithms.WorkEfficiencyId, new DefaultAlgorithms.WorkEfficiency());
         _builder.BuildAutoEvalProcess(DefaultAlgorithms.WorkComplexityId, new DefaultAlgorithms.WorkComplexity());
         _builder.BuildAutoEvalProcess(DefaultAlgorithms.BUGQuantityId, new DefaultAlgorithms.DevelopmentBugQuantity());
         _builder.BuildAutoEvalProcess(DefaultAlgorithms.TaskUploadFileQuantityId, new DefaultAlgorithms.TaskUploadFileQuantity());
         _builder.BuildAutoEvalProcess(DefaultAlgorithms.CostControlId, new DefaultAlgorithms.CostControl());
         _builder.BuildAutoEvalProcess(DefaultAlgorithms.BugetDiviationId, new DefaultAlgorithms.BugetDiviation());
         _builder.BuildAutoEvalProcess(DefaultAlgorithms.TaskQuantityId, new DefaultAlgorithms.WorkTaskQuantity());
         _builder.BuildAutoEvalProcess(DefaultAlgorithms.PlanTaskComplentionId, new DefaultAlgorithms.PlanTaskCompletion());
         _builder.BuildAutoEvalProcess(DefaultAlgorithms.PlanTaskTimelinessId, new DefaultAlgorithms.PlanTaskTimeliness());
         _builder.BuildAutoEvalProcess(DefaultAlgorithms.WorkJournalFillingRateId, new DefaultAlgorithms.WorkJournalFillingRate());
         _builder.BuildAutoEvalProcess(DefaultAlgorithms.ProjectComprehensiveId, new DefaultAlgorithms.ProjectComprehensive());
         _builder.BuildAutoEvalProcess(DefaultAlgorithms.TaskAchievementId, new DefaultAlgorithms.TaskAchievement());
         _builder.BuildAutoEvalProcess(DefaultAlgorithms.TaskInDeptId, new DefaultAlgorithms.TaskInDept());
      }


      private void RemoveExistsEvalResult(AutoEvalParams paras)
      {
         var db = paras.db;

         var existEvalResult = db.EvalResultDal.ConditionQuery(
         er.AccesserId == paras.AccessorId
         & er.TableId == paras.CurrentTableId
         & er.TargetId == paras.TargetId
         //& er.TargetRoleId == paras.TargetRoleId
         & er.PeriodId == paras.PeriodId
         , null, null, null).FirstOrDefault();

         if (existEvalResult != null)
         {
            db.EvalResultItemDal.ConditionDelete(eri.ResultId == existEvalResult.ResultId);
            db.EvalResultDal.ConditionDelete(er.ResultId == existEvalResult.ResultId);
         }
      }

   }

}
