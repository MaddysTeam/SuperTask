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

      static APDBDef.MileStoneTableDef m = APDBDef.MileStone;


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

         var query = APQuery.select(m.StoneId, m.StoneName, m.StoneStatus,m.StoneType)
            .from(m)
            .primary(m.StoneId)
            .skip((current - 1) * rowCount)
            .take(rowCount);


         //过滤条件
         //模糊搜索用户名、实名进行

         searchPhrase = searchPhrase.Trim();
         if (searchPhrase != "")
         {
            query.where_and(m.StoneName.Match(searchPhrase));
         }


         //获得查询的总数量

         var total = db.ExecuteSizeOfSelect(query);


         //查询结果集
         var result = query.query(db, rd =>
         {
            return new
            {
               id = m.StoneId.GetValue(rd),
               name=m.StoneName.GetValue(rd),
               type= MilestoneKeys.GetTypeKeyById(m.StoneType.GetValue(rd)),
               status= MilestoneKeys.GetStatusKeyById(m.StoneStatus.GetValue(rd))
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


      // GET: MileStone/ChooseExceptDefault

      public ActionResult ChooseExceptDefault()
      {
         var milestones = db.MileStoneDal.ConditionQuery(m.StoneType!= MilestoneKeys.DefaultType, null, null, null);

         return PartialView("_milestones", milestones);
      }


      // GET: MileStone/Edit
      // Post-ajax: MileStone/Edit
      // Post-ajax: Project/Details

      public ActionResult Edit(Guid id)
      {
         var mileStone = APBplDef.MileStoneBpl.PrimaryGet(id);
         return PartialView(mileStone);
      }

      [HttpPost]
      public ActionResult Edit(MileStone mileStone)
      {
         if (mileStone.StoneId.IsEmpty())
         {
            mileStone.StoneId = Guid.NewGuid();
            APBplDef.MileStoneBpl.Insert(mileStone);
         }
         else
         {
            APBplDef.MileStoneBpl.Update(mileStone);
         }

         return Json(new {
            result = AjaxResults.Success,
            msg = Success.MileStone.EDITSUCCESS
         });
      }




   }

}