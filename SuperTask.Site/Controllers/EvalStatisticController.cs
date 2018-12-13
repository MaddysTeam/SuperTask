using Business;
using Business.Helper;
using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TheSite.Models;

namespace TheSite.Controllers
{

   /// <summary>
   /// 考核统计，所有报表逻辑先暂定使用dapper 用于简化
   /// </summary>
   public class EvalStatisticController : BaseController
   {

      // GET: Statistic/EvalResultReport
      // POST-Ajax: Statistic/EvalResultReport

      public ActionResult EvalResultReport(Guid? targetId)
      {
         ViewBag.Periods = EvalPeriod.GetAll();

         return View();
      }

      [HttpPost]
      public ActionResult EvalResultReport(Guid periodId, int current, int rowCount, AjaxOrder sort, string start, string end, string searchPhrase)
      {
         var esr = APDBDef.EvalSubmitResult;

         string sql = @"if exists (select * from tempdb.dbo.sysobjects where id = object_id(N'tempdb..#temp1') and type='U')
                        drop table #temp1
                     if exists (select * from tempdb.dbo.sysobjects where id = object_id(N'tempdb..#temp2') and type='U')
                        drop table #temp2

                     select et.id as tableId, 
		                     er.TargetId,
		                     er.targetroleId,
		                     er.AccesserRoleId,
		                     ep.id as periodId,
		                     ep.name as periodName,
		                     u.UserName as targetName, 
		                     et.Name as tableName, 
		                     r2.Name as targetRoleName,
		                     eri.Score*(ei.Propertion/100) as score,
		                     eri.IndicationId as eriId
		                     into #temp1
		                     from [dbo].[EvalResultItem] eri
		                     inner join EvalResult er
		                     on er.id=eri.ResultId
		                     left join  [dbo].[EvalIndication] ei
		                     on ei.IndicationId=eri.IndicationId and ei.tableid=er.tableId and er.AccesserRoleId=ei.AccessorRoleId
		                     left join [role] r
		                     on r.id=ei.AccessorRoleId
		                     left join [role] r2
		                     on r2.id=er.targetRoleId
		                     left join EvalTable et
		                     on et.ID=er.TableId
		                     left join UserInfo u 
		                     on er.TargetId=u.id
		                     left join EvalPeriod ep
		                     on er.PeriodId=ep.id
							   
	                     select 
	                     AccesserRoleId as 'AccesserRoleId',
	                     targetroleId as 'targetroleId',
	                     targetId as 'targetId',
	                     periodId 'periodId',
	                     periodName 'periodName',
	                     targetName 'targetName',
	                     avg(score) as 'score',
	                     targetRoleName as 'targetRoleName',
	                     tableId as 'tableid'
	                     into #temp2
	                     from #temp1
	                     group by AccesserRoleId,targetroleId,periodId,periodName,targetName,targetRoleName,targetId,eriId,tableId
							   
	                     select
	                     t.PeriodId  as 'PeriodId',
	                     t.periodName as 'PeriodName',
	                     t.TargetId as 'TargetId',
	                     t.TargetName as 'TargetName',
	                     t.targetRoleId as 'TargetRoleId',
                        t.targetRoleName as 'TargetRoleName',
	                     round(sum(t.Score*(etgi.Propertion/100)),1) Score
	                     from #temp2 t 
	                     left join [dbo].[EvalTableGroupItem] etgi
	                     on t.Tableid=etgi.TableId
	                     join EvalTableGroup etg
	                     on  etgi.TableGroupId=etg.ID and etg.targetRoleId=t.targetRoleId and etg.PeriodId=@PeriodId
                        where  t.periodId = @PeriodId
	                     group by t.PeriodId,t.PeriodName, t.TargetId,t.TargetName,t.targetRoleId,t.targetRoleName";


         var models = DapperHelper.QueryBySQL<EvalReportModel>(sql, new { PeriodId = periodId });
         var submitItems = new List<EvalSubmitResult>();
         var newSubmitItems = new List<EvalSubmitResult>();

         if (models.Count > 0)
         {
            // TODO:暂时更新EvalSubmitResult 表 需求太恶心了！将在7月改掉
            submitItems = EvalSubmitResult.ConditionQuery(esr.PeriodId == periodId, null);
          
            foreach (var item in models)
            {
               EvalSubmitResult esri = submitItems.FirstOrDefault(x => x.UserId == item.TargetId && x.PeriodId == item.PeriodId && x.RoleId == item.TargetRoleId);
               if (esri != null)
                  esri.Score = item.Score;
               else
                  esri = new EvalSubmitResult {
                     SubmitResultId = Guid.NewGuid(),
                     PeriodId = item.PeriodId,
                     UserId = item.TargetId,
                     Score = item.Score,
                     RoleId =item.TargetRoleId,
                     UserName =item.TargetName,
                     RoleName =item.TargetRoleName,
                     PeriodName =item.PeriodName};

               newSubmitItems.Add(esri);
            }

            db.BeginTrans();

            try
            {
               db.EvalSubmitResultDal.ConditionDelete(esr.PeriodId == periodId);

               foreach (var item in newSubmitItems)
                  db.EvalSubmitResultDal.Insert(item);

               db.Commit();
            }
            catch
            {
               db.Rollback();
            }
         }

         var total = newSubmitItems.Count();

         var results = newSubmitItems
            .Skip((rowCount * current) - rowCount)
            .Take(rowCount);


         return Json(new
         {
            rows = results,
            current,
            rowCount,
            total
         });
      }


      // GET: Statistic/EvalResultDetails

      public ActionResult EvalResultDetails(EvalResultDetailsViewModel model)
      {
         var models = DapperHelper.QueryBySQL<EvalResultDetailsViewModel>(_detailsSQL, new { TargetId = model.TargetId, TargetRoleId = model.TargetRoleId, PeriodId = model.PeriodId, });
         if (models.Count <= 0) throw new ApplicationException();
         var tableIds = models.Select(x => x.TableId).Distinct();

         var dic = new Dictionary<Guid, List<EvalResultItem>>();
         var periodTables = EvalPeriodTable.GetAllEvalPeriodTables(db).FindAll(x => x.PeriodId == model.PeriodId);
         var resultTables = new List<EvalPeriodTable>();

         foreach (var item in tableIds)
         {
            dic.Add(item, new List<EvalResultItem>());
            var table = periodTables.Find(x => x.TableId == item);
            if (table != null)
               resultTables.Add(table);
         }

         foreach (var item in models)
         {
            if (dic.ContainsKey(item.TableId))
            {
               dic[item.TableId].Add(new EvalResultItem
               {
                  ResultId = item.EvalResultId,
                  TableId = item.TableId,
                  EvalIndication = new EvalIndication { IndicationName = item.IndicationName, FullScore = item.FullScore, IndicationDescription = item.IndicationDescription },
                  Score = item.Score,
                  PeriodId = item.PeriodId,
               });

               var table = resultTables.Find(x => x.TableId == item.TableId);
               if (table != null)
                  table.Score += item.Score;
            }

            model.PeriodName = item.PeriodName;
         }

         model.TableResultItems = dic;
         model.PeriodTables = resultTables;
         model.IsShowOthersEvalResult = true;

         return View(model);
      }


      private static string _detailsSQL = @"select 
                     s.IndicationName,
                     s.IndicationId,
                     s.TargetId,
                     s.PeriodId,
                     s.PeriodName,
                     s.TargetRoleId,
                     s.Description,
                     s.TableId,
                     s2.FullScore,
                     avg(s.Score) Score,
                     s2.Propertion into #temp
                     from (
	                         select  
	                         er.id as 'EvalResultId',
	                         i.Name as 'IndicationName',
	                         eri.Score as 'Score',
	                         er.TargetId as 'TargetId',
                            er.periodId as 'PeriodId',
	                         r.ID as accessorRoleId,
	                         et.Name as 'TableName',
	                         er.TargetRoleId as 'TargetRoleId',
	                         i.ID as 'IndicationId',
                             i.[Description] as 'Description',
	                         et.ID as 'TableId',
                            ep.Name as 'PeriodName'
	                         from 
	                         [dbo].[EvalResultItem] eri
	                         join EvalResult er
	                         on eri.ResultId=er.id
	                         join EvalPeriod ep
	                         on ep.id=er.PeriodId
	                         join EvalTable et on
	                         er.TableId=et.ID
	                         join [Role] r2
	                         on r2.ID=er.TargetRoleId
	                         join [role] r
	                         on r.id=er.AccesserRoleId
	                         join Indication i
	                         on eri.IndicationId=i.id
	                         where er.TargetId=convert(uniqueidentifier,@TargetId) and er.PeriodId=convert(uniqueidentifier,@PeriodId)
                         ) s
                         join 
                         (
	                         select ei.AccessorRoleId,ei.IndicationId,t.Name,t.ID as tableId,ei.FullScore,ei.Propertion
	                         from [dbo].[EvalIndication] ei
	                         join Indication i
	                         on i.ID=ei.IndicationId
	                         join EvalTable t 
	                         on t.ID=ei.TableId
                         ) s2
                         on s.accessorRoleId=s2.AccessorRoleId 
                         and s.indicationId=s2.IndicationId 
                         and s.tableid=s2.tableId
                         and s.TargetRoleId= convert(uniqueidentifier,@TargetRoleId)
	                     group by s.IndicationName,s.IndicationId,s.TargetId,s.PeriodId,s.PeriodName,s.TargetRoleId,s2.AccessorRoleId,s.Description,s.TableId,s2.FullScore,s2.Propertion

                     select 
                      s.IndicationName,
                      s.IndicationId,
                      s.TargetId,
                      s.PeriodId,   
                      s.PeriodName,
                      s.TargetRoleId,
                      s.Description,
                      s.TableId,
                      s.FullScore,
                      SUM(s.Score*(s.Propertion/100)) Score
                     from #temp s
                     group by s.IndicationName,s.IndicationId,s.TargetId,s.PeriodId,s.PeriodName,s.TargetRoleId,s.Description,s.TableId,s.FullScore";
   }

}

