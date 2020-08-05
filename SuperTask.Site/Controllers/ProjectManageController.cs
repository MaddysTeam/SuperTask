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

   public class ProjectManageController : BaseController
   {

      static APDBDef.ProjectTableDef p = APDBDef.Project;
      static APDBDef.ResourceTableDef r = APDBDef.Resource;

      public ActionResult Resource(Guid projectId)
      {
         if (projectId == null)
            throw new ArgumentException(Errors.Project.NOT_ALLOWED_ID_NULL);

         var me = db.ResourceDal
           .ConditionQuery(r.Projectid == projectId & r.UserId == GetUserInfo().UserId, null, null, null).FirstOrDefault();

         if (me == null) throw new ApplicationException();

         ViewBag.CurrentResource = me;

         return View();
      }


      // GET: ProjectManage/ResourceTasksInfo
      // Post-Ajax: ProjectManage/ResourceTasksInfo

      public ActionResult ResourceTasksInfo(Guid projectId)
      {
         if (projectId.IsEmpty())
            throw new ArgumentException(Errors.Project.NOT_ALLOWED_ID_NULL);

         var u = APDBDef.UserInfo;
         var subquery = APQuery.select(r.UserId).from(r).where(r.Projectid == projectId);
         var users = db.UserInfoDal.ConditionQuery(u.UserId.In(subquery), null, null, null);

         return View(users);
      }

      [HttpPost]
      public ActionResult ResourceTasksInfo(Guid projectId, Guid userId)
      {
         var t = APDBDef.WorkTask;
         var u = APDBDef.UserInfo;

         var subquery = APQuery.select(r.UserId).from(r).where(r.Projectid == projectId);

         var query = APQuery.select(t.TaskId.As("TaskId"), t.TaskName.As("TaskName"), t.StartDate, t.EndDate, t.TaskStatus, t.TaskType,
                                    p.ProjectName.As("ProjectName"), u.UserName.As("UserName"))
                            .from(t,
                                  p.JoinInner(t.Projectid == p.ProjectId),
                                  u.JoinInner(t.ManagerId == u.UserId)
                                  )
                            .where(t.ManagerId.In(subquery) & u.UserId != ResourceKeys.TempBossId
                            & t.EndDate >= DateTime.Now.MonthStart() & t.StartDate <= DateTime.Now.MonthEnd()
                            ).take(5);//TODO:先暂时把部门老大不算入资源


         if (!userId.IsEmpty())
            query.where_and(u.UserId == userId);

         //查询结果集

         var result = query.query(db, rd =>
         {
            var projectName = p.ProjectName.GetValue(rd, "ProjectName");
            var taskName = t.TaskName.GetValue(rd, "TaskName");
            return new
            {
               title = $"项目:{projectName} 任务:{taskName}",
               start = t.StartDate.GetValue(rd).ToString("yyyy-MM-dd"),
               end = t.EndDate.GetValue(rd).ToString("yyyy-MM-dd"),
            };
         }).ToList();


         return Json(new
         {
            rows = result,
         });

      }


      // GET: ProjectManage/TaskArrangement

      public ActionResult TaskArrangement(Guid projectId)
      {
         //TODO:该方法比较特殊，因为前端引用了甘特图插件，所以暂时不用viewModel,用Viewbag 替换

         ViewBag.ResourceList = db.ResourceDal.ConditionQuery(r.Projectid == projectId, null, null, null);

         ViewBag.myTasks = TaskHelper.GetProjectUserTasks(projectId, GetUserInfo().UserId, db);

         ViewBag.project = ProjectHelper.GetCurrentProject(projectId, null, true);

         return View();
      }


      // GET: ProjectManage/Files
      // Post-Ajax: ProjectManage/Files

      public ActionResult Files(Guid? projectId)
      {
         var me = db.ResourceDal
          .ConditionQuery(r.Projectid == projectId & r.UserId == GetUserInfo().UserId, null, null, null).FirstOrDefault();

         if (me == null) throw new ApplicationException();

         ViewBag.CurrentResource = me;

         return View();
      }


      [HttpPost]
      public ActionResult Files(Guid projectId, Guid taskFileTypeId, int current, int rowCount, AjaxOrder sort, string searchPhrase)
      {
         ThrowNotAjax();

         var a = APDBDef.Attachment;
         var u = APDBDef.UserInfo;
         var p = APDBDef.Project;
         var t = APDBDef.WorkTask;


         var query = APQuery.select(a.Asterisk, u.UserName, p.ProjectName, t.TaskName.As("TaskName"))
            .from(a,
            t.JoinInner(t.TaskId == a.ItemId),
            u.JoinInner(a.PublishUserId == u.UserId),
            p.JoinInner(p.ProjectId == a.Projectid)
            );

         if (!projectId.IsEmpty())
            query.where(a.Projectid == projectId);

         if (taskFileTypeId != TaskKeys.DefaultFileType)
            query.where(t.TaskFileType == taskFileTypeId);

         query.primary(a.AttachmentId)
            .order_by(a.UploadDate.Desc)
            .skip((current - 1) * rowCount)
            .take(rowCount);


         //过滤条件
         //模糊搜索用户名、实名进行

         searchPhrase = searchPhrase.Trim();
         if (searchPhrase != "")
         {
            query.where_and(a.RealName.Match(searchPhrase) |
               p.ProjectName.Match(searchPhrase) |
               u.UserName.Match(searchPhrase)
               );
         }


         //排序条件表达式

         if (sort != null)
         {
            switch (sort.ID)
            {
               case "projectName": query.order_by(sort.OrderBy(p.ProjectName)); break;
               case "filePath": query.order_by(sort.OrderBy(a.Url)); break;
               case "fileName": query.order_by(sort.OrderBy(a.RealName)); break;
               case "taskName": query.order_by(sort.OrderBy(t.TaskName)); break;
               case "uploadUser": query.order_by(sort.OrderBy(u.UserName)); break;
               case "uploadDate": query.order_by(sort.OrderBy(a.UploadDate)); break;
            }
         }


         //获得查询的总数量

         var total = db.ExecuteSizeOfSelect(query);


         //查询结果集

         var result = query.query(db, r =>
         {
            return new
            {
               id = a.AttachmentId.GetValue(r),
               uploadUser = u.UserName.GetValue(r),
               fileName = a.RealName.GetValue(r),
               filePath = a.Url.GetValue(r),
               uploadDate = a.UploadDate.GetValue(r),
               projectName = p.ProjectName.GetValue(r),
               taskName = t.TaskName.GetValue(r, "TaskName")
            };
         });

         return Json(new
         {
            rows = result,
            current,
            rowCount,
            total
         });
      }


      // Post-Ajax: ProjectManage/DeleteFile

      [HttpPost]
      public ActionResult DeleteFile(Guid fileId)
      {
         if (!fileId.IsEmpty())
         {
            db.AttachmentDal.PrimaryDelete(fileId);
         }


         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.File.DELETE_SUCCESS
         });
      }


      // GET: ProjectManage/ProcessInfo TODO:will change later

      public ActionResult ProcessInfo(TaskSearchOption option)
      {
         ViewBag.SubMenu = MenuHelper.GetProjectMenuItems(option.ProjectId, GetUserInfo().UserId, MenuHelper.ProjectProcessPageCode);

         return View(option);
      }

      [HttpPost]
      public ActionResult ProcessInfo(int current, int rowCount, AjaxOrder sort, string searchPhrase, string searchType, string start, string end, string taskId)
      {
         ThrowNotAjax();

         var wj = APDBDef.WorkJournal;
         var p = APDBDef.Project;
         var t = APDBDef.WorkTask;
         var at = APDBDef.Attachment;
         var userId = this.GetUserInfo().UserId;


         var query = APQuery.select(wj.JournalId, wj.Comment, wj.RecordDate, wj.Progress, wj.WorkHours, wj.RecordType, wj.TaskSubTypeValue,
            wj.ServiceCount, p.ProjectId.As("ProjectId"), p.ProjectName.As("ProjectName"),
            t.TaskId.As("TaskId"), t.TaskName.As("TaskName"),
            t.RateOfProgress, t.WorkHours, t.StartDate, t.EndDate, t.TaskType,
            at.Url, at.AttachmentId.As("AttachmentId"))
            .from(t,
                  p.JoinLeft(p.ProjectId == t.Projectid),
                  wj.JoinLeft(wj.TaskId == t.TaskId),
                  at.JoinLeft(at.AttachmentId == wj.AttachmentId))
            .where(t.TaskId == taskId.ToGuid(Guid.Empty)
            & (wj.WorkHours > 0)
            & (t.TaskStatus != TaskKeys.PlanStatus & t.TaskStatus != TaskKeys.DeleteStatus))
            .order_by(wj.RecordDate.Desc);

         //如果是查看个人任务进度，则只显示人工输入的日志记录，否则显示该任务的所有日志记录
         if (searchType == TaskKeys.SearchByPersonal.ToString())
            query.where_and(wj.RecordType == JournalKeys.ManuRecordType);


         query.primary(wj.JournalId)
        .skip((current - 1) * rowCount)
        .take(rowCount);


         //过滤条件
         //模糊搜索用户名、实名进行

         var dateMin = DateTime.MinValue.ToString();
         searchPhrase = searchPhrase.Trim();
         if (searchPhrase != "")
            query.where_and(wj.Comment.Match(searchPhrase));
         if (start != "" && start != dateMin)
            query.where_and(wj.RecordDate >= DateTime.Parse(start).TodayStart());
         if (end != "" && end != dateMin)
            query.where_and(wj.RecordDate < DateTime.Parse(end).TodayEnd());

         //排序条件表达式

         if (sort != null)
         {
            switch (sort.ID)
            {
               case "recordDate": query.order_by(sort.OrderBy(wj.RecordDate)); break;
               case "progress": query.order_by(sort.OrderBy(wj.Progress)); break;
               case "workhours": query.order_by(sort.OrderBy(wj.WorkHours)); break;
               case "recordType": query.order_by(sort.OrderBy(wj.RecordType)); break;
            }
         }


         //获得查询的总数量

         var total = db.ExecuteSizeOfSelect(query);


         //查询结果集

         var result = query.query(db, rd =>
         {
            var journalId = wj.JournalId.GetValue(rd);
            var recordDate = wj.RecordDate.GetValue(rd);
            return new
            {
               id = journalId.IsEmpty() ? Guid.NewGuid() : journalId,
               userId = GetUserInfo().UserId,
               projectName = p.ProjectName.GetValue(rd, "ProjectName"),
               projectId = p.ProjectId.GetValue(rd, "ProjectId"),
               taskId = t.TaskId.GetValue(rd, "TaskId"),
               taskName = t.TaskName.GetValue(rd, "TaskName"),
               progress = wj.Progress.GetValue(rd) + "%",
               workhours = wj.WorkHours.GetValue(rd),
               comment = wj.Comment.GetValue(rd),
               recordDate = recordDate == DateTime.MinValue ? DateTime.Now : recordDate,
               start = t.StartDate.GetValue(rd),
               end = t.EndDate.GetValue(rd),
               attachmentUrl = at.Url.GetValue(rd),
               hasAttachment = !at.AttachmentId.GetValue(rd, "AttachmentId").IsEmpty(),
               recordType = wj.RecordType.GetValue(rd) == JournalKeys.ManuRecordType ? "人工输入" : "父节点自动计算",
               taskType = t.TaskType.GetValue(rd),
               subTypeValue = wj.TaskSubTypeValue.GetValue(rd) // 任务子类型的工作数量
            };
         }).ToList();


         return Json(new
         {
            rows = result,
            current,
            rowCount,
            total
         });
      }

   }

}