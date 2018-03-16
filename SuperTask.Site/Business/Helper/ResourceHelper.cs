using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Business.Helper
{

   public static class ResourceHelper
   {
      private static Dictionary<Guid, List<Resource>> _currentProjectResource;

      public static List<Resource> GetCurrentProjectResources(Guid projectId, bool isForceClear, APDBDef db = null)
      {
         if (_currentProjectResource != null && _currentProjectResource.ContainsKey(projectId))
            return _currentProjectResource[projectId];

         db = db ?? new APDBDef();
         var re = APDBDef.Resource;
         var resources = db.ResourceDal.ConditionQuery(re.Projectid == projectId, null, null, null);
         if (resources.Count <= 0) return new List<Resource>();
         _currentProjectResource = _currentProjectResource ?? new Dictionary<Guid, List<Resource>>();
         _currentProjectResource.Add(projectId, resources);

         return resources;
      }

      public static Resource GetCurrentProjectResource(Guid projectId, Guid userId, bool isForceClear, APDBDef db = null)
      {
         var resources = GetCurrentProjectResources(projectId, isForceClear, db);

         return resources.Find(r => r.UserId == userId);
      }

      public static void CleanResourceCache()
      {
         if (_currentProjectResource != null)
            _currentProjectResource.Clear();
      }


      public static void ReplaceLeader(Guid projectId, Guid managerId, Guid headerId, APDBDef db)
      {
         ReplacePM(projectId, managerId, db);

         ReplaceHeader(projectId, headerId, db);
      }


      public static void ReplacePM(Guid projectId, Guid managerId, APDBDef db)
      {
         var re = APDBDef.Resource;

         var resources = db.ResourceDal.ConditionQuery(re.Projectid == projectId, null, null, null);
         var orignalPMs = resources.FindAll(x => x.IsPM());

         foreach (var pm in orignalPMs)
         {
            var array = pm.ResourceTypes.Trim(',').Split(',').ToList();
            if (array.Count > 0)
               array.Remove(ResourceKeys.PMType + "");

            pm.ResourceTypes = string.Join(",", array.ToArray());
         }

         var current = resources.Find(x => x.UserId == managerId);
         if (current != null)
            current.ResourceTypes += "," + ResourceKeys.PMType;


         db.ResourceDal.ConditionDelete(re.Projectid == projectId);

         foreach (var item in resources)
            db.ResourceDal.Insert(item);

         db.ProjectDal.UpdatePartial(projectId, new { ManagerId = managerId });
      }


      public static void ReplaceHeader(Guid projectId, Guid headerId, APDBDef db)
      {
         var re = APDBDef.Resource;

         var resources = db.ResourceDal.ConditionQuery(re.Projectid == projectId, null, null, null);
         var orignalHeaders = resources.FindAll(x => x.IsHeader());
         foreach (var header in orignalHeaders)
         {
            var array = header.ResourceTypes.Trim(',').Split(',').ToList();
            if (array.Count > 0)
               array.Remove(ResourceKeys.HeaderType + "");

            header.ResourceTypes = string.Join(",", array.ToArray());
         }

         var currentHeader = resources.Find(x => x.UserId == headerId);
         if (currentHeader != null)
            currentHeader.ResourceTypes += "," + ResourceKeys.HeaderType;


         db.ResourceDal.ConditionDelete(re.Projectid == projectId);

         foreach (var item in resources)
            db.ResourceDal.Insert(item);

         db.ProjectDal.UpdatePartial(projectId, new { ManagerId = headerId });

      }


      public static void AddUserToResourceIfNotExist(Guid projectId, Guid TaskId, Guid userId, Guid type, APDBDef db)
      {
         var re = APDBDef.Resource;

         var resource = db.ResourceDal.ConditionQuery(re.Projectid == projectId & re.UserId == userId, null, null, null).FirstOrDefault();
         if (resource == null)
         {
            var user = db.UserInfoDal.PrimaryGet(userId);

            resource = new Resource
            {
               ResourceId = Guid.NewGuid(),
               UserId = userId,
               ResourceName = user.UserName,
               Projectid = projectId,
               TaskId = TaskId,
               Status = ResourceKeys.EditableStatus,
               ResourceTypes = type + ""
            };

            db.ResourceDal.Insert(resource);
         }

         if (resource.IsPM())
            ReplacePM(resource.Projectid, resource.UserId, db);

         if (resource.IsHeader())
            ReplaceHeader(resource.Projectid, resource.UserId, db);
      }


      public static List<ProjectRole> GetProjectRoles(Guid projectId, APDBDef db)
      {
         var r = APDBDef.Role;
         var pr = APDBDef.ProjectRole;

         var roles = APQuery.select(r.RoleId.As("roleId"), r.RoleName, pr.AppIds, pr.PRID.As("prid"))
            .from(r, pr.JoinInner(pr.RoleId == r.RoleId))
            .where(r.RoleType == RoleKeys.ProjectType & pr.ProjectId == projectId)
            .query(db, rd => new ProjectRole
            {
               PRID = pr.PRID.GetValue(rd, "prid"),
               RoleId = r.RoleId.GetValue(rd, "roleId"),
               RoleName = r.RoleName.GetValue(rd),
               ProjectId = projectId,
               AppIds = pr.AppIds.GetValue(rd)
            }).ToList();


         return roles;
      }


      public static void AddDefaultResoureRoles(Project project, APDBDef db)
      {
         var r = APDBDef.Role;
         var app = APDBDef.App;

         var prs = db.RoleDal.ConditionQuery(r.RoleType == RoleKeys.ProjectType, null, null, null);
         var apps = db.AppDal.ConditionQuery(app.Code.Match(AppKeys.ProjectAppCodePrefix), null, null, null);
         var appIds = string.Join(",", apps.Select(x => x.AppId).ToArray());

         prs.ForEach(item =>
            {
               db.ProjectRoleDal.Insert(new ProjectRole
               {
                  PRID = Guid.NewGuid(),
                  ProjectId = project.ProjectId,
                  RoleId = item.RoleId,
                  RoleName = item.RoleName,
                  AppIds = item.RoleName == RoleKeys.HEADER ||
                           item.RoleName == RoleKeys.PROGRAME_MANAGER ?
                                          appIds :
                                          string.Empty
               });
            });
      }


      public static bool HasPermission(Guid userId, Guid projectId, string codeOrUrl, APDBDef db)
      {
         var re = APDBDef.Resource;
         var rr = APDBDef.ProjectRole;
         var a = APDBDef.App;

         var apps = db.AppDal.ConditionQuery(a.Code.Match(AppKeys.ProjectAppCodePrefix), null, null, null);
         var resourceRoles = db.ProjectRoleDal.ConditionQuery(rr.ProjectId == projectId, null, null, null);
         var resource = db.ResourceDal.ConditionQuery(re.Projectid == projectId & re.UserId == userId, null, null, null).FirstOrDefault();

         var currentApps = new List<App>();

         if (resource != null && !string.IsNullOrEmpty(resource.ResourceTypes))
         {
            foreach (var roleid in resource.ResourceTypes.Trim(',').Split(','))
            {
               var role = resourceRoles.Find(r => r.RoleId == Guid.Parse(roleid));
               if (role != null && !string.IsNullOrEmpty(role.AppIds))
               {
                  foreach (var appId in role.AppIds.Trim(',').Split(','))
                  {
                     var ap = apps.Find(appl => appl.AppId == Guid.Parse(appId));
                     currentApps.Add(ap);
                  }
               }
            }
         }

         return codeOrUrl.IndexOf(AppKeys.ProjectAppCodePrefix) >= 0 ?
            currentApps.Exists(app => app.Code == codeOrUrl) :
            currentApps.Exists(app => app.Address == codeOrUrl);

      }


      public static bool RemoveUserResource(Guid userId, APDBDef db)
      {
         if (userId.IsEmpty()) return false;

         var u = APDBDef.UserInfo;

         db.ResourceDal.ConditionDelete(u.UserId == userId);

         return true;
      }

   }

}
