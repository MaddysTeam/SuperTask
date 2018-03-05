using Business;
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

      public abstract string EvalView { get;}

      public abstract string ResultView { get;}

      public Guid TableId { get; }

      public abstract List<EvalResultItem> GetEvalResultItems(EvalParams paras);
   }


   public class SubjectEvalUnit : AnalysisUnit
   {
      public override string RoleView => "SubjectRoleView";

      public override string EvalView => "SubjectEvalView";

      public override string ResultView => "SubjectResultView";

      public virtual void Eval(EvalParams paras,EvalResult result)
      {
         if (result == null) { throw new ApplicationException($"考核表结果为空"); }
         if (result.Items == null || result.Items.Count <= 0) { throw new ApplicationException($"考核表id{paras.TargetId}的考核项为空"); }

         var re = APDBDef.EvalResult;
         var rei = APDBDef.EvalResultItem;
         var db = paras.db;
         var evaResult = EvalResult.PrimaryGet(result.ResultId);

         db.BeginTrans();

         try
         {
            if (!result.ResultId.IsEmpty())
            {
              
               if (evaResult != null)
               {
                  db.EvalResultItemDal.ConditionDelete(rei.ResultId == evaResult.ResultId);
                  db.EvalResultDal.ConditionDelete(re.ResultId == evaResult.ResultId);
               }
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
         var evalIndications = EvalTable.GetEvalIndications(paras.CurrentTableId);
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
            AccesserRoleId=EvalConfig.AutoAccessorRoleId.ToGuid(Guid.Empty),
            GroupId = paras.GroupId,
            EvalType = EvalTableKeys.AutoType,
            TargetRoleId=paras.TargetRoleId
         };

         foreach (var item in results)
         {
            evalResult.Score += item.Score;

            item.ResultId = evalResult.ResultId;
            db.EvalResultItemDal.Insert(item);
         }

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
      }


      private void RemoveExistsEvalResult(AutoEvalParams paras)
      {
         var db = paras.db;

         var existEvalResult = db.EvalResultDal.ConditionQuery(
         er.AccesserId == paras.AccessorId
         & er.TableId == paras.CurrentTableId
         & er.TargetId == paras.TargetId
         & er.TargetRoleId==paras.TargetRoleId
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
