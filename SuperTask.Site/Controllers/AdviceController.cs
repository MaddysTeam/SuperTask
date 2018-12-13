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

   public class AdviceController : BaseController
   {

      APDBDef.AdviceTableDef a = APDBDef.Advice;
      APDBDef.UserInfoTableDef u = APDBDef.UserInfo;
      APDBDef.AdviceSupportTableDef asu = APDBDef.AdviceSupport;

      // GET: Advice/search
      // Post-ajax: Advice/search

      public ActionResult Search()
      {
         return View();
      }

      [HttpPost]
      public ActionResult Search(int current, int rowCount, AjaxOrder sort, string searchPhrase)
      {
         ThrowNotAjax();

         var currentUserId = GetUserInfo().UserId;
         var query = APQuery.select(a.Asterisk, u.UserName, asu.SupportId.As("supportId"))
            .from(a,
                  u.JoinLeft(a.CreatorId == u.UserId),
                  asu.JoinLeft(asu.AdviceId == a.AdviceId & asu.SupporterId == currentUserId)
                  )
            .primary(a.AdviceId)
            .where(a.CreatorId == currentUserId | a.Status != AdviceKeys.AdviceSaveGuid)
            .skip((current - 1) * rowCount)
            .take(rowCount)
            .order_by(a.CreateDate.Desc);


         //过滤条件
         //模糊搜索用户名、实名进行

         searchPhrase = searchPhrase.Trim();
         if (searchPhrase != "")
         {
            query.where_and(a.Title.Match(searchPhrase)
               | a.Content.Match(searchPhrase)
               | a.Reason.Match(searchPhrase));
         }


         //排序条件表达式

         if (sort != null)
         {
            switch (sort.ID)
            {
               case "title": query.order_by(sort.OrderBy(a.Title)); break;
               case "isAdobt": query.order_by(sort.OrderBy(a.IsAdopt)); break;
               case "createDate": query.order_by(sort.OrderBy(a.CreateDate)); break;
            }
         }


         //获得查询的总数量

         var total = db.ExecuteSizeOfSelect(query);


         //查询结果集

         var result = query.query(db, rd =>
         {
            return new
            {
               id = a.AdviceId.GetValue(rd),
               title = a.Title.GetValue(rd),
               content = a.Content.GetValue(rd).Ellipsis(),
               reason = a.Reason.GetValue(rd).Ellipsis(),
               isAdobt = a.IsAdopt.GetValue(rd),
               isSupport = asu.SupportId.GetValue(rd, "supportId") != Guid.Empty,
               status = AdviceKeys.GetStatusKeyByValue(a.Status.GetValue(rd)),
               supportCount = a.SupportCount.GetValue(rd),
               creatorId = a.CreatorId.GetValue(rd),
               createDate = a.CreateDate.GetValue(rd),
               creator = u.UserName.GetValue(rd),
               type = AdviceKeys.GetTypeKeyByValue(a.AdviceType.GetValue(rd))
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

      //	GET:	/Advice/Edit
      //	POST-AJAX:	/Advice/Edit

      public ActionResult Edit(Guid id)
      {
         var model = db.AdviceDal.PrimaryGet(id);

         return PartialView("Edit", model);
      }

      [HttpPost]
      public ActionResult Edit(Advice model)
      {
         if (null == model)
            return Json(new
            {
               result = AjaxResults.Error,
               msg = Errors.Advice.NOT_ALLOWED_NULL
            });

         if (string.IsNullOrEmpty(model.Title))
            return Json(new
            {
               result = AjaxResults.Error,
               msg = Errors.Advice.NOT_ALLOWED_TITLE_NULL
            });

         if (string.IsNullOrEmpty(model.Content))
            return Json(new
            {
               result = AjaxResults.Error,
               msg = Errors.Advice.NOT_ALLOWED_CONTENT_NULL
            });

         var t = APDBDef.UserInfo;

         if (model.AdviceId.IsEmpty())
         {
            model.AdviceId = Guid.NewGuid();
            model.Status = AdviceKeys.AdviceSaveGuid;
            model.CreatorId = GetUserInfo().UserId;
            model.CreateDate = DateTime.Now;

            db.AdviceDal.Insert(model);
         }
         else
         {
            model.ModifierId = GetUserInfo().UserId;
            model.ModifyDate = DateTime.Now;

            db.AdviceDal.Update(model);
         }

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Advice.EDITSUCCESS
         });
      }

      //	POST-AJAX:	/Advice/Approve

      [HttpPost]
      public ActionResult Approve(Guid id)
      {
         var model = GetAdviceById(id);
         if (null != model)
         {
            model.IsAdopt = true;
            model.Reason = "";

            Edit(model);
         }

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Advice.EDITSUCCESS
         });
      }

      //	GET:	/Advice/Deny

      public ActionResult Deny(Guid id)
      {
         var model = GetAdviceById(id);
         model.IsAdopt = false;

         ViewBag.isDeny = true;

         return PartialView("Edit", model);
      }

      //	GET:	/Advice/Detial

      public ActionResult Detial(Guid id)
      {
         var model = GetAdviceById(id);
         model.IsAdopt = false;

         ViewBag.isDeny = !model.IsAdopt;
         ViewBag.isDetail = true;

         return PartialView("Edit", model);
      }

      //	Post:	/Advice/Support

      [HttpPost]
      public ActionResult Support(Guid id)
      {
         var model = GetAdviceById(id);
         var userId = GetUserInfo().UserId;
         var isSupport = db.AdviceSupportDal.ConditionQueryCount(asu.AdviceId == id & asu.SupporterId == userId) > 0;
         if (isSupport)
            return Json(new { });


         db.AdviceSupportDal.Insert(new AdviceSupport { SupportId = Guid.NewGuid(), AdviceId = id, SupporterId = userId });

         model.SupportCount += 1;

         return Edit(model);
      }


      private Advice GetAdviceById(Guid id)
      {
         if (id == Guid.Empty) throw new ArgumentException();

         var model = db.AdviceDal.PrimaryGet(id);
         if (null == model) throw new NullReferenceException();

         return model;
      }

   }

}