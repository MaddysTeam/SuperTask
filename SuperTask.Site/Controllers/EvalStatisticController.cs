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

         string sql = @"  if exists (select * from tempdb.dbo.sysobjects where id = object_id(N'tempdb..#temp1') and type='U')
                        drop table #temp1
			
                     select  
					        distinct
					         er.PeriodId as periodId,
					         er.TableId as tableId, 
		                     er.TargetId,
							 er.Score,
							 --sum(er.Score * (eat.Propertion/100) * (ettp.propertion/100)) score,
		                     ep.name as periodName,
		                     u.UserName as targetName, 
		                     et.Name as tableName,
							 eat.Propertion as propertion,
							 ettp.Propertion  as tablepropertion
		                     into #temp1
		                     from 
							 EvalResult er
		                     left join EvalTable et
		                     on et.ID=er.TableId
		                     left join UserInfo u 
		                     on er.TargetId=u.id
		                     left join EvalPeriod ep
		                     on er.PeriodId=ep.id
							 left join EvalAccessorTarget eat
							 on eat.TargetId=er.TargetId and eat.TableId = er.TableId and eat.PeriodId= er.PeriodId 
							 left join EvalTargetTablePropertion ettp 
							 on ettp.TargetId=er.TargetId and ettp.TableId=er.TableId and ettp.PeriodId= er.PeriodId
							 where eat.TableId<> @EmptyTableId and er.PeriodId=@PeriodId and eat.Propertion>0
							   
							group by er.TableId,er.TargetId,er.PeriodId,ep.Name,u.UserName,et.Name,er.Score,eat.Propertion,ettp.Propertion

							select 
							periodId,
							TargetId,
							periodName,
							targetName,
							sum(score * (propertion/100)* (tablepropertion/100)) score
							from #temp1
						   group by periodId,TargetId,periodName,targetName";


         var models = DapperHelper.QueryBySQL<EvalReportModel>(sql, new { EmptyTableId = Guid.Empty, PeriodId = periodId });
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
                  esri = new EvalSubmitResult
                  {
                     SubmitResultId = Guid.NewGuid(),
                     PeriodId = item.PeriodId,
                     UserId = item.TargetId,
                     Score = item.Score,
                     UserName = item.TargetName,
                     PeriodName = item.PeriodName
                  };

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
         var models = DapperHelper.QueryBySQL<EvalResultDetailsViewModel>(_detailsSQL, new { TargetId = model.TargetId, PeriodId = model.PeriodId, });
         if (models.Count <= 0) throw new ApplicationException();

         var allTables = EvalTable.GetAll();
         var resultTables = new List<EvalTable>();
         var dic = new Dictionary<Guid, List<EvalResultItem>>();
         var tableIds = models.Select(x => x.TableId).Distinct();

         foreach (var item in tableIds)
         {
            dic.Add(item, new List<EvalResultItem>());
            var table = allTables.Find(x => x.TableId == item);
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
                  EvalIndication = new EvalIndication { IndicationName = item.IndicationName, FullScore = item.FullScore},
                  Score = item.Score,
                  PeriodId = item.PeriodId,
               });

               //var table = resultTables.Find(x => x.TableId == item.TableId);
               //if (table != null)
               //   table.Score += item.Score;
            }

            model.PeriodName = item.PeriodName;
         }

         model.TableResultItems = dic;
         model.PeriodTables = resultTables;
         model.IsShowOthersEvalResult = true;

         return View(model);

      }


      private static string _detailsSQL = @"if exists (select * from tempdb.dbo.sysobjects where id = object_id(N'tempdb..#temp2') and type='U')
                        drop table #temp2

         
					   select distinct
	                 
	                         i.Name as 'IndicationName',
							       eri.Score,
						          er.AccesserId,
	                         er.TargetId as 'TargetId',
                            er.periodId as 'PeriodId',
	                         et.Name as 'TableName',
	                         i.ID as 'IndicationId',
	                         et.ID as 'TableId',
                            ep.Name as 'PeriodName',
							eat.Propertion as propertion,
							ettp.Propertion as tablepropertion
							 into #temp2
	                         from 
	                         [dbo].[EvalResultItem] eri
	                         join EvalResult er
	                         on eri.ResultId=er.id
	                         join EvalPeriod ep
	                         on ep.id=er.PeriodId
	                         join EvalTable et on
	                         er.TableId=et.ID
	                         join Indication i
	                         on eri.IndicationId=i.id
							 join EvalAccessorTarget eat
							  on eat.TargetId=er.TargetId and eat.TableId = er.TableId and eat.PeriodId= er.PeriodId 
							 left join EvalTargetTablePropertion ettp 
							 on ettp.TargetId=er.TargetId and ettp.TableId=er.TableId and ettp.PeriodId= er.PeriodId
	                         where er.TargetId=@TargetId and er.PeriodId=@PeriodId and eat.Propertion >0 and ettp.Propertion>0
							 group by i.Name,er.AccesserId,er.TargetId,er.PeriodId,et.Name,i.ID,et.ID,ep.Name,eri.Score,eat.Propertion,ettp.Propertion
							 

							select 
						   t.IndicationName,
							sum(Score * (propertion/100) * (tablepropertion/100)) 'Score',
							t.IndicationId,
							t.PeriodId,
							t.TargetId,
							t.TableId,
							t.PeriodName
							from #temp2 t
						    group by periodId,TargetId,IndicationName,IndicationId,TableId,PeriodName";
   }

}

