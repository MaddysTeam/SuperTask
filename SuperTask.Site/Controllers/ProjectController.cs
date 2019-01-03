using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business;
using Symber.Web.Data;
using Business.Config;
using Business.Helper;
using TheSite.Models;
using Business.Roadflow;

namespace TheSite.Controllers
{

   public class ProjectController : BaseController
   {

      static APDBDef.ProjectTableDef p = APDBDef.Project;


      // GET: Project/Search
      // Post-ajax: Project/Search

      public ActionResult Search()
      {
         //TODO: 稍晚要修改从 company 表读取

         var all = db.ProjectDal.ConditionQuery(null, null, null, null)
            .ToDictionary(x => x.ProjectId, v => new { owner = v.ProjectOwner, executor = v.ProjectExecutor });

         var executors = new List<SelectListItem>();
         var owners = new List<SelectListItem>();

         if (all != null && all.Count > 0)
         {
            executors = all.Values.GroupBy(x => x.executor).Select(v => new SelectListItem { Text = v.Key, Value = v.Key }).ToList();
            owners = all.Values.GroupBy(x => x.owner).Select(v => new SelectListItem { Text = v.Key, Value = v.Key }).ToList();
         }

         ViewBag.Owners = owners;
         ViewBag.Executors = executors;

         return View();
      }

      [HttpPost]
      public ActionResult Search(string owner, string executor, int current, int rowCount, AjaxOrder sort, string searchPhrase)
      {
         ThrowNotAjax();

         var o = APDBDef.Organize;
         var u = APDBDef.UserInfo;
         var r = APDBDef.Resource;
         var user = GetUserInfo();

         var query = APQuery.select(p.ProjectId, p.RealCode, p.ProjectType, p.ProjectExecutor, p.ProjectName, p.RateOfProgress,
                                    p.ProjectOwner, p.ProjectStatus, p.OrgId, u.UserName.As("managerName"),
                                    o.Name.As("orgName"))
            .from(p,
                  o.JoinLeft(o.ID == p.OrgId),
                  u.JoinLeft(u.UserId == p.ManagerId),
                  r.JoinInner((r.Projectid == p.ProjectId & r.UserId == user.UserId))
                  )
            .where(p.ProjectStatus.NotIn(ProjectKeys.DeleteStatus, ProjectKeys.CompleteStatus));

         if (owner != ThisApp.SelectAll)
            query.where_and(p.ProjectOwner == owner);

         if (executor != ThisApp.SelectAll)
            query.where_and(p.ProjectExecutor == executor);


         query.primary(p.ProjectId)
            .order_by(p.CreateDate.Desc)
            .skip((current - 1) * rowCount)
             .take(rowCount);


         //过滤条件
         //模糊搜索用户名、实名进行

         searchPhrase = searchPhrase.Trim();
         if (searchPhrase != "")
         {
            query.where_and(p.ProjectName.Match(searchPhrase) |
               p.ProjectOwner.Match(searchPhrase) |
               p.Code.Match(searchPhrase) |
               p.ProjectName.Match(searchPhrase) |
               p.ProjectExecutor.Match(searchPhrase) |
               u.UserName.Match(searchPhrase)
               );
         }


         //排序条件表达式

         if (sort != null)
         {
            switch (sort.ID)
            {
               case "projectName": query.order_by(sort.OrderBy(p.ProjectName)); break;
               case "projectOwner": query.order_by(sort.OrderBy(p.ProjectOwner)); break;
               case "projectExecutor": query.order_by(sort.OrderBy(p.ProjectExecutor)); break;
               case "pmName": query.order_by(sort.OrderBy(u.UserName)); break;
               case "type": query.order_by(sort.OrderBy(p.ProjectType)); break;
               case "code": query.order_by(sort.OrderBy(p.Code)); break;
            }
         }


         //获得查询的总数量

         var total = db.ExecuteSizeOfSelect(query);


         //查询结果集
         var result = query.query(db, rd =>
         {
            var statusVal = p.ProjectStatus.GetValue(rd);
            var type = p.ProjectType.GetValue(rd);

            return new
            {
               id = p.ProjectId.GetValue(rd),
               code = p.RealCode.GetValue(rd),
               projectName = p.ProjectName.GetValue(rd),
               progress = p.RateOfProgress.GetValue(rd) + "%",
               projectOwner = p.ProjectOwner.GetValue(rd),
               projectExecutor = p.ProjectExecutor.GetValue(rd),
               pmName = u.UserName.GetValue(rd, "managerName"),
               projectStatus = ProjectKeys.GetStatusKeyById(statusVal),
               orgId = p.OrgId.GetValue(rd),
               orgName = o.Name.GetValue(rd, "orgName"),
               type = ProjectKeys.GetTypeKeyById(type)
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


      // GET: Project/Edit
      // Post-ajax: Project/Edit
      // Post-ajax: Project/Details

      public ActionResult Edit(Guid id)
      {
         var userId = GetUserInfo().UserId;
         var prj = new Project { PMId = userId, ManagerId = userId, ReviewerId = userId, CreatorId = userId, CreateDate = DateTime.Now }; //default

         return PartialView(prj);
      }

      [HttpPost]
      public ActionResult Edit(Project project)
      {
         var orignal = ProjectrHelper.GetCurrentProject(project.ProjectId,db,true);
         var option = new ProjectEditOption { CurrentUser = GetUserInfo(), db = db, Status=project.ProjectStatus,Orignal= orignal };
         HandleManager.ProjectEditHandlers[ProjectKeys.DefaultProjectType].Handle(project, option);

         if (!option.Result.IsSuccess)
         {
            return Json(new
            {
               result = AjaxResults.Error,
               msg = option.Result.Msg
            });
         }

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Project.EDIT_SUCCESS
         });
      }


      [HttpGet]
      public ActionResult Details(Guid id)
      {
         if (id.IsEmpty())
            return RedirectToAction("Search", "Project");

         var cu = APDBDef.UserInfo;
         var pu = APDBDef.UserInfo.As("Manager");//项目经理
         var he = APDBDef.UserInfo.As("Header");//项目负责人
         var ru = APDBDef.UserInfo.As("Reviewer");
         var o = APDBDef.Organize;
         var re = APDBDef.Resource;

         var project = APQuery.select(p.Asterisk, he.UserName.As("Header"), cu.UserName, pu.UserName.As("Manager"), ru.UserName.As("ReviewerName"), o.Name.As("OrgName"))
            .from(p,
            cu.JoinLeft(cu.UserId == p.CreatorId),
            pu.JoinLeft(pu.UserId == p.ManagerId),//项目经理
            he.JoinLeft(he.UserId == p.PMId),//项目负责人
            ru.JoinLeft(ru.UserId == p.ReviewerId),
            o.JoinLeft(o.ID == p.OrgId)
            )
            .where(p.ProjectId == id)
            .query(db, r =>
            {
               var proj = new Project();
               p.Fullup(r, proj, false);

               proj.Creator = cu.UserName.GetValue(r);
               proj.OrgName = o.Name.GetValue(r, "OrgName");
               proj.Reviewer = ru.UserName.GetValue(r, "ReviewerName");
               proj.Manager = pu.UserName.GetValue(r, "Manager");
               proj.Header = he.UserName.GetValue(r, "Header");
               proj.RateOfProgress = p.RateOfProgress.GetValue(r);

               return proj;
            }).FirstOrDefault();

         project.Resources= db.ResourceDal.ConditionQuery(re.Projectid == project.ProjectId, null, null, null);
         project.MileStones = MilestoneHelper.GetProjectMileStones(id, db);
         project.Payments = PaymentsHelper.GetProjectPayments(id,db);

         return View(project);
      }


      // Post-ajax: Project/LoadTemplate

      [HttpPost]
      public ActionResult LoadTemplate(string template)
      {
         return PartialView(template,null);
      }


      // GET: Project/CreateByTemplate
      // Post-ajax: Project/CreateByTemplate

      public ActionResult CreateByTemplate()
      {
         return PartialView();
      }

      [HttpPost]
      public ActionResult CreateByTemplate(Guid projectType)
      {
         var option = new PorjectTemplateEditOption {
            CurrentUser =GetUserInfo(),
            db =db,
            Status =ProjectKeys.PlanStatus,
            ProjectEditHandler =HandleManager.ProjectEditHandlers[ProjectKeys.DefaultProjectType],
         };
         HandleManager.ProjectTemplateEditHandlers[projectType].Handle(null, option);

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Project.EDIT_SUCCESS
         });
      }


   }

}