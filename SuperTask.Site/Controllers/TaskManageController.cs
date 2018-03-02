using Business;
using Business.Config;
using Business.Helper;
using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TheSite.Models;

namespace TheSite.Controllers
{

   public class TaskManageController : BaseController
   {

      APDBDef.ResourceTableDef re = APDBDef.Resource;
      APDBDef.WorkTaskComplextiyTableDef wc = APDBDef.WorkTaskComplextiy;
      APDBDef.WorkTaskTableDef t = APDBDef.WorkTask;
      APDBDef.ProjectTableDef p = APDBDef.Project;
      APDBDef.TaskStandardItemTableDef st = APDBDef.TaskStandardItem;
      APDBDef.WorkTaskIssueTableDef wtit = APDBDef.WorkTaskIssue;
      APDBDef.IssueItemTableDef iit = APDBDef.IssueItem;


      //	GET: TaskManage/TaskComplexityList
      //	POST-Ajax: TaskManage/TaskComplexityList

      public ActionResult TaskComplexityList()
      {
         ViewBag.Roles = RoleHelper.GetUserRoles(GetUserInfo().UserId, db);

         return View();
      }

      [HttpPost]
      public ActionResult TaskComplexityList(Guid roleId, int current, int rowCount, AjaxOrder sort, string searchPhrase)
      {
         var user = GetUserInfo();
         var role = Role.PrimaryGet(roleId);

         var subQuery = APQuery.select(re.Projectid).from(re).where(re.UserId == user.UserId);

         var query = APQuery.select(t.TaskId, t.TaskName, st.StandardComplextiy,
            p.ProjectId.As("projectId"), p.ProjectName,
            wc.Complexity, wc.CreatorId, wc.ModifyDate, wc.ComplextiyId)
                          .from(t,
                                st.JoinLeft(t.StandardItemId == st.ItemId),
                                wc.JoinLeft(wc.TaskId == t.TaskId & wc.CreatorRoleId == roleId & wc.CreatorId == user.UserId),
                                p.JoinLeft(t.Projectid == p.ProjectId)
                                )
                           .where(p.ProjectId.In(subQuery) & t.TaskStatus != TaskKeys.DeleteStatus)
                           .order_by(p.ProjectId.Asc)
                           .primary(t.TaskId)
                           .skip((current - 1) * rowCount)
                           .take(rowCount);


         //过滤条件

         searchPhrase = searchPhrase.Trim();
         if (searchPhrase != "")
         {
            query.where_and(t.TaskName.Match(searchPhrase));
         }


         //获得查询的总数量

         var total = db.ExecuteSizeOfSelect(query);


         //查询结果集

         var result = query.query(db, rd =>
         {
            var complexity = wc.Complexity.GetValue(rd) == 0 ? "未修改" : wc.Complexity.GetValue(rd).ToString();
            var standard = st.StandardComplextiy.GetValue(rd) == 0 ? ThisApp.DefalutTaskStandardComplexity : st.StandardComplextiy.GetValue(rd);
            var modifyDate = wc.ModifyDate.GetValue(rd).IsEmpty() ? "" : wc.ModifyDate.GetValue(rd).ToString();

            return new
            {
               id = wc.ComplextiyId.GetValue(rd),
               taskId = t.TaskId.GetValue(rd),
               task = t.TaskName.GetValue(rd),
               project = p.ProjectName.GetValue(rd),
               standard = standard,
               complexity = complexity,
               modifyDate = wc.ModifyDate.GetValue(rd),
               roleName = role.RoleName,
               roleId = roleId
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


      //	GET: TaskManage/TaskComplexityEdit
      //	POST-Ajax: TaskManage/TaskComplexityEdit

      public ActionResult TaskComplexityEdit(Guid id, Guid taskId, Guid roleId)
      {
         var r = APDBDef.Role;
         var model = WorkTaskComplextiy.PrimaryGet(id);
         var user = GetUserInfo();

         if (model == null)
            model = new WorkTaskComplextiy
            {
               Complexity = ThisApp.DefalutTaskStandardComplexity,
               TaskId = taskId,
               CreatorId = user.UserId,
               CreatorRoleId = roleId
            };

         return PartialView(model);
      }

      [HttpPost]
      public ActionResult TaskComplexityEdit(WorkTaskComplextiy model)
      {
         if (model.ComplextiyId.IsEmpty())
         {
            model.CreateDate = DateTime.Now;
            model.ComplextiyId = Guid.NewGuid();

            db.WorkTaskComplextiyDal.Insert(model);
         }
         else
         {
            model.ModifyDate = DateTime.Now;

            db.WorkTaskComplextiyDal.Update(model);
         }


         return Json(new
         {
            result = AjaxResults.Success,
            msg = "操作成功"
         });
      }


      //	GET: TaskManage/TaskStandardItemList
      //	POST-Ajax: TaskManage/TaskStandardItemList

      public ActionResult TaskStandardItemList()
      {
         return View();
      }

      [HttpPost]
      public ActionResult TaskStandardItemList(int current, int rowCount, AjaxOrder sort, string searchPhrase)
      {
         var query = APQuery.select(st.Asterisk)
                 .from(st)
                 .primary(st.ItemId)
                 .order_by(st.ItemName.Asc)
                 .skip((current-1) * rowCount)
                 .take(rowCount);


         //获得查询的总数量

         var total = db.ExecuteSizeOfSelect(query);


         var result = query.query(db, r => new
         {
            id=st.ItemId.GetValue(r),
            name = st.ItemName.GetValue(r),
            description = st.ItemDescription.GetValue(r),
            standardWorkhours = st.StandardWorkhours.GetValue(r),
            standardComplextiy = st.StandardComplextiy.GetValue(r)
         });

         return Json(new
         {
            rows = result,
            current,
            rowCount,
            total
         });
      }


      //	GET: TaskManage/TaskStandardItemEdit
      //	POST-Ajax: TaskManage/TaskStandardItemEdit

      public ActionResult TaskStandardItemEdit(Guid? id)
      {
         if (null == id)
         {
            return PartialView();
         }

         var model = TaskStandardItem.PrimaryGet(id.Value);

         return PartialView(model);
      }

      [HttpPost]
      public ActionResult TaskStandardItemEdit(TaskStandardItem model)
      {
         if(model.ItemId.IsEmpty())
         {
            model.ItemId = Guid.NewGuid();
            db.TaskStandardItemDal.Insert(model);
         }
         else
         {
            db.TaskStandardItemDal.Update(model);
         }

         return Json(new
         {
            result = AjaxResults.Success,
            msg = "操作成功"
         });
      }


      //	GET: TaskManage/TaskIssueList 
      //	POST-Ajax: TaskManage/TaskIssueList

      public ActionResult TaskIssueList()
      {
         return View();
      }

      [HttpPost]
      public ActionResult TaskIssueList(int current, int rowCount, AjaxOrder sort, string searchPhrase)
      {
         var u = APDBDef.UserInfo;

         var user = GetUserInfo();
         var subQuery = APQuery.select(re.Projectid).from(re).where(re.UserId == user.UserId);
         var query = APQuery.select(t.TaskId.As("TaskId"), t.TaskName.As("TaskName"), u.UserName,
                     p.ProjectName.As("ProjectName"),
                     wtit.ModifyDate, wtit.TaskIssueId.As("TaskIssueId"), wtit.Description,
                     iit.ItemName.As("issueName"))
                          .from(t,
                                p.JoinLeft(t.Projectid == p.ProjectId),
                                u.JoinInner(t.ManagerId == u.UserId),
                                wtit.JoinInner(t.TaskId == wtit.TaskId),
                                iit.JoinInner(iit.ItemId == wtit.IssueId)
                                )
                           .where(p.ProjectId.In(subQuery) & t.TaskStatus != TaskKeys.DeleteStatus)
                           .order_by(p.ProjectId.Asc)
                           .primary(t.TaskId)
                           .skip((current - 1) * rowCount)
                           .take(rowCount);


         //过滤条件

         searchPhrase = searchPhrase.Trim();
         if (searchPhrase != "")
         {
            query.where_and(t.TaskName.Match(searchPhrase)
                          | p.ProjectName.Match(searchPhrase)
                          | iit.ItemName.Match(searchPhrase)
                          | u.UserName.Match(searchPhrase)
                          );
         }


         //获得查询的总数量

         var total = db.ExecuteSizeOfSelect(query);


         //查询结果集

         var result = query.query(db, rd =>
         {
            return new
            {
               id = wtit.TaskIssueId.GetValue(rd, "TaskIssueId"),
               taskId = t.TaskId.GetValue(rd, "TaskId"),
               task = t.TaskName.GetValue(rd, "TaskName"),
               project = p.ProjectName.GetValue(rd, "ProjectName"),
               manager = u.UserName.GetValue(rd),
               issue = iit.ItemName.GetValue(rd, "issueName"),
               desc = wtit.Description.GetValue(rd),
               modifyDate = wc.ModifyDate.GetValue(rd),
               modifyUser = GetUserInfo().UserName
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


      //	GET: TaskManage/TaskIssueEdit
      //	POST-Ajax: TaskManage/TaskIssueEdit
      // POST-Ajax: TaskManage/TaskIssueDelete

      public ActionResult TaskIssueEdit()
      {
         var projects = db.ProjectDal.ConditionQuery(p.ManagerId == GetUserInfo().UserId, null, null, null);

         ViewBag.MyProjects = projects;

         ViewBag.IssueList = IssueItem.GetAll();

         ViewBag.ProjectId = projects.Count > 0 ? projects.First().ProjectId : Guid.Empty;

         return PartialView();
      }

      [HttpPost]
      public ActionResult TaskIssueEdit(WorkTaskIssue model)
      {
         if (model.TaskIssueId.IsEmpty())
         {
            model.CreateDate = DateTime.Now;
            model.ModifyDate = DateTime.Now;
            model.TaskIssueId = Guid.NewGuid();

            db.WorkTaskIssueDal.Insert(model);
         }
         else
         {
            model.ModifyDate = DateTime.Now;

            db.WorkTaskIssueDal.Update(model);
         }


         return Json(new
         {
            result = AjaxResults.Success,
            msg = "操作成功"
         });
      }

      [HttpPost]
      public ActionResult TaskIssueDelete(Guid id)
      {
         db.WorkTaskIssueDal.PrimaryDelete(id);

         return Json(new
         {
            result = AjaxResults.Success,
            msg = "操作成功"
         });
      }


      [HttpPost]
      public ActionResult GetProjectTasks(Guid projectId)
      {
         var userid = GetUserInfo().UserId;
         var tasks = TaskHelper.GetProjectTasks(projectId, db)
             .Select(x => new
             {
                parentId = x.TaskId,
                parentName = x.TaskName
             }).ToList();

         return Json(new
         {
            tasks
         });
      }

   }

}