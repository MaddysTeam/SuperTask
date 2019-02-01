//using Business;
//using Symber.Web.Data;
//using System;
//using System.Web.Mvc;
//using TheSite.EvalAnalysis;
//using TheSite.Models;
//using System.Linq;
//using System.Collections.Generic;
//using Business.Helper;

//namespace TheSite.Controllers
//{

//   public class EvalTableGroupController : BaseController
//   {

//      static APDBDef.EvalTableGroupTableDef etg = APDBDef.EvalTableGroup;
//      static APDBDef.EvalTableTableDef et = APDBDef.EvalTable;
//      static APDBDef.EvalTableGroupItemTableDef etgi = APDBDef.EvalTableGroupItem;
//      static APDBDef.EvalPeriodTableDef ep = APDBDef.EvalPeriod;
//      static APDBDef.RoleTableDef r = APDBDef.Role;

//      public ActionResult Index(Guid? periodId)
//      {
//         if (periodId == null || periodId.Value.IsEmpty())
//         {
//            var currentPeriods = EvalPeriod.GetCurrentPeriod(db);
//            if (currentPeriods.Count > 0)
//            {
//               return RedirectToAction("List", new { currentPeriods.First().PeriodId });
//            }
//         }

//         return RedirectToAction("List", new { periodId });
//      }


//      //	GET: EvalTableGroup/List
//      //	POST-Ajax: EvalTableGroup/List

//      public ActionResult List(Guid periodId)
//      {
//         ViewBag.Periods = EvalPeriod.ConditionQuery(null, ep.BeginDate.Desc);

//         return View();
//      }

//      [HttpPost]
//      public ActionResult List(Guid periodId, int current, int rowCount, AjaxOrder order, string searchPhrase)
//      {
//         var query = APQuery.select(etg.TableGroupId, etg.TableGroupName, etgi.GroupItemId.Count().As("tableCount"), ep.Name.As("periodName"))
//                         .from(etg,
//                              etgi.JoinLeft(etg.TableGroupId == etgi.TableGroupId),
//                              ep.JoinLeft(etg.PeriodId == ep.PeriodId))
//                         .group_by(etgi.TableGroupId, etg.TableGroupId, etg.TableGroupName, ep.Name);

//         //如果周期id等于空则不用过滤周期

//         //if(!periodId.IsEmpty())
//         //   query.where(etg.PeriodId == periodId);



//         //过滤条件
//         //模糊搜索用户名、实名进行

//         searchPhrase = searchPhrase.Trim();
//         if (searchPhrase != "")
//            query.where_and(etg.TableGroupName.Match(searchPhrase));


//         //获得查询的总数量

//         var total = db.ExecuteSizeOfSelect(query);


//         //查询结果集

//         var result = query.query(db, r =>
//         {
//            return new
//            {
//               id = etg.TableGroupId.GetValue(r),
//               groupName = etg.TableGroupName.GetValue(r),
//               tableCount =r["tableCount"],
//               periodId = periodId,
//               periodName= ep.Name.GetValue(r, "periodName")
//            };
//         }).ToList();

//         return Json(new
//         {
//            rows = result,
//            current,
//            rowCount,
//            total
//         });
//      }


//      //	GET: EvalTableGroup/EditTablePropertion
//      //	POST-Ajax: EvalTableGroup/EditTablePropertion

//      public ActionResult EditTablePropertion(Guid groupId)
//      {
//         var result = APQuery.select(etgi.Asterisk, et.TableName, ep.Name.As("periodName"))
//                             .from(etgi,
//                                    etg.JoinInner(etg.TableGroupId == etgi.TableGroupId),
//                                    et.JoinInner(et.TableId == etgi.TableId),
//                                    ep.JoinInner(ep.PeriodId == etg.PeriodId))
//                              .where(etgi.TableGroupId== groupId)
//                              .query(db, r => {
//                                 var item = new EvalTableGroupItem();
//                                 etgi.Fullup(r, item, false);
//                                 item.TableName = et.TableName.GetValue(r);
//                                 item.PeriodName = ep.Name.GetValue(r, "periodName");

//                                 return item;
//                              }).ToList();


//         return PartialView(result);
//      }

//      [HttpPost]
//      public ActionResult EditTablePropertion(List<EvalTableGroupItem> items)
//      {
//         if (items == null && items.Count <= 0) throw new ApplicationException(Errors.EvalTableGroup.NOT_EXIST_ANY_TABLES);

//         var sumPropertion = items.Sum(x => x.Propertion);
//         if (sumPropertion != 100)
//         {
//            return Json(new
//            {
//               result = AjaxResults.Error,
//               msg = Errors.EvalTableGroup.SUM_OF_TALBES_PROPERTION_MUST_BE_100
//            });
//         }

//         foreach (var item in items)
//         {
//            db.EvalTableGroupItemDal.UpdatePartial(item.GroupItemId, new { Propertion = item.Propertion });
//         }

//         return Json(new
//         {
//            result = AjaxResults.Success,
//            msg = "操作成功"
//         });
//      }


//      //	GET: EvalTableGroup/Edit
//      //	POST-Ajax: EvalTableGroup/Edit
//      //	POST-Ajax: EvalTableGroup/GetEvalTables

//      public ActionResult Edit(Guid? id, Guid? periodId)
//      {
//         if (periodId == null) throw new ApplicationException(Errors.Eval.NOT_IN_PERIOD);

//         EvalTableGroup group = null;
//         if (id == null || EvalTableGroup.PrimaryGet(id.Value) == null)
//            group = new EvalTableGroup();
//         else
//            group = db.EvalTableGroupDal.PrimaryGet(id.Value);

//         var evalTables = EvalPeriodTable.GetAllAvaliableEvalPeriodTables(db);
//         if (evalTables.Count <= 0) throw new ApplicationException(Errors.EvalTable.WITHOUT_ANY_TABLES);

//         ViewBag.AllTables = evalTables.Where(t => t.PeriodId == periodId).ToList();


//         var selectTables = EvalTableGroupItem.ConditionQuery(etgi.TableGroupId==group.TableGroupId,null);
//         var tableIds = string.Empty;
//         if (selectTables.Count>0)
//            tableIds = string.Join(",", selectTables.Select(x => x.TableId).ToArray());

//         ViewBag.TableIds = tableIds;

//         ViewBag.Periods = EvalPeriod.ConditionQuery(null, ep.BeginDate.Desc);

//         ViewBag.TargetRoles = Role.ConditionQuery(r.RoleType==RoleKeys.SystemType,null);

//         return PartialView(group);
//      }


//      [HttpPost]
//      public ActionResult Edit(EvalTableGroup group)
//      {
//         db.BeginTrans();

//         try
//         {
//            if (group.TableGroupId.IsEmpty())
//            {
//               group.TableGroupId = Guid.NewGuid();

//               db.EvalTableGroupDal.Insert(group);
//            }
//            else
//            {
//               db.EvalTableGroupDal.Update(group);
//            }

//            db.EvalTableGroupItemDal.ConditionDelete(etgi.TableGroupId == group.TableGroupId);

//            var tableIds = group.TableIds;
//            if (!string.IsNullOrEmpty(tableIds))
//            {
//               var tableIdList = tableIds.Split(',').ToList();
//               foreach (var item in tableIdList)
//               {
//                  db.EvalTableGroupItemDal.Insert(new EvalTableGroupItem
//                  {
//                     GroupItemId = Guid.NewGuid(),
//                     TableGroupId = group.TableGroupId,
//                     TableId = item.ToGuid(Guid.Empty)
//                  });
//               }
//            }

//            db.Commit();
//         }
//         catch
//         {
//            db.Rollback();
//         }

//         return Json(new
//         {
//            result = AjaxResults.Success,
//            msg = "操作成功"
//         });
//      }

//      [HttpPost]
//      public ActionResult GetEvalTablesByPeriodId(Guid periodId)
//      {
//         var evalTables = EvalPeriodTable.GetAllAvaliableEvalPeriodTables(db)
//            .Where(x=>x.PeriodId==periodId)
//            .ToList();

//         return Json(new
//         {
//            rows = evalTables
//         });
       
//      }


//      [HttpPost]
//      public ActionResult Remove(Guid id)
//      {
//         db.EvalTableGroupDal.ConditionDelete(etg.TableGroupId == id);

//         return Json(new
//         {
//            result = AjaxResults.Success,
//            msg = "操作成功"
//         });
//      }

//   }

//}