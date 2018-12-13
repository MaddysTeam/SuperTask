using Business;
using Business.Cache;
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

   public class ResourceController : BaseController
   {

      protected APDBDef.ResourceTableDef re = APDBDef.Resource;


      // GET: Resource/List
      // POST-Ajax: Resource/List

      public ActionResult List()
      {
         return View();
      }

      [HttpPost]
      public ActionResult List(ResourceSearchOption option)
      {
         var t = APDBDef.WorkTask;
         var p = APDBDef.Project;
         var u = APDBDef.UserInfo;
         var rr = APDBDef.ProjectRole;

         var roles = ResourceHelper.GetProjectRoles(option.ProjectId, db);

         var query = APQuery.select(re.ResourceId, re.UserId.As("UserId"), re.ResourceTypes, u.UserName.As("UserName"), p.ProjectId.As("ProjectId"), p.ProjectName.As("ProjectName"), p.ProjectStatus)
                  .from(re,
               u.JoinInner(u.UserId == re.UserId),
               p.JoinInner(p.ProjectId == re.Projectid));

         HandleManager.ResourceSearchHandlers[option.SearchType].Handle(query, option);

         var list = query.query(db, r =>
            new Resource
            {
               ResourceId = re.ResourceId.GetValue(r),
               UserId = re.UserId.GetValue(r, "UserId"),
               ResourceName = u.UserName.GetValue(r, "UserName"),
               Projectid = p.ProjectId.GetValue(r, "ProjectId"),
               ProjectName = p.ProjectName.GetValue(r, "ProjectName"),
               ResourceTypes = re.ResourceTypes.GetValue(r),
               TypeNames = GetNamesConnectByCommas(re.ResourceTypes.GetValue(r), roles),
               Status = p.ProjectStatus.GetValue(r) == ProjectKeys.CompleteStatus ?
                                              ResourceKeys.ReadonlyStatus : ResourceKeys.EditableStatus,
            }).ToList();


         return PartialView(option.ViewName, list);
      }


      // POST-Ajax: Resource/RoleList

      [HttpPost]
      public ActionResult RoleList(ResourceSearchOption option)
      {
         var roles = ResourceHelper.GetProjectRoles(option.ProjectId, db);

         return PartialView(option.ViewName, roles);
      }


      // GET: Resource/ResourceEdit
      // POST-Ajax: Resource/Edit

      public ActionResult Edit(Guid? id)
      {
         var projectId = Request["projectId"];

         if (projectId == null) throw new ApplicationException();

         var roles = ResourceHelper.GetProjectRoles(Guid.Parse(projectId), db);

         var roleSelectItems = roles
            .Select(item => new SelectListItem { Text = item.RoleName, Value = item.RoleId.ToString() })
            .ToList();


         Resource resource = null;

         if (id != null && !id.Value.IsEmpty())
         {
            resource = db.ResourceDal.PrimaryGet(id.Value);

            var resourceRoles = resource.ResourceTypes == null ? null : resource.ResourceTypes.Split(',');

            foreach (var item in roleSelectItems)
               item.Selected = resourceRoles.Contains(item.Value, StringComparison.InvariantCultureIgnoreCase);

         }
         else
         {
            resource = new Resource
            {
               ResourceId = Guid.NewGuid(),
               Projectid = Guid.Parse(projectId)
            };
         }

         ViewBag.ResourceRoles = roleSelectItems;

         var ac = APDBDef.Account;
         ViewBag.Resource = db.AccountDal.ConditionQuery(ac.Status == 0, null, null, null); //没有被禁用的所有人员

         return PartialView(resource);
      }

      [HttpPost]
      public ActionResult Edit(Resource resource)
      {
         var result = resource.Validate();
         if (!result.IsSuccess)
            return Json(new
            {
               result = AjaxResults.Error,
               msg = result.Msg
            });

         var notExists = db.ResourceDal.ConditionQueryCount(re.Projectid == resource.Projectid & re.UserId == resource.UserId) <= 0;


         db.BeginTrans();


         try
         {
            if (notExists)
            {
               resource.SetStatus(ResourceKeys.EditableStatus);

               db.ResourceDal.Insert(resource);
            }
            else
            {
               db.ResourceDal.Update(resource);
            }

            if (resource.IsPM())
               ResourceHelper.ReplacePM(resource.Projectid, resource.UserId, db);

            if (resource.IsHeader())
               ResourceHelper.ReplaceHeader(resource.Projectid, resource.UserId, db);

            //编辑后检查，如果没有领导角色则提示，如果没有项目经理或负责人，在项目属性中修改
            var resources = db.ResourceDal.ConditionQuery(re.Projectid == resource.Projectid, null, null, null);
            if (!resources.Exists(re => re.IsLeader()))
            {
               db.Rollback();

               return Json(new
               {
                  result = AjaxResults.Error,
                  msg = Errors.Resource.LEADER_NOT_EXISTS
               });
            }
            else if (!resources.Exists(re => re.IsPM()))
               db.ProjectDal.UpdatePartial(resource.Projectid, new { ManagerId = Guid.Empty });
            else if (!resources.Exists(re => re.IsHeader()))
               db.ProjectDal.UpdatePartial(resource.Projectid, new { PMId = Guid.Empty });


            db.Commit();
         }
         catch
         {
            db.Rollback();

            return Json(new
            {
               result = AjaxResults.Error,
               msg = Errors.Resource.EDIT_FAIL
            });
         }


         ResourceHelper.CleanResourceCache();

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Resource.EDIT_SUCCESS
         });
      }


      // GET: Resource/RoleAppEdit
      // POST-Ajax: Resource/RoleAppEdit

      public ActionResult RoleAppEdit(Guid resourceRoleId)
      {
         var r = APDBDef.Role;
         var pr = APDBDef.ProjectRole;
         var p = APDBDef.App;


         var role = db.ProjectRoleDal
                      .ConditionQuery(pr.PRID == resourceRoleId, null, null, null)
                      .FirstOrDefault();


         var models = role == null ?
            new List<RoleAppViewModel>() :
            GetViewModelbyAppIds(role.AppIds);

         var existApps = models.Select(vm => vm.AppId);

         ViewBag.ExistAppids = existApps == null && existApps.Count() <= 0 ? string.Empty: string.Join(",", existApps.ToArray());


         return PartialView(models);
      }

      [HttpPost]
      public ActionResult RoleAppEdit(Guid resourceRoleId, string appIds)
      {
         db.ProjectRoleDal.UpdatePartial(resourceRoleId, new { AppIds = appIds });

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Role.ROLE_APP_EDIT_SUCCESS
         });
      }


      // POST-Ajax: Resource/Delete

      [HttpPost]
      public ActionResult Delete(Guid id)
      {
         if (!id.IsEmpty())
         {
            var t = APDBDef.WorkTask;
            var p = APDBDef.Project;

            var query = APQuery.select(re.ResourceId)
                             .from(re,
                                   p.JoinInner(p.ProjectId == re.Projectid),
                                   t.JoinInner(t.Projectid == p.ProjectId & (t.ManagerId == re.UserId | t.ReviewerID == re.UserId) & t.TaskStatus != TaskKeys.DeleteStatus))
                             .where(re.ResourceId == id);

            var total = db.ExecuteSizeOfSelect(query);

            if (total > 0)
            {
               return Json(new
               {
                  result = AjaxResults.Error,
                  msg = Errors.Resource.NOT_ALLOWED_DELETE_DUETO_HASTASKS
               });
            }


            db.ResourceDal.PrimaryDelete(id);
         }


         return Json(new
         {
            result = AjaxResults.Success,
            msg = "操作成功"
         });
      }


      private string GetNamesConnectByCommas(string ids, List<ProjectRole> roles)
      {
         if (string.IsNullOrEmpty(ids) || roles == null || roles.Count <= 0)
            return ids;

         var str = string.Empty;

         ids.Trim(',').Split(',').ToList().ForEach(id =>
         {
            var role = roles.Find(x => x.RoleId == id.ToGuid(Guid.Empty));
            if (role != null)
               str += role.RoleName + ",";
         });

         return str.Trim(',');
      }


      private List<RoleAppViewModel> GetViewModelbyAppIds(string appIds)
      {
         var viewModes = new List<RoleAppViewModel>();
         var apps = GetRoleApp();

         apps.ForEach(app =>
         {
            var model = new RoleAppViewModel
            {
               AppId = app.AppId,
               AppName = app.Title,
            };

            viewModes.Add(model);
         });

         if (string.IsNullOrEmpty(appIds))
            return viewModes;

         appIds.Trim(',').Split(',').ToList().ForEach(id =>
         {
            var model = viewModes.Find(vm => vm.AppId == Guid.Parse(id));
            if (model != null)
               model.IsChecked = true;
         });

         return viewModes;
      }


      private List<App> GetRoleApp()
      {
         var a = APDBDef.App;

         return db.AppDal.ConditionQuery(a.Code.Match(AppKeys.ProjectAppCodePrefix), null, null, null);
      }

   }

}