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

   public class RoleController : BaseController
   {

      APDBDef.RoleTableDef r = APDBDef.Role;


      // GET: Role/search
      // Post-ajax: Role/search

      public ActionResult Search()
      {
         return View();
      }

      [HttpPost]
      public ActionResult Search(int current, int rowCount, AjaxOrder sort, string searchPhrase)
      {
         ThrowNotAjax();

         var query = APQuery.select(r.Asterisk)
            .from(r)
            .primary(r.RoleId)
            .skip((current - 1) * rowCount)
            .take(rowCount);


         //过滤条件
         //模糊搜索用户名、实名进行

         searchPhrase = searchPhrase.Trim();
         if (searchPhrase != "")
         {
            query.where_and(r.RoleName.Match(searchPhrase));
         }


         //获得查询的总数量

         var total = db.ExecuteSizeOfSelect(query);


         //查询结果集

         var result = query.query(db, rd =>
         {
            return new
            {
               id = r.RoleId.GetValue(rd),
               roleName = r.RoleName.GetValue(rd),
               roleType= RoleKeys.Types[r.RoleType.GetValue(rd)] 
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


      //	GET:	/Role/Edit
      //	POST-Ajax:	/Role/Edit

      public ActionResult Edit(Guid? id)
      {
         var model = id == null || id.Value.IsEmpty() ?
            new Role() :
            db.RoleDal.PrimaryGet(id.Value);

         return PartialView("Edit", model);
      }

      [HttpPost]
      public ActionResult Edit(Role model)
      {
         ThrowNotAjax();

         if (model.RoleId.IsEmpty())
         {
            model.RoleId = Guid.NewGuid();

            db.RoleDal.Insert(model);
         }
         else
         {
            db.RoleDal.UpdatePartial(model.RoleId, new
            {
               model.RoleName,
               model.RoleUseMember,
               model.RoleType
            });
         }


         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Role.EDIT_SUCCESS
         });
      }


      //	POST-Ajax:	/Role/Delete

      public ActionResult Delete(Guid id)
      {
         var role = db.RoleDal.PrimaryGet(id);

         if (role.RoleId != Guid.Empty)
            db.RoleDal.ConditionDelete(r.RoleId == role.RoleId);

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Role.DELETE_SUCCESS
         });
      }


      //	GET:	/Role/UserRoleEdit
      //	POST-Ajax:	/Role/UserRoleEdit

      public ActionResult UserRoleEdit(Guid userId)
      {
         var ur = APDBDef.UserRole;

         var result = APQuery.select(ur.ID.As("urid"), ur.RoleId, r.RoleName, r.RoleId.As("roleId"))
                           .from(r,
                                 ur.JoinLeft(ur.RoleId == r.RoleId & ur.UserId == userId))
                            .query(db, re =>
                            {
                               var urid = ur.RoleId.GetValue(re, "urid");
                               return new UserRoleViewModel
                               {
                                  RoleId = r.RoleId.GetValue(re, "roleId"),
                                  RoleName = r.RoleName.GetValue(re),
                                  IsChecked = !urid.IsEmpty()
                               };
                            }).ToList();

         if (result.Exists(x => x.IsChecked))
            ViewBag.roleIds = string.Join(",", result.Select(x => x.RoleId).ToArray());


         return PartialView(result);
      }

      [HttpPost]
      public ActionResult UserRoleEdit(Guid userId, string roleIds)
      {
         var ur = APDBDef.UserRole;

         var roleIdArray = string.IsNullOrEmpty(roleIds) ?
            new string[0] :
            roleIds.Trim(',').Split(',');

         db.BeginTrans();


         try
         {

            db.UserRoleDal.ConditionDelete(ur.UserId == userId);

            foreach (var item in roleIdArray)
            {
               db.UserRoleDal.Insert(new UserRole { ID = Guid.NewGuid(), RoleId = item.ToGuid(Guid.Empty), UserId = userId });
            }


            db.Commit();
         }
         catch
         {
            db.Rollback();

            return Json(new
            {
               result = AjaxResults.Error,
               msg = Errors.Role.USER_ROLE_EDIT_FAIL
            });
         }


         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Role.USER_ROLE_EDIT_SUCCESS
         });
      }


      //	GET:	/Role/RoleAppEdit
      //	POST-Ajax:	/Role/RoleAppEdit

      //public ActionResult RoleAppEdit(Guid roleId)
      //{
      //   var ra = APDBDef.RoleApp;
      //   var a = APDBDef.App;

      //   var result = APQuery.select(a.AppId.As("AppId"), a.Title, ra.RoleAppId.As("raid"), ra.RoleId.As("roleId"))
      //                     .from(a,
      //                           ra.JoinLeft(ra.AppId == a.AppId & ra.RoleId == roleId))
      //                      .where(a.AppType == ThisApp.ThisAPPType)
      //                      .query(db, r =>
      //                      {
      //                         var raid = ra.AppId.GetValue(r, "raid");
      //                         return new RoleAppViewModel
      //                         {
      //                            AppId = a.AppId.GetValue(r, "AppId"),
      //                            AppName = a.Title.GetValue(r),
      //                            IsChecked = !raid.IsEmpty()
      //                         };
      //                      }).ToList();

      //   if (result.Exists(x=>x.IsChecked))
      //      ViewBag.AppIds = string.Join(",", result.Select(x => x.AppId).ToArray());

      //   return PartialView(result);
      //}


      //[HttpPost]
      //public ActionResult RoleAppEdit(Guid roleId, string appIds)
      //{
      //   var ra = APDBDef.RoleApp;

      //   var appIdArray = string.IsNullOrEmpty(appIds) ?
      //      new string[0] :
      //      appIds.Trim(',').Split(',');


      //   db.BeginTrans();


      //   try
      //   {

      //      db.RoleAppDal.ConditionDelete(ra.RoleId == roleId);

      //      foreach (var item in appIdArray)
      //      {
      //         db.RoleAppDal.Insert(new RoleApp { RoleAppId = Guid.NewGuid(), AppId = item.ToGuid(Guid.Empty), RoleId = roleId, Title=string.Empty });
      //      }


      //      db.Commit();
      //   }
      //   catch(Exception e)
      //   {
      //      db.Rollback();

      //      return Json(new
      //      {
      //         result = AjaxResults.Error,
      //         msg = Errors.Role.ROLE_APP_EDIT_FAIL
      //      });
      //   }


      //   return Json(new
      //   {
      //      result = AjaxResults.Success,
      //      msg = Success.Role.ROLE_APP_EDIT_SUCCESS
      //   });
      //}

   }

}