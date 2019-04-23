using Business;
using Business.Config;
using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TheSite.Models;

namespace TheSite.Controllers
{

   public class UserController : BaseController
   {

      static APDBDef.UserInfoTableDef u = APDBDef.UserInfo;


      // GET: User/search
      // Post-ajax: User/search

      public ActionResult Search()
      {
         return View();
      }

      [HttpPost]
      public JsonResult Search(int current, int rowCount, AjaxOrder sort, string searchPhrase)
      {
         ThrowNotAjax();

         var query = APQuery.select(u.UserId, u.UserName, u.RealName, u.Department, u.Email)
            .from(u)
             .where(u.IsDelete == false)
            .primary(u.UserId)
            .skip((current - 1) * rowCount)
            .take(rowCount);


         //过滤条件
         //模糊搜索用户名、实名进行

         searchPhrase = searchPhrase.Trim();
         if (searchPhrase != "")
         {
            query.where_and(u.UserName.Match(searchPhrase) | u.RealName.Match(searchPhrase));
         }


         //排序条件表达式

         if (sort != null)
         {
            switch (sort.ID)
            {
               case "userName": query.order_by(sort.OrderBy(u.UserName)); break;
               case "realName": query.order_by(sort.OrderBy(u.RealName)); break;
                  //case "userType": query.order_by(sort.OrderBy(u.UserType)); break;
            }
         }


         //获得查询的总数量

         var total = db.ExecuteSizeOfSelect(query);


         //查询结果集

         var result = query.query(db, rd =>
         {
            return new
            {
               id = u.UserId.GetValue(rd),
               userName = u.UserName.GetValue(rd),
               realName = u.RealName.GetValue(rd),
               email = u.Email.GetValue(rd),
               department = u.Department.GetValue(rd),
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


      //	编辑用户
      //	GET:	/User/Edit
      //	POST-AJAX:	/User/Edit

      public ActionResult Edit(Guid? id)
      {
         var model = id == null || id.Value.IsEmpty() ? new UserInfo() : db.UserInfoDal.PrimaryGet(id.Value);

         return PartialView("Edit", model);
      }

      [HttpPost]
      public ActionResult Edit(UserInfo model)
      {
         ThrowNotAjax();

         var manager = new ST_AccountManager(db);
         var result = false;

         var userInfo = db.UserInfoDal.PrimaryGet(model.UserId);
         if (userInfo == null)
         {
            var account = new Account
            {
               UserId = Guid.NewGuid(),
               Account = model.UserName,
               UserName = model.UserName,
               Password = MD5Encryptor.Encrypt(ThisApp.DefaultPassword),
               Status = (int)AccountStatus.Enable
            };

            db.BeginTrans();

            try
            {
               result = manager.Register(account);

               if (userInfo == null && result)
               {
                  userInfo = UserInfo.Initial(account.UserId, account.UserName);
                  db.UserInfoDal.Insert(userInfo);

                  db.Commit();
               }
               else
               {
                  db.Rollback();

                  return Json(new
                  {
                     result = AjaxResults.Error,
                     msg = "用户名已存在!"
                  });
               }
            }
            catch
            {
               result = false;

               db.Rollback();
            }
         }
         else
         {

            var t = APDBDef.UserInfo;

            db.UserInfoDal.UpdatePartial(model.UserId, new
            {
               model.UserName,
               model.RealName,
               model.UserId,
               model.Email,
               model.Department,
            });
         }


         return Json(new
         {
            result = AjaxResults.Success,
            msg = "用户编辑成功!"
         });
      }

   }

}