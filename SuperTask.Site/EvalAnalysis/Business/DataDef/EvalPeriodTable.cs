using Business.Helper;
using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TheSite.EvalAnalysis;

namespace Business
{

   public partial class EvalPeriodTable
   {
      public string TableName { get; set; }

      public double Score { get; set; }

      public string AccessorRoleIds { get; set; }

      public string TargetRoIds { get; set; }

      public int TableType { get; set; }

      public Guid CurrentTargetRoleId { get; set; }

      public string CurrentTargetRoleIds { get; set; }


      public static List<EvalPeriodTable> GetAllEvalPeriodTables(APDBDef db, int? evalType = null)
      {
         var ept = APDBDef.EvalPeriodTable;
         var t = APDBDef.EvalTable;

         var query = APQuery.select(ept.Asterisk, t.TableName, t.AccessorRoleIds, t.TableType, t.MemberRoleIds)
            .from(ept, t.JoinInner(t.TableId == ept.TableId))
            .where(t.TableStatus == EvalTableKeys.DoneStatus);

         if (evalType != null)
            query.where_and(t.TableType == evalType);


         return query.query(db, r =>
         {
            var evalPeriodTable = new EvalPeriodTable();
            ept.Fullup(r, evalPeriodTable, false);
            evalPeriodTable.TableName = t.TableName.GetValue(r);
            evalPeriodTable.AccessorRoleIds = t.AccessorRoleIds.GetValue(r);
            evalPeriodTable.TableType = t.TableType.GetValue(r);
            evalPeriodTable.TargetRoIds = t.MemberRoleIds.GetValue(r);

            return evalPeriodTable;
         }).ToList();
      }


      public static List<EvalPeriodTable> GetAllAvaliableEvalPeriodTables(APDBDef db, int? evalType = null)
      {
         var ept = APDBDef.EvalPeriodTable;
         var t = APDBDef.EvalTable;
         var er = APDBDef.EvalResult;
         var ep = APDBDef.EvalPeriod;

         var current = DateTime.Now;
         var subQuery = APQuery.select(ep.PeriodId).from(ep).where(current <= ep.AccessEndDate & current >= ep.AccessBeginDate);

         var query = APQuery.select(ept.Asterisk, t.TableName, t.AccessorRoleIds, t.TableType, t.MemberRoleIds)
            .from(ept, t.JoinInner(t.TableId == ept.TableId))
            .where(t.TableStatus == EvalTableKeys.DoneStatus & ept.PeriodId.In(subQuery));

         if (evalType != null)
            query.where_and(t.TableType == evalType);


         return query.query(db, r =>
         {
            var evalPeriodTable = new EvalPeriodTable();
            ept.Fullup(r, evalPeriodTable, false);
            evalPeriodTable.TableName = t.TableName.GetValue(r);
            evalPeriodTable.AccessorRoleIds = t.AccessorRoleIds.GetValue(r);
            evalPeriodTable.TableType = t.TableType.GetValue(r);
            evalPeriodTable.TargetRoIds = t.MemberRoleIds.GetValue(r);

            return evalPeriodTable;
         }).ToList();
      }


      public static List<EvalPeriodTable> GetAvaliableEvalTables(Guid targetId, Guid accessorRoleId, Guid targetRoleId, int evalType, List<EvalResult> evalResults, APDBDef db = null)
      {
         db = db ?? new APDBDef();
         var originTables = EvalPeriodTable.GetAllAvaliableEvalPeriodTables(db, EvalKeys.SubjectType);

         //从考核结果得到考核表
         var resultTables = new List<EvalPeriodTable>();
         if (evalResults != null)
            foreach (var item in evalResults)
            {
               var table = originTables.Find(x => x.TableId == item.TableId && x.PeriodId == item.PeriodId && x.TableType == evalType);
               if (table != null)
               {
                  table.Score = item.Score;
                  table.CurrentTargetRoleId = item.TargetRoleId;
                  resultTables.Add(table);
               }
            }

         //得到考核对象及搜索其是否有考核表的选择
         var egt = APDBDef.EvalGroupTarget;
         var evalgroupTarget = EvalGroupTarget.ConditionQuery(egt.GroupId ==
                              EvalGroupConfig.DefaultGroupId.ToGuid(Guid.Empty)
                              & egt.MemberId == targetId, null)
                              .FirstOrDefault();
         if (evalgroupTarget == null) return new List<EvalPeriodTable>();

         if (!string.IsNullOrEmpty(evalgroupTarget.TableIds))
         {
            foreach (var tableId in evalgroupTarget.TableIds.Split(','))
            {
               var table = originTables.Find(x => x.TableId == tableId.ToGuid(Guid.Empty)
               && x.AccessorRoleIds.Contains(accessorRoleId.ToString())
               && x.TargetRoIds.Contains(targetRoleId.ToString()));

               if (table != null && !resultTables.Contains(table))
               {
                  table.CurrentTargetRoleId = targetRoleId;
                  resultTables.Add(table);
               }
            }
         }

         if (resultTables.Count > 0) return resultTables;

         //根据其角色读取允许使用的考核表

         var targetAllowedTables = GetEvalPeriodTablesByTableIds(evalgroupTarget.TableIds, evalType, db);
         if (targetAllowedTables.Count > 0)
            originTables = targetAllowedTables;

         foreach (var item in originTables)
         {
            if (item.TargetRoIds.Contains(targetRoleId.ToString()) &&
             item.AccessorRoleIds.Contains(accessorRoleId.ToString()))
            {
               item.Score = 0;
               item.CurrentTargetRoleId = targetRoleId;
               resultTables.Add(item);
            }
         }

         return resultTables;
      }


      public static List<EvalPeriodTable> GetPeriodEvaledTables(Guid periodId, int evalType, List<EvalResult> evalResults, APDBDef db = null)
      {
         db = db ?? new APDBDef();

         var originTables = EvalPeriodTable.GetAllAvaliableEvalPeriodTables(db, evalType);
         var resultTables = new List<EvalPeriodTable>();

         foreach (var item in evalResults)
         {
            var table = originTables.Find(x =>  x.TableId == item.TableId && x.PeriodId == periodId && x.TableType == evalType);
            if (table != null)
            {
               table.Score = item.Score;
               table.CurrentTargetRoleId = item.TargetRoleId;
               resultTables.Add(table);
            }
         }

            return resultTables;
      }


      public static List<EvalPeriodTable> GetEvalPeriodTablesByTableIds(string tableIds, int evalType, APDBDef db)
      {
         if (string.IsNullOrEmpty(tableIds)) return new List<EvalPeriodTable>();

         var tables = GetAllAvaliableEvalPeriodTables(db);
         var results = new List<EvalPeriodTable>();
         foreach (var item in tableIds.Split(',').ToList())
         {
            var table = tables.Where(x => x.TableType == evalType).FirstOrDefault(x => x.TableId.ToString() == item);
            if (table != null)
               results.Add(table);
         }

         return results;
      }


      public static string GetTableIdsByList(List<EvalPeriodTable> tables)
      {
         if (tables != null && tables.Count > 0)
            return string.Join(",", tables.Select(x => x.TableId).ToArray());
         return string.Empty;
      }


      public static string FilteTableIds(string tableIds, int evalType, APDBDef db)
      {
         var tables = GetEvalPeriodTablesByTableIds(tableIds, evalType, db).Where(x => x.TableType == evalType);

         return tables == null && tableIds.Count() > 0 ? string.Join(",", tableIds.ToArray())
                                                       : string.Empty;
      }
   }

}