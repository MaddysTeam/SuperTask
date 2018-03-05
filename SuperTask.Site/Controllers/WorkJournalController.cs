using Business;
using Business.Config;
using Business.Helper;
using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TheSite.Models;

namespace TheSite.Controllers
{

   public class WorkJournalController : BaseController
   {

      static APDBDef.WorkJournalTableDef wj = APDBDef.WorkJournal;
      static APDBDef.WorkTaskTableDef t = APDBDef.WorkTask;
      static APDBDef.AttachmentTableDef at = APDBDef.Attachment;
      static APDBDef.ProjectTableDef p = APDBDef.Project;
      static APDBDef.DictionaryTableDef d = APDBDef.Dictionary;

      // GET:  WorkJournal/Search
      // POST-Ajax:  WorkJournal/Search

      public ActionResult Search()
      {
         ViewBag.Projects = ProjectrHelper.UserJoinedProjects(GetUserInfo().UserId, db);

         return View();
      }

      [HttpPost]
      public ActionResult Search(int current, int rowCount, AjaxOrder sort,
         string searchPhrase, Guid searchType, Guid? projectId,
         string start, string end, Guid selectDateType)
      {
         ThrowNotAjax();


         var p = APDBDef.Project;

         var user = GetUserInfo();

         var query = APQuery.select(wj.Asterisk, wj.JournalId.As("JournalId"), wj.ServiceCount,
                                t.IsParent, t.StartDate, t.EndDate, t.TaskId.As("TaskId"), t.SubTypeId,
                                t.TaskName.As("TaskName"), t.EstimateWorkHours.As("EsHours"), t.TaskStatus.As("tkStatus"), t.TaskType,
                                p.ProjectId.As("ProjectId"), p.ProjectName.As("ProjectName"),
                                at.AttachmentId.As("AttachmentId"), at.Url, at.RealName,
                                d.Title, d.Other, d.Note
                                );

         HandleManager.JournalSearchHandlers[searchType].Handle(query,
            new WorkJournalSearchOption
            {
               db = db,
               User = user,
               Start = DateTime.Parse(start),
               End = DateTime.Parse(end),
               SelectDateType = selectDateType,

               wj = wj,
               t = t,
               at = at,
               p = p,
               d = d
            });

         if (projectId != null && !projectId.Value.IsEmpty() && projectId.Value != ProjectKeys.SelectAll)
         {
            query.where_and(wj.Projectid == projectId);
         }

         query.where_and(t.IsParent == false);

         query.primary(wj.JournalId)
              .skip((current - 1) * rowCount)
              .take(rowCount)
              .order_by(wj.RecordDate.Desc);


         if (searchPhrase != "")
         {
            query.where_and(t.TaskName.Match(searchPhrase) |
                            p.ProjectName.Match(searchPhrase) |
                            wj.Comment.Match(searchPhrase));
         }


         //排序条件表达式

         if (sort != null)
         {
            switch (sort.ID)
            {
               case "projectName": query.order_by(sort.OrderBy(p.ProjectName)); break;
               case "taskName": query.order_by(sort.OrderBy(t.TaskName)); break;
               case "start": query.order_by(sort.OrderBy(t.StartDate)); break;
               case "end": query.order_by(sort.OrderBy(t.EndDate)); break;
               case "workhours": query.order_by(sort.OrderBy(wj.WorkHours)); break;
               case "progress": query.order_by(sort.OrderBy(wj.Progress)); break;
               case "recordDate": query.order_by(sort.OrderBy(wj.RecordDate)); break;
               case "estimateHours": query.order_by(sort.OrderBy(t.EstimateWorkHours)); break;
            }
         }


         var total = db.ExecuteSizeOfSelect(query);


         var result = query.query(db, rd =>
                 {
                    var workhours = wj.WorkHours.GetValue(rd);
                    var recordDate = wj.RecordDate.GetValue(rd);
                    var taskTypeId = t.TaskType.GetValue(rd);
                    var taskType = DictionaryHelper.GetDicById(TaskKeys.TypeGuid, taskTypeId);
                    var subTypeTitle = taskTypeId == TaskKeys.ProjectTaskType || taskTypeId == TaskKeys.TempTaskType ?
                                                   "无" :
                                                   $"{d.Title.GetValue(rd)} [{d.Note.GetValue(rd)}]";

                    return new
                    {
                       id = wj.JournalId.GetValue(rd, "JournalId"),
                       userId = GetUserInfo().UserId,
                       projectName = p.ProjectName.GetValue(rd, "ProjectName"),
                       projectId = p.ProjectId.GetValue(rd, "ProjectId"),
                       taskId = t.TaskId.GetValue(rd, "TaskId"),
                       taskName = t.TaskName.GetValue(rd, "TaskName"),
                       taskStatus = t.TaskStatus.GetValue(rd, "tkStatus"),
                       estimateHours = t.EstimateWorkHours.GetValue(rd, "EsHours"),
                       progress = wj.Progress.GetValue(rd) + "%",
                       workhours = workhours,
                       comment = wj.Comment.GetValue(rd).Ellipsis(ThisApp.ContentDisplayLength),
                       recordDate = wj.RecordDate.GetValue(rd),
                       start = t.StartDate.GetValue(rd),
                       end = t.EndDate.GetValue(rd),
                       createDate = wj.CreateDate.GetValue(rd),
                       isParent = t.IsParent.GetValue(rd),
                       status = wj.Status.GetValue(rd),
                       hasAttachment = !at.AttachmentId.GetValue(rd, "AttachmentId").IsEmpty()
                                       && !string.IsNullOrEmpty(at.Url.GetValue(rd)),
                       attachmentUrl = at.Url.GetValue(rd),
                       recordType = wj.RecordType.GetValue(rd),
                       taskTypeName = taskType == null ? string.Empty : taskType.Title,
                       serviceCount = wj.ServiceCount.GetValue(rd),//for maintained task only
                       subTypeTitle = subTypeTitle,
                       subTypeValue = wj.TaskSubTypeValue.GetValue(rd),
                       subTypeId = t.SubTypeId.GetValue(rd)
                    };
                 }).ToList();


         return Json(new
         {
            rows = result,
            current,
            rowCount,
            total,
         });
      }


      // GET:  WorkJournal/Edit
      // POST-Ajax:  WorkJournal/Edit

      public ActionResult Edit(Guid id)
      {
         var currentUser = GetUserInfo();

         var workJournal = APQuery.select(wj.WorkHours, wj.Progress, wj.Comment, wj.Projectid, wj.TaskId, wj.UserId, wj.RecordDate, wj.RecordType, wj.ServiceCount,
                                          wj.CreateDate, wj.TaskSubTypeValue, t.WorkHours.As("TaskWorkedHours"), t.TaskType, t.SubTypeValue,
                                          p.ProjectName.As("ProjectName"), t.TaskName.As("TaskName"), t.EstimateWorkHours,
                                          at.AttachmentId.As("AttachmentId"), at.Url, at.RealName.As("RealName"),
                                          d.ID.As("SubTypeID"), d.Title, d.Other, d.Note, d.Code)
                        .from(wj,
                              p.JoinLeft(p.ProjectId == wj.Projectid),
                              t.JoinInner(t.TaskId == wj.TaskId),
                              d.JoinLeft(d.ID == t.SubTypeId),
                              at.JoinLeft(at.AttachmentId == wj.AttachmentId)
                              )
                        .where(wj.UserId == currentUser.UserId & wj.JournalId == id)
                        .query(db, r =>
                        {
                           var taskTypeId = t.TaskType.GetValue(r);
                           var subTypeTitle = taskTypeId == TaskKeys.ProjectTaskType || taskTypeId == TaskKeys.TempTaskType ?
                                                          "无" :
                                                           $"{d.Title.GetValue(r)} (单位:{d.Other.GetValue(r)})";
         
                           var subTaskScore = t.SubTypeValue.GetValue(r) * Convert.ToDouble(d.Code.GetValue(r));

                           return new WorkJournal
                           {
                              JournalId = id,
                              UserId = wj.UserId.GetValue(r),
                              Projectid = wj.Projectid.GetValue(r),
                              TaskId = wj.TaskId.GetValue(r),
                              RecordDate = wj.RecordDate.GetValue(r),
                              RecordType = wj.RecordType.GetValue(r),
                              TaskName = t.TaskName.GetValue(r, "TaskName"),
                              ProjectName = p.ProjectName.GetValue(r, "ProjectName"),
                              Progress = wj.Progress.GetValue(r),
                              TaskEstimateWorkHours = t.EstimateWorkHours.GetValue(r),
                              WorkHours = wj.WorkHours.GetValue(r),
                              Comment = wj.Comment.GetValue(r),
                              CreateDate = wj.CreateDate.GetValue(r),
                              AttachmentId = at.AttachmentId.GetValue(r, "AttachmentId"),
                              Attachment = new Attachment
                              {
                                 Url = at.Url.GetValue(r),
                                 AttachmentId = at.AttachmentId.GetValue(r, "AttachmentId"),
                                 RealName = at.RealName.GetValue(r)
                              },
                              TaskWorkHours = t.WorkHours.GetValue(r, "TaskWorkedHours"),
                              TaskType = taskTypeId,
                              ServiceCount = t.ServiceCount.GetValue(r),//当日运维数量 ,停用，已被子类型取代
                              TaskSubType = t.SubTypeId.GetValue(r),
                              TaskSubTypeValue = wj.TaskSubTypeValue.GetValue(r),
                              SubTypeTitle = subTypeTitle,
                              SubTaskScore = subTaskScore
                           };
                        }).FirstOrDefault();


         var yesterdayJournal = db.WorkJournalDal.ConditionQuery(
            wj.UserId == currentUser.UserId &
            wj.TaskId == workJournal.TaskId
            & wj.RecordDate > workJournal.RecordDate.AddDays(-1).TodayStart()
            & wj.RecordDate < workJournal.RecordDate.AddDays(-1).TodayEnd()
            , null, null, null).FirstOrDefault();

         ViewBag.YesterdayProgress = yesterdayJournal == null ? 0 : yesterdayJournal.Progress;

         return PartialView(workJournal);
      }

      [HttpPost]
      public ActionResult Edit(WorkJournal journal)
      {
         ThrowNotAjax();

         var tks = db.WorkTaskDal.ConditionQuery(t.Projectid == journal.Projectid, null, null, null);
         var journals = db.WorkJournalDal.ConditionQuery(wj.Projectid == journal.Projectid, null, null, null);
         var originalTasks = tks.DeepClone();
         var editOption = new WorkJournalEditOption { db = db, Journals = journals, Tasks = tks, Result = Result.Initial(), OriginalTasks = originalTasks };

         HandleManager.JournalEditHandlers[journal.TaskType].Handle(journal, editOption);

         return Json(new
         {
            result = editOption.Result.IsSuccess ? AjaxResults.Success : AjaxResults.Error,
            msg = editOption.Result.Msg
         });
      }

   }

}
