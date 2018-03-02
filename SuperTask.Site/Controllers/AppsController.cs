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

   public class AppsController : BaseController
   {

      APDBDef.AppTableDef a = APDBDef.App;

      // GET: Apps/search
      // Post-ajax: Apps/search

      public ActionResult Search()
      {
         return View();
      }

      [HttpPost]
      public ActionResult Search(int current,int rowCount, AjaxOrder sort, string searchPhrase)
      {
         ThrowNotAjax();

         var query = APQuery.select(a.Asterisk)
            .from(a)
            .where(a.AppType== ThisApp.ThisAPPType)
            .primary(a.AppId)
            .skip((current - 1) * rowCount)
            .take(rowCount);


         //过滤条件
         //模糊搜索用户名、实名进行

         searchPhrase = searchPhrase.Trim();
         if (searchPhrase != "")
         {
            query.where_and(a.Title.Match(searchPhrase));
         }


         //排序条件表达式

         if (sort != null)
         {
            switch (sort.ID)
            {
               //case "userName": query.order_by(sort.OrderBy(u.UserName)); break;
               //case "realName": query.order_by(sort.OrderBy(u.RealName)); break;
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
               id = a.AppId.GetValue(rd),
               title=a.Title.GetValue(rd),
               code=a.Code.GetValue(rd),
               address=a.Address.GetValue(rd),
               note=a.Note.GetValue(rd)
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


      //	GET:	/Apps/Edit
      //	POST-AJAX:	/Apps/Edit

      public ActionResult Edit(Guid? id)
      {
         var model = id == null || id.Value.IsEmpty() ? 
            new App() : 
            db.AppDal.PrimaryGet(id.Value);

         return PartialView("Edit", model);
      }

      [HttpPost]
      public ActionResult Edit(App model)
      {
         ThrowNotAjax();

         var t = APDBDef.UserInfo;

         //设置当前APP类型为项目管理系统
         model.AppType = ThisApp.ThisAPPType;

         if (model.AppId.IsEmpty())
         {
            model.AppId = Guid.NewGuid();

            db.AppDal.Insert(model);
         }
         else
         {
            db.AppDal.Update(model);
         }


         return Json(new
         {
            result = AjaxResults.Success,
            msg = "用户编辑成功!"
         });
      }



      public ActionResult GetRoleApps()
      {
         return null;
      }

   }

}