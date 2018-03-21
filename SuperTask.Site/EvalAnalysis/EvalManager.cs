using Business;
using Business.Helper;
using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheSite.Models;

namespace TheSite.EvalAnalysis
{

   public class EvalManager
   {

      public static Dictionary<string, EvalEngine> Engines;
      public static string MonthlyAnylizeType = "Monthly V1.0";
      public static string QuarteryAnylizeType = "Quartery V1.0";
      public static string HalfYearAnylizeType = "HalfYear V1.0";
      public static string FullYearAnylizeType = "FullYear V1.0";

      static APDBDef.EvalIndicationTableDef ei = APDBDef.EvalIndication;
      static APDBDef.EvalResultTableDef er = APDBDef.EvalResult;
      static APDBDef.EvalResultItemTableDef eri = APDBDef.EvalResultItem;
      static APDBDef.EvalTableTableDef evt = APDBDef.EvalTable;
      static APDBDef.IndicationTableDef i = APDBDef.Indication;

      static EvalManager()
      {
         CreateEngines();
      }


      public static Dictionary<Guid, AnalysisUnit> GetAnalysisUnits(string anylizeType, string viewPath, int evalType, APDBDef db)
      {
         var evt = APDBDef.EvalPeriodTable;
         var e = APDBDef.EvalPeriod;
         var et = APDBDef.EvalTable;

         var tables = APQuery.select(et.Asterisk)
                            .from(et,
                                  evt.JoinInner(evt.TableId == et.TableId),
                                  e.JoinInner(evt.PeriodId == e.PeriodId & e.AnalysisType == anylizeType))
                            .where(et.TableType == evalType)
                            .query(db, r =>
                            {
                               var evalTable = new EvalTable();
                               et.Fullup(r, evalTable, false);
                               return evalTable;
                            }).ToList();

         var result = new Dictionary<Guid, AnalysisUnit>();
         tables.ForEach(t =>
         {
            if (t.TableType == EvalTableKeys.SubjectType && !result.ContainsKey(t.TableId))
               result.Add(t.TableId, new SubjectEvalUnit { ViewPath = viewPath });
            else if (t.TableType == EvalTableKeys.AutoType && !result.ContainsKey(t.TableId))
               result.Add(t.TableId, new AutoEvalUnit { ViewPath = viewPath });
         });

         return result;
      }


      public static List<EvalResultItem> GetEvalReultItems(EvalParams paras)
      {
         var evalIndications = GetEvalIndications(paras);
         if (evalIndications == null) return null;

         //获取考评结果
         var evalResultItmes = GetEvalResultItems(paras);

         //如果打过分则将指标实体放入考核结果明细
         if (evalResultItmes != null && evalResultItmes.Count > 0)
         {
            foreach (var item in evalResultItmes)
            {
               var evalIndication = evalIndications.FirstOrDefault(e => e.IndicationId == item.IndicationId);
               if (evalIndication != null)
                  item.EvalIndication = evalIndication;
            }
         }
         else
            evalResultItmes = evalIndications.Select(x => new EvalResultItem { EvalIndication = x }).ToList();


         return evalResultItmes;
      }


      public static List<EvalIndication> GetEvalIndications(EvalParams paras)
      {
         var evalIndications = APQuery.select(ei.FullScore, ei.Propertion, i.IndicationId, i.IndicationName, i.Description, evt.TableId.As("tableId"))
                             .from(ei,
                                   i.JoinLeft(ei.IndicationId == i.IndicationId),
                                   evt.JoinLeft(ei.TableId == evt.TableId));
         
         if (!paras.AccessorRoleId.IsEmpty())
            evalIndications.where_and(ei.AccessorRoleId == paras.AccessorRoleId);
         if (!paras.CurrentTableId.IsEmpty())
            evalIndications.where_and(ei.TableId == paras.CurrentTableId);

         var results = evalIndications.query(paras.db, r =>
         {
            return new EvalIndication
            {
               IndicationName = i.IndicationName.GetValue(r),
               IndicationDescription = i.Description.GetValue(r),
               FullScore = ei.FullScore.GetValue(r),// * (ei.Propertion.GetValue(r) / 100),
               TableId = evt.TableId.GetValue(r, "tableId"),
               IndicationId = i.IndicationId.GetValue(r)
            };
         }).ToList();

         return results;
      }


      public static void RefreshEngines()
      {
         CreateEngines();
      }


      private static void CreateEngines()
      {
         Engines = new Dictionary<string, EvalEngine>()
         {
            {MonthlyAnylizeType,new MonthlyEvalEngine
               { SubjectEvals=GetAnalysisUnits(MonthlyAnylizeType,MonthlyEvalEngine.ViewPath,EvalTableKeys.SubjectType, new APDBDef()),
                 AutoEvals=GetAnalysisUnits(MonthlyAnylizeType,MonthlyEvalEngine.ViewPath,EvalTableKeys.AutoType, new APDBDef()),
               }
            },
            {QuarteryAnylizeType,new QuarteryEvalEngine
               { SubjectEvals=GetAnalysisUnits(QuarteryAnylizeType,QuarteryEvalEngine.ViewPath,EvalTableKeys.SubjectType,new APDBDef()),
                 AutoEvals=GetAnalysisUnits(QuarteryAnylizeType,QuarteryEvalEngine.ViewPath,EvalTableKeys.AutoType, new APDBDef()),
               }
            },
             {HalfYearAnylizeType,new HalfYearEvalEngine
               { SubjectEvals=GetAnalysisUnits(HalfYearAnylizeType,HalfYearEvalEngine.ViewPath,EvalTableKeys.SubjectType,new APDBDef()),
                 AutoEvals=GetAnalysisUnits(HalfYearAnylizeType,HalfYearEvalEngine.ViewPath,EvalTableKeys.AutoType, new APDBDef()),
               }
            },
             {FullYearAnylizeType,new FullYearEvalEngine
               { SubjectEvals=GetAnalysisUnits(FullYearAnylizeType,FullYearEvalEngine.ViewPath,EvalTableKeys.SubjectType,new APDBDef()),
                 AutoEvals=GetAnalysisUnits(FullYearAnylizeType,FullYearEvalEngine.ViewPath,EvalTableKeys.AutoType, new APDBDef()),
               }
            },
         };
      }


      private static List<EvalResultItem> GetEvalResultItems(EvalParams paras)
      {
         var evalResultItmes = APQuery.select(eri.Asterisk)
           .from(er, eri.JoinInner(er.ResultId == eri.ResultId))
           .where(er.PeriodId == paras.PeriodId & er.TargetId == paras.TargetId);

         if (!paras.AccessorId.IsEmpty())
            evalResultItmes.where_and(er.AccesserId == paras.AccessorId);
         if (!paras.AccessorRoleId.IsEmpty())
            evalResultItmes.where_and(er.AccesserRoleId == paras.AccessorRoleId);
         if (!paras.CurrentTableId.IsEmpty())
            evalResultItmes.where_and(er.TableId == paras.CurrentTableId);
         if(!paras.TargetRoleId.IsEmpty())
            evalResultItmes.where_and(er.TargetRoleId == paras.TargetRoleId);

         var results = evalResultItmes.query(paras.db, r =>
             {
                var evalresultItem = new EvalResultItem();
                eri.Fullup(r, evalresultItem, false);
                return evalresultItem;
             }).ToList();

         return results;
      }

   }

}
