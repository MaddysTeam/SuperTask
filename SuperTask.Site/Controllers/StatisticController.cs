using Business.Helper;
using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TheSite.Models;

namespace TheSite.Controllers
{

   public class StatisticController : BaseController
   {

      // GET: Statistic/PersonalReport
      // POST-Ajax: Statistic/PersonalReport

      //[Permission]
      public ActionResult PersonalReport()
      {
         return View();
      }

      [HttpPost]
      public ActionResult PersonalReport(int current, int rowCount, AjaxOrder sort, string start, string end, string searchPhrase)
      {
         var sql = $@"if exists (select * from tempdb.dbo.sysobjects where id = object_id(N'tempdb..#temp') and type='U')
                     drop table #temp

                  if exists (select * from tempdb.dbo.sysobjects where id = object_id(N'tempdb..#temp2') and type='U')
                     drop table #temp2

                  if exists (select * from tempdb.dbo.sysobjects where id = object_id(N'tempdb..#temp3') and type='U')
                     drop table #temp3
  
                    SELECT  [ID]
                        ,[Projectid]
                        ,[TaskId]
                        ,[WorkHours]
                        ,[RecordDate]
                        ,[CreateDate]
                        ,[ModifyDate]
                        ,[Comment]
                        ,[UserId]
                        ,[progress]
                        ,[Status]
                        ,[AttachmentId]
                        ,[RecordType]
	                    into #temp
                    FROM [SuperTask].[dbo].[WorkJournal] 
                    where RecordDate>=@StartDate and RecordDate <= @EndDate 
                    and ModifyDate is not null
                    and RecordType='{JournalKeys.ManuRecordType}'
                   -- and workhours>0
  
                    select * into #temp2 from worktask t4
                    where t4.id in (select taskId from #temp)

                    select j.workhours,j.userId,j.projectId,t.[Type] as taskType into #temp3 from #temp j 
                    left join worktask t
                    on t.id=j.taskId
                    where t.[Status]!='{TaskKeys.DeleteStatus}'
                    and j.RecordType='{JournalKeys.ManuRecordType}'


                    select u.id as UserId,
                       u.UserName,
	                    p.ID as ProjectId,
	                    isnull(p.Name,N'【临时任务】') as projectName,
	                    (select count(1) from #temp2 t4 where t4.ManagerId=u.id and t4.projectId=p.id)  as totalTaskCount, 
	                    (select count(1) from #temp2 t4 where t4.[status]='{TaskKeys.CompleteStatus}' and t4.ManagerId=u.id and t4.projectId=p.id )  as completeTaskCount, 
	                    (select count(1) from #temp2 t4 where t4.[status]='{TaskKeys.ProcessStatus}' and t4.ManagerId=u.id and t4.projectId=p.id)  as processCount, 
	                    (select count(1) from #temp2 t4 where t4.[status]='{TaskKeys.PlanStatus}' and t4.ManagerId=u.id and t4.projectId=p.id)  as planCount, 
	                    (select count(1) from #temp2 t4 where t4.[status]='{TaskKeys.DeleteStatus}' and t4.ManagerId=u.id and t4.projectId=p.id)  as delCount, 
	                    (select count(1) from #temp2 t4 where t4.[status]='{TaskKeys.ReviewStatus}' and t4.ManagerId=u.id and t4.projectId=p.id)  as reviewCount, 
	                    (select sum(t5.workhours) from #temp3 t5 where t5.projectId=p.id) as projectHours,
	                    (select sum(t5.workhours) from #temp3 t5 where t5.UserId=u.ID and t5.projectId=p.id ) as workHours,
	                     sum(t.EstimateWorkHours) estimateWorkHours,
	                    (select sum(case when re.[Type]='{ReviewKeys.TaskRequestFailed}' then 1 else 0 end )*1.0/count(1) as rr from review re where u.ID=re.senderid and re.ProjectId=p.id)*100 as returnRatio
                    from UserInfo u
                    left join WorkTask t
                    on t.managerId=u.id
                    left join project p 
                    on p.ID=t.ProjectId
                    join Users ac 
                    on ac.id=u.id
                    where 
                    u.UserName not like N'%【临】%'
                    and t.[Status] != '{TaskKeys.DeleteStatus}'
                    and t.projectId is not null 
                    and p.id is not null
                    and ac.status=0
                    group by u.UserName, p.ID,p.name,u.id

                    union all

                      select 
                       u.id as UserId,
                       u.UserName,
	                    p.ID as ProjectId,
	                    isnull(p.Name,N'【临时任务】') as projectName,
	                    (select count(1) from #temp2 t4 where t4.ManagerId=u.id and t4.Type='{TaskKeys.TempTaskType}')  as totalTaskCount, 
	                    (select count(1) from #temp2 t4 where t4.[status]='{TaskKeys.CompleteStatus}' and t4.ManagerId=u.id and t4.Type='{TaskKeys.TempTaskType}' )  as completeTaskCount, 
	                    (select count(1) from #temp2 t4 where t4.[status]='{TaskKeys.ProcessStatus}' and t4.ManagerId=u.id and t4.Type='{TaskKeys.TempTaskType}')  as processCount, 
	                    (select count(1) from #temp2 t4 where t4.[status]='{TaskKeys.PlanStatus}' and t4.ManagerId=u.id and t4.Type='{TaskKeys.TempTaskType}')  as planCount, 
	                    (select count(1) from #temp2 t4 where t4.[status]='{TaskKeys.DeleteStatus}' and t4.ManagerId=u.id and t4.Type='{TaskKeys.TempTaskType}')  as delCount, 
	                    (select count(1) from #temp2 t4 where t4.[status]='{TaskKeys.ReviewStatus}' and t4.ManagerId=u.id and t4.Type='{TaskKeys.TempTaskType}')  as reviewCount, 
	                     null,
	                    (select sum(t5.workhours) from #temp3 t5 where t5.UserId=u.ID and t5.taskType='{TaskKeys.TempTaskType}') as workHours,
	                     sum(t.EstimateWorkHours) estimateWorkHours,
	                    (select sum(case when re.[Type]='{ReviewKeys.TaskRequestFailed}' then 1 else 0 end )*1.0/count(1) as rr from review re where u.ID=re.senderid and re.ProjectId='00000000-0000-0000-0000-000000000000' )*100 as returnRatio
                    from UserInfo u
                    left join WorkTask t
                    on t.managerId=u.id
                    left join project p 
                    on p.ID=t.ProjectId
                    join Users ac 
                    on ac.id=u.id
                    where 
                     u.UserName  not like N'%【临】%'
                    -- t.projectId is  null and  p.id is  null
                    -- and u.username=N'郑华超'
                     and t.type='{TaskKeys.TempTaskType}'
                     and ac.status=0
                    group by u.UserName, p.ID,p.name,u.id";

         var result = DapperHelper.QueryBySQL<PersonalReportModel>(sql, new { StartDate = start, EndDate = end });
         if (!string.IsNullOrEmpty(searchPhrase) && result.Count > 0)
         {
            result = result.FindAll(r => r.UserName.IndexOf(searchPhrase) >= 0 || r.ProjectName.IndexOf(searchPhrase) >= 0);
         }

         if (sort != null && result.Count > 0)
            switch (sort.ID)
            {
               case "ProjectName": result = result.OrderBy(r => r.ProjectName).ToList(); break;
               case "UserName": result = result.OrderBy(r => r.UserName).ToList(); break;
            }


         var total = result.Count;

         if (result != null && result.Count > 0)
            result = result.Skip((current - 1) * rowCount).Take(rowCount).ToList();

         //添加总计数据
         result.Add(new PersonalReportModel
         {
            IsTotal = true,
            ProjectName = "总计",
            TotalTaskCount = result.Sum(t => t.TotalTaskCount),
            CompleteTaskCount = result.Sum(t => t.CompleteTaskCount),
            PlanCount = result.Sum(t => t.PlanCount),
            ProcessCount = result.Sum(t => t.ProcessCount),
            ReviewCount = result.Sum(t => t.ReviewCount),
            DelCount = result.Sum(t => t.DelCount),
            WorkHours = result.Sum(t => t.WorkHours)
         });

         return Json(new
         {
            rows = result,
            current,
            rowCount,
            total
         });

      }


      // GET: Statistic/PersonalScoreReport
      // POST-Ajax: Statistic/PersonalScoreReport

      public ActionResult PersonalScoreReport()
      {
         return View();
      }

      [HttpPost]
      public ActionResult PersonalScoreReport(int current, int rowCount, AjaxOrder sort, string start, string end, string searchPhrase)
      {
         var sql = @"select u.id as UserId, u.UserName,t.ID,wj.TaskSubTypeValue as SubValue,d.Code from WorkJournal wj
                     inner join WorkTask t 
                     on t.id=wj.TaskId
                     inner join UserInfo u
                     on u.id=t.ManagerId
                     left join Dictionary d 
                     on d.ID=t.SubTypeId
                     where TaskSubTypeValue > 0
                     and wj.RecordDate>@StartDate and RecordDate<@EndDate
                     and t.IsParent=0 ";

         var scores = DapperHelper.QueryBySQL<PersonalScore>(sql, new { StartDate = start, EndDate = end });
         var result = new List<PersonalScoreViewModel>();
         var total = 0;
         if (scores.Count > 0)
         {
            var scoreViewModels = scores.GroupBy(x => new { x.UserId, x.UserName })
                                                    .Select(x => new PersonalScoreViewModel
                                                    {
                                                       UserId=x.Key.UserId.ToString(),
                                                       Score = x.Sum(y => y.SubValue * y.Code).ToString("0.00"),
                                                       UserName = x.Key.UserName
                                                    });
            total = scoreViewModels.Count();
            if (total > 0)
               result = scoreViewModels.Skip(rowCount*(current-1)).Take(rowCount).ToList();
         }

         return Json(new
         {
            rows = result,
            current,
            rowCount,
            total
         });
      }


      // GET: Statistic/PersonalScoreDetails
      // POST-Ajax: Statistic/PersonalScoreDetails

      public ActionResult PersonalScoreDetails(Guid userId)
      {
         return View(userId);
      }

      [HttpPost]
      public ActionResult PersonalScoreDetails(int current, int rowCount, AjaxOrder sort,Guid userId, string start, string end, string searchPhrase)
      {
         var total = 0;
         var result = new List<PersonalScoreViewModel>();
         var sql = @"select 
                            u.UserName,
                            t.name as TaskName,
                            wj.TaskSubTypeValue as SubValue,
                            d.Code,
                            p.id as ProjectId,
                            t.id as TaskId,
                            p.Name as ProjectName,
                            d.Title as SubType 
                     from WorkJournal wj
                     inner join WorkTask t 
                     on t.id=wj.TaskId
                     inner join UserInfo u
                     on u.id=t.ManagerId
                     inner join Project p
                     on p.id=t.ProjectId
                     left join Dictionary d 
                     on d.ID=t.SubTypeId
                     where TaskSubTypeValue > 0
                     and wj.RecordDate>@StartDate and RecordDate<@EndDate
                     and t.IsParent=0
                     and t.managerId=@ManagerId";

         var scores = DapperHelper.QueryBySQL<PersonalScore>(sql, new { ManagerId=userId, StartDate = start, EndDate = end });
         if (scores.Count > 0)
         {
            var scoreViewModels = scores.GroupBy(x => new { x.UserId, x.UserName, x.ProjectId,x.TaskId, x.TaskName, x.ProjectName, x.Code, x.SubType })
                                                    .Select(x => new PersonalScoreViewModel
                                                    {
                                                       UserId= userId.ToString(),
                                                       Score = x.Sum(y => y.SubValue * y.Code).ToString("0.00"),
                                                       SubValue = x.Sum(y => y.SubValue).ToString(),
                                                       SubType = x.Key.SubType,
                                                       UnitScore =x.Key.Code.ToString(),
                                                       UserName = x.Key.UserName,
                                                       TaskName=x.Key.TaskName,
                                                       ProjectId=x.Key.ProjectId.ToString(),
                                                       ProjectName = x.Key.ProjectName,
                                                       TaskId=x.Key.TaskId.ToString()
                                                    });
            total = scoreViewModels.Count();
            if (total > 0)
               result = scoreViewModels.Skip(rowCount * (current - 1)).Take(rowCount).ToList();
         }

         return Json(new
         {
            rows = result,
            current,
            rowCount,
            total
         });
      }


      public ActionResult PersonalScoreExport(string start, string end, string searchPhrase)
      {
         var result = new List<PersonalScoreViewModel>();
         var sql = @"select 
                            u.UserName,
                            t.name as TaskName,
                            wj.TaskSubTypeValue as SubValue,
                            d.Code,
                            p.id as ProjectId,
                            t.id as TaskId,
                            p.Name as ProjectName,
                            d.Title as SubType 
                     from WorkJournal wj
                     inner join WorkTask t 
                     on t.id=wj.TaskId
                     inner join UserInfo u
                     on u.id=t.ManagerId
                     inner join Project p
                     on p.id=t.ProjectId
                     left join Dictionary d 
                     on d.ID=t.SubTypeId
                     where TaskSubTypeValue > 0
                     and wj.RecordDate>@StartDate and RecordDate<@EndDate
                     and t.IsParent=0";

         var scores = DapperHelper.QueryBySQL<PersonalScore>(sql, new {StartDate = start, EndDate = end });
         if (scores.Count > 0)
         {
            var scoreViewModels = scores.GroupBy(x => new { x.UserId, x.UserName, x.ProjectId, x.TaskId, x.TaskName, x.ProjectName, x.Code, x.SubType })
                                                    .Select(x => new PersonalScoreExportViewModel
                                                    {
                                                       Score = x.Sum(y => y.SubValue * y.Code).ToString("0.00"),
                                                       SubValue = x.Sum(y => y.SubValue).ToString(),
                                                       SubType = x.Key.SubType,
                                                       UnitScore = x.Key.Code.ToString(),
                                                       UserName = x.Key.UserName,
                                                       TaskName = x.Key.TaskName,
                                                       ProjectName = x.Key.ProjectName,
                                                    }).ToList();

            var book = ExportHelper.ExportToExcel(scoreViewModels,new string[] {"用户名","项目名称", "任务名称","任务子类型","任务子类型分值","工作数量","得分" });
            var ms = new System.IO.MemoryStream();
            book.Write(ms);
            ms.Seek(0, System.IO.SeekOrigin.Begin);

            return File(ms, "application/vnd.ms-excel", $"本周期得分统计{start}-{end}.xls");
         }

         return Content("没有数据导出");
      }
   }

}