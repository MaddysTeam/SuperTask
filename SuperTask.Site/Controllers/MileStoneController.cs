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

   public class MileStoneController : BaseController
   {

      static APDBDef.MileStoneTableDef ms = APDBDef.MileStone;


      // GET: MileStone/Search
      // Post-ajax: MileStone/Search

      public ActionResult Search()
      {
         return View();
      }

      [HttpPost]
      public ActionResult Search(string owner, string executor, int current, int rowCount, AjaxOrder sort, string searchPhrase)
      {
         //ThrowNotAjax();

         //var o = APDBDef.Organize;
         //var u = APDBDef.UserInfo;
         //var r = APDBDef.Resource;
         //var user = GetUserInfo();

         //var query = APQuery.select(p.ProjectId, p.RealCode, p.ProjectType, p.ProjectExecutor, p.ProjectName, p.RateOfProgress,
         //                           p.ProjectOwner, p.ProjectStatus, p.OrgId, u.UserName.As("managerName"),
         //                           o.Name.As("orgName"))
         //   .from(p,
         //         o.JoinLeft(o.ID == p.OrgId),
         //         u.JoinLeft(u.UserId == p.ManagerId),
         //         r.JoinInner((r.Projectid == p.ProjectId & r.UserId == user.UserId))
         //         )
         //   .where(p.ProjectStatus.NotIn(ProjectKeys.DeleteStatus, ProjectKeys.CompleteStatus));

         //if (owner != ThisApp.SelectAll)
         //   query.where_and(p.ProjectOwner == owner);

         //if (executor != ThisApp.SelectAll)
         //   query.where_and(p.ProjectExecutor == executor);


         //query.primary(p.ProjectId)
         //   .order_by(p.CreateDate.Desc)
         //   .skip((current - 1) * rowCount)
         //    .take(rowCount);


         ////过滤条件
         ////模糊搜索用户名、实名进行

         //searchPhrase = searchPhrase.Trim();
         //if (searchPhrase != "")
         //{
         //   query.where_and(p.ProjectName.Match(searchPhrase) |
         //      p.ProjectOwner.Match(searchPhrase) |
         //      p.Code.Match(searchPhrase) |
         //      p.ProjectName.Match(searchPhrase) |
         //      p.ProjectExecutor.Match(searchPhrase) |
         //      u.UserName.Match(searchPhrase)
         //      );
         //}


         ////排序条件表达式

         //if (sort != null)
         //{
         //   switch (sort.ID)
         //   {
         //      case "projectName": query.order_by(sort.OrderBy(p.ProjectName)); break;
         //      case "projectOwner": query.order_by(sort.OrderBy(p.ProjectOwner)); break;
         //      case "projectExecutor": query.order_by(sort.OrderBy(p.ProjectExecutor)); break;
         //      case "pmName": query.order_by(sort.OrderBy(u.UserName)); break;
         //      case "type": query.order_by(sort.OrderBy(p.ProjectType)); break;
         //      case "code": query.order_by(sort.OrderBy(p.Code)); break;
         //   }
         //}


         ////获得查询的总数量

         //var total = db.ExecuteSizeOfSelect(query);


         ////查询结果集
         //var result = query.query(db, rd =>
         //{
         //   var statusVal = p.ProjectStatus.GetValue(rd);
         //   var type = p.ProjectType.GetValue(rd);

         //   return new
         //   {
         //      id = p.ProjectId.GetValue(rd),
         //      code = p.RealCode.GetValue(rd),
         //      projectName = p.ProjectName.GetValue(rd),
         //      progress = p.RateOfProgress.GetValue(rd) + "%",
         //      projectOwner = p.ProjectOwner.GetValue(rd),
         //      projectExecutor = p.ProjectExecutor.GetValue(rd),
         //      pmName = u.UserName.GetValue(rd, "managerName"),
         //      projectStatus = ProjectKeys.GetStatusKeyById(statusVal),
         //      orgId = p.OrgId.GetValue(rd),
         //      orgName = o.Name.GetValue(rd, "orgName"),
         //      type = ProjectKeys.GetTypeKeyById(type)
         //   };
         //});

         //return Json(new
         //{
         //   rows = result,
         //   current,
         //   rowCount,
         //   total
         //});

         return null;
      }


      // GET: MileStone/Edit
      // Post-ajax: MileStone/Edit
      // Post-ajax: Project/Details

      public ActionResult Edit(Guid id)
      {
         return PartialView(null);
      }

      [HttpPost]
      public ActionResult Edit(Project project)
      {
         return null;
      }



   }

}