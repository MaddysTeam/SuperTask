using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheSite.Models;
using Business;
using Symber.Web.Data;
using Business.Helper;
using Business.Config;

namespace TheSite.EvalAnalysis
{

   public partial class DefaultAlgorithms
   {

      public static Guid WorkQuantityId => Guid.Parse("E00AA280-45DA-4D22-9EA3-6CC7C1E4E129");
      public static Guid WorkEfficiencyId => Guid.Parse("CD9B2F71-1906-4517-B194-EC1786B76B6E");
      public static Guid WorkComplexityId => Guid.Parse("E5EFC6FD-71D8-4EE7-80AC-9312FBD4EA6B");
      public static Guid BUGQuantityId => Guid.Parse("F07A2DE3-FAFB-4AC0-A72E-FFA8ECA49978");
      public static Guid TaskUploadFileQuantityId = Guid.Parse("9A5E5C49-131A-4E58-A88D-F8C628BD2E47");
      public static Guid ProjectQualtiyId => Guid.Parse("F08C2DE9-FCFB-4BC0-A72E-FFA8ECA49978");
      public static Guid ExcutiveCapabilityId = Guid.Parse("F07A4ED9-FDFB-4AC0-A72E-FFA8ECB49978");
      public static Guid CostControlId = Guid.Parse("F09E2DE9-FAFB-4AC1-A72F-FFA8ECA41978");
      public static Guid BugetDiviationId = Guid.Parse("F14A2DE3-FFFB-4BC0-AB2E-FFA8ECA42978");


      /// <summary>
      /// 工作量算法
      /// </summary>
      internal class WorkQuantity : IIndicationAlgorithmn<AutoEvalParams, EvalResultItem>
      {
         public Func<AutoEvalParams, EvalResultItem> Algorithmn
         => (paras) =>
         {
            var evalIndication = paras.EvalIndication;
            var taskComplexities = ComplexityHelper.GetFloatComplexity(paras.db);//获取角色对应的复杂度*百分比
            var tasks = Util.GetWorkTaskByWorkJournal(paras);//获取当前考核期内的所有任务
            var starndPropertion = TaskCompelxtiyRole.GetAll().Find(x => x.IsStandard).Propertion;//获取标准复杂度所占比率
            double score = 0;

            foreach (var item in tasks)
            {
               if (item.WorkHours <= 0) continue;
               if (item.StandardComplextiy == 0)
                  item.StandardComplextiy = ThisApp.DefalutTaskStandardComplexity;

               //浮动复杂度=项目经理设置的复杂度*角色占比%+ 标准复杂度*角色占比%
               var floatComplexities = taskComplexities.Where(x => x.TaskId == item.TaskId).Sum(x => x.Complexity);
               if (floatComplexities == 0)
                  floatComplexities = item.StandardComplextiy;
               else
                  floatComplexities += (item.StandardComplextiy * starndPropertion / 100);
               //浮动工时=(浮动复杂度/标准复杂度)*标准工时
               var floatWorkhours = (floatComplexities / item.StandardComplextiy) * item.StandardWorkhours;
               //工作量得分= (浮动工时 / 22*8)*指标满分
               score += (floatWorkhours / 176) * evalIndication.FullScore;
            }

            if (score > paras.EvalIndication.FullScore)
               score = paras.EvalIndication.FullScore;

            return new EvalResultItem
            {
               ResultItemId = Guid.NewGuid(),
               PeriodId = paras.PeriodId,
               Score = score.Round(1),
               TableId = paras.CurrentTableId,
               IndicationId = paras.EvalIndication.IndicationId
            };
         };

         public Guid Id => WorkQuantityId;

         public string Name => "工作量";
      }


      /// <summary>
      /// 工作效率算法
      /// </summary>
      internal class WorkEfficiency : IIndicationAlgorithmn<AutoEvalParams, EvalResultItem>
      {
         public Func<AutoEvalParams, EvalResultItem> Algorithmn
         => (paras) =>
         {
            var evalIndication = paras.EvalIndication;
            var taskComplexities = ComplexityHelper.GetFloatComplexity(paras.db);//获取角色对应的复杂度*百分比
            var tasks = Util.GetWorkTaskByWorkJournal(paras);
            var starndPropertion = TaskCompelxtiyRole.GetAll().Find(x => x.IsStandard).Propertion;//获取标准复杂度所占比率

            double totalWorkhours = 0;
            double floatWorkhours = 0;
            foreach (var item in tasks)
            {
               if (item.WorkHours <= 0) continue;
               if (item.StandardComplextiy == 0)
                  item.StandardComplextiy = ThisApp.DefalutTaskStandardComplexity;

               //浮动复杂度=项目经理设置的复杂度*角色占比%+ 标准复杂度*角色占比%
               var floatComplexities = taskComplexities.Where(x => x.TaskId == item.TaskId).Sum(x => x.Complexity);
               if (floatComplexities == 0)
                  floatComplexities = item.StandardComplextiy;
               else
                  floatComplexities += (item.StandardComplextiy * starndPropertion / 100);
               //浮动工时=(浮动复杂度/标准复杂度)*标准工时
               floatWorkhours += (floatComplexities / item.StandardComplextiy) * item.StandardWorkhours;
               totalWorkhours += item.WorkHours;
            }

            double score = 0;
            if (totalWorkhours > 0) score = (floatWorkhours / totalWorkhours) * evalIndication.FullScore;
            if (score > paras.EvalIndication.FullScore)
               score = paras.EvalIndication.FullScore;

            return new EvalResultItem
            {
               ResultItemId = Guid.NewGuid(),
               PeriodId = paras.PeriodId,
               Score = score.Round(1),
               TableId = paras.CurrentTableId,
               IndicationId = paras.EvalIndication.IndicationId
            };
         };

         public Guid Id => WorkEfficiencyId;

         public string Name => "工作效率";
      }


      /// <summary>
      /// 最高复杂度算法
      /// </summary>
      internal class WorkComplexity : IIndicationAlgorithmn<AutoEvalParams, EvalResultItem>
      {
         public Func<AutoEvalParams, EvalResultItem> Algorithmn
         => (paras) =>
         {
            var evalIndication = paras.EvalIndication;
            var taskComplexities = ComplexityHelper.GetFloatComplexity(paras.db);//获取角色对应的复杂度*百分比
            var tasks = Util.GetWorkTaskByWorkJournal(paras);
            var starndPropertion = TaskCompelxtiyRole.GetAll().Find(x => x.IsStandard).Propertion;//获取标准复杂度所占比率
            double score = 0;

            foreach (var item in tasks)
            {
               if (item.WorkHours <= 0) continue;
               if (item.StandardComplextiy == 0)
                  item.StandardComplextiy = ThisApp.DefalutTaskStandardComplexity;

               //浮动复杂度=项目经理或其他角色设置的复杂度*角色占比%+ 标准复杂度*角色占比%
               var floatComplexities = taskComplexities.Where(x => x.TaskId == item.TaskId).Sum(x => x.Complexity);//其他角色复杂度之和
               if (floatComplexities == 0)
                  floatComplexities = item.StandardComplextiy;
               else
                  floatComplexities += (item.StandardComplextiy * starndPropertion / 100);//最后加上标准复杂度*角色占比

               if (score < floatComplexities)
                  score = floatComplexities;
            }

            if (score > paras.EvalIndication.FullScore)
               score = paras.EvalIndication.FullScore;

            return new EvalResultItem
            {
               ResultItemId = Guid.NewGuid(),
               PeriodId = paras.PeriodId,
               Score = score.Round(1),
               TableId = paras.CurrentTableId,
               IndicationId = paras.EvalIndication.IndicationId
            };
         };

         public Guid Id => WorkComplexityId;

         public string Name => "复杂度";
      }


      /// <summary>
      /// 开发BUG量算法
      /// </summary>
      internal class DevelopmentBugQuantity : IIndicationAlgorithmn<AutoEvalParams, EvalResultItem>
      {
         public Func<AutoEvalParams, EvalResultItem> Algorithmn
          => (paras) =>
          {
             var issueCount = Util.GetTaskIssuesCount(paras);
             var score = paras.EvalIndication.FullScore;
             score = issueCount >= score ? 0 : score - issueCount;

             return new EvalResultItem
             {
                ResultItemId = Guid.NewGuid(),
                PeriodId = paras.PeriodId,
                Score = score,
                TableId = paras.CurrentTableId,
                IndicationId = paras.EvalIndication.IndicationId
             };
          };

         public Guid Id => BUGQuantityId;

         public string Name => "BUG量";
      }


      /// <summary>
      /// 项目质量算法
      /// </summary>
      internal class ProjectQualtiy : IIndicationAlgorithmn<AutoEvalParams, EvalResultItem>
      {
         public Func<AutoEvalParams, EvalResultItem> Algorithmn
          => (paras) =>
          {
             var issueCount = Util.GetTaskIssuesCount(paras);
             var score = paras.EvalIndication.FullScore;
             score = issueCount >= score ? 0 : score - issueCount;

             return new EvalResultItem
             {
                ResultItemId = Guid.NewGuid(),
                PeriodId = paras.PeriodId,
                Score = score,
                TableId = paras.CurrentTableId,
                IndicationId = paras.EvalIndication.IndicationId
             };
          };

         public Guid Id => ProjectQualtiyId;

         public string Name => "项目质量";
      }


      /// <summary>
      /// 交付物确认算法
      /// </summary>
      internal class TaskUploadFileQuantity : IIndicationAlgorithmn<AutoEvalParams, EvalResultItem>
      {
         public Func<AutoEvalParams, EvalResultItem> Algorithmn
          => (paras) =>
          {
             var a = APDBDef.Attachment;
             var t = APDBDef.WorkTask;

             var evalIndication = paras.EvalIndication;
             var period = EvalPeriod.PrimaryGet(paras.PeriodId);
             var tasks = Util.GetWorkTaskByWorkJournal(paras);//获取当前考核期内的所有任务
             var result = APQuery.select(t.TaskId, t.TaskStatus, a.AttachmentId.As("attachmentId"))
                                 .from(t, a.JoinLeft(a.TaskId == t.TaskId))
                                 .where(a.UploadDate >= period.BeginDate
                                      & a.UploadDate <= period.EndDate
                                      & t.ManagerId == paras.TargetId
                                      & t.IsParent == false
                                      )
                                 .query(paras.db, r => new
                                 {
                                    TaskId = t.TaskId.GetValue(r),
                                    AttachmentId = a.AttachmentId.GetValue(r, "attachmentId"),
                                    Status = t.TaskStatus.GetValue(r)
                                 });


             var uploadedAndCompletedTaskCount = result.Where(x => !x.AttachmentId.IsEmpty()
                                                             && x.Status == TaskKeys.CompleteStatus)
                                                   .GroupBy(x => x.TaskId).Count();
             var score = tasks.Count == 0 ? 0 : ((double)uploadedAndCompletedTaskCount / tasks.Count) * evalIndication.FullScore;
             if (score > paras.EvalIndication.FullScore)
                score = paras.EvalIndication.FullScore;

             return new EvalResultItem
             {
                ResultItemId = Guid.NewGuid(),
                PeriodId = paras.PeriodId,
                Score = score.Round(1),
                TableId = paras.CurrentTableId,
                IndicationId = paras.EvalIndication.IndicationId
             };
          };

         public Guid Id => TaskUploadFileQuantityId;

         public string Name => "交付物确认";
      }


      /// <summary>
      /// 执行力算法
      /// </summary>
      internal class ExcutiveCapability : IIndicationAlgorithmn<AutoEvalParams, EvalResultItem>
      {
         public Func<AutoEvalParams, EvalResultItem> Algorithmn
         => (paras) =>
         {
            var wk = APDBDef.WorkTask;

            var period = EvalPeriod.PrimaryGet(paras.PeriodId);
            var evalIndication = paras.EvalIndication;
            var tasks = WorkTask.ConditionQuery(wk.ManagerId == paras.TargetId
                                              & wk.StartDate <= period.EndDate & wk.EndDate >= period.BeginDate
                                              , null);
            var completeTasks = tasks.Where(x => x.RealEndDate <= x.EndDate);

            double score = 0;
            if (tasks.Count > 0 && completeTasks.Count() > 0)
               score = ((double)completeTasks.Count() / tasks.Count()) * evalIndication.FullScore;

            if (score > paras.EvalIndication.FullScore)
               score = paras.EvalIndication.FullScore;

            return new EvalResultItem
            {
               ResultItemId = Guid.NewGuid(),
               PeriodId = paras.PeriodId,
               Score = score,
               TableId = paras.CurrentTableId,
               IndicationId = paras.EvalIndication.IndicationId
            };
         };

         public Guid Id => ExcutiveCapabilityId;

         public string Name => "执行力";
      }


      /// <summary>
      /// 成本控制算法
      /// </summary>
      internal class CostControl : IIndicationAlgorithmn<AutoEvalParams, EvalResultItem>
      {
         public Func<AutoEvalParams, EvalResultItem> Algorithmn
         => (paras) =>
         {
            var evalIndication = paras.EvalIndication;

            return new EvalResultItem
            {
               ResultItemId = Guid.NewGuid(),
               PeriodId = paras.PeriodId,
               Score = evalIndication.FullScore,
               TableId = paras.CurrentTableId,
               IndicationId = paras.EvalIndication.IndicationId
            };
         };

         public Guid Id => CostControlId;

         public string Name => "成本控制";
      }


      /// <summary>
      /// 预算偏差算法
      /// </summary>
      internal class BugetDiviation : IIndicationAlgorithmn<AutoEvalParams, EvalResultItem>
      {
         public Func<AutoEvalParams, EvalResultItem> Algorithmn
         => (paras) =>
         {
            var evalIndication = paras.EvalIndication;

            return new EvalResultItem
            {
               ResultItemId = Guid.NewGuid(),
               PeriodId = paras.PeriodId,
               Score = evalIndication.FullScore,
               TableId = paras.CurrentTableId,
               IndicationId = paras.EvalIndication.IndicationId
            };
         };

         public Guid Id => BugetDiviationId;

         public string Name => "预算偏差";
      }


      class Util
      {

         /// <summary>
         /// 通过工时，得到考核周期内的任务情况，包含工时、任务标准工时和标准复杂度
         /// </summary>
         /// <param name="paras">考核参数</param>
         /// <returns>工时集合</returns>
         internal static List<WorkTask> GetWorkTaskByWorkJournal(AutoEvalParams paras)
         {
            var wj = APDBDef.WorkJournal;
            var t = APDBDef.WorkTask;
            var st = APDBDef.TaskStandardItem;

            var period = EvalPeriod.PrimaryGet(paras.PeriodId);

            var result = APQuery.select(wj.TaskId, wj.WorkHours.Sum().As("TotalWorkHours"), st.StandardWorkhours, st.StandardComplextiy)
                            .from(wj,
                                  t.JoinInner(wj.TaskId == t.TaskId),
                                  st.JoinLeft(st.ItemId == t.StandardItemId)
                                  )
                            .where(wj.RecordDate >= period.BeginDate
                            & wj.RecordDate <= period.EndDate
                            & t.ManagerId == paras.TargetId
                            & t.IsParent == false)
                            .group_by(wj.TaskId, t.EstimateWorkHours, st.StandardWorkhours, st.StandardComplextiy)
                            .query(paras.db, r =>
                            {
                               var StandardComplexity = st.StandardComplextiy.GetValue(r);
                               if (StandardComplexity == 0)
                                  StandardComplexity = Business.Config.ThisApp.DefalutTaskStandardComplexity;
                               return new WorkTask
                               {
                                  TaskId = wj.TaskId.GetValue(r),
                                  WorkHours = wj.WorkHours.GetValue(r, "TotalWorkHours"),
                                  StandardComplextiy = StandardComplexity,
                                  StandardWorkhours = st.StandardWorkhours.GetValue(r)
                               };
                            }).ToList();

            return result;
         }


         /// <summary>
         /// 得到任务问题(bug)数
         /// </summary>
         /// <param name="paras">考核参数</param>
         /// <returns>bug数</returns>
         internal static int GetTaskIssuesCount(AutoEvalParams paras)
         {
            var t = APDBDef.WorkTask;
            var wti = APDBDef.WorkTaskIssue;
            var wj = APDBDef.WorkJournal;

            var period = EvalPeriod.PrimaryGet(paras.PeriodId);
            var tasks = Util.GetWorkTaskByWorkJournal(paras); //通过日志获取考核时间范围内的所有考核对象负责的任务
            var subQuery = APQuery.select(wj.TaskId)
                            .from(wj,t.JoinInner(wj.TaskId == t.TaskId))
                            .where(wj.RecordDate >= period.BeginDate
                            & wj.RecordDate <= period.EndDate
                            & t.ManagerId == paras.TargetId
                            & t.IsParent == false)
                            .group_by(wj.TaskId);

            var query =
             APQuery
            .select(wti.TaskIssueId)
            .from(wti)
            .where(wti.TaskId.In(subQuery));

            return paras.db.ExecuteSizeOfSelect(query);
         }

      }

   }

}
