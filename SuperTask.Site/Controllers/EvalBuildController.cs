using Business;
using Business.Helper;
using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TheSite.EvalAnalysis;
using TheSite.Models;

namespace TheSite.Controllers
{

   public class EvalBuildController : BaseController
   {

      static APDBDef.EvalIndicationTableDef evi = APDBDef.EvalIndication;
      static APDBDef.EvalTableTableDef evt = APDBDef.EvalTable;
      static APDBDef.IndicationTableDef i = APDBDef.Indication;
      static APDBDef.EvalIndicationItemTableDef evii = APDBDef.EvalIndicationItem;
      static APDBDef.UserInfoTableDef u = APDBDef.UserInfo;


      // GET: EvalBuild/IndicationList
      // POST-Ajax: EvalBuild/IndicationList

      public ActionResult IndicationList()
      {
         return View();
      }

      [HttpPost]
      public ActionResult IndicationList(int current, int rowCount, AjaxOrder sort, string searchPhrase)
      {
         var eri = APDBDef.EvalResultItem;

         var query = APQuery.select(i.IndicationId, i.IndicationName, i.Description,
            i.IndicationType, i.IndicationStatus, i.CreateDate,
            u.UserName, eri.IndicationId
            )
            .from(i,
                  u.JoinLeft(u.UserId == i.CreaterId),
                  eri.JoinLeft(eri.IndicationId == i.IndicationId)
                  )
            .group_by(i.IndicationId, i.IndicationName, i.Description,
                     i.IndicationType, i.IndicationStatus, i.CreateDate,
                     u.UserName, eri.IndicationId)
            .primary(i.IndicationId)
            .skip((current - 1) * rowCount)
            .take(rowCount);
         //.where(i.IndicationStatus==IndicationKeys.EnabelStatus);


         //过滤条件
         //模糊搜索用户名、实名进行

         searchPhrase = searchPhrase.Trim();
         if (searchPhrase != "")
         {
            query.where_and(i.IndicationName.Match(searchPhrase));
         }


         //获得查询的总数量

         var total = Indication.ConditionQueryCount(null);


         //查询结果集

         var result = query.query(db, rd =>
         {
            var typeNum = i.IndicationType.GetValue(rd);
            var statusNum = i.IndicationStatus.GetValue(rd);

            return new
            {
               id = i.IndicationId.GetValue(rd),
               name = i.IndicationName.GetValue(rd),
               descripiton = i.Description.GetValue(rd),
               type = typeNum,
               satus = statusNum,
               typeName = IndicationKeys.GetIndicaitonTypeByValue(typeNum),
               statusName = statusNum == IndicationKeys.EnabelStatus ? "可用" : "禁用",
               creatDate = i.CreateDate.GetValue(rd),
               creator = u.UserName.GetValue(rd),
               isUsed = !eri.IndicationId.GetValue(rd).IsEmpty(),
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


      // GET: EvalBuild/IndicationEdit
      // POST-Ajax: EvalBuild/IndicationEdit

      public ActionResult IndicationEdit(Guid id)
      {
         Indication indication = null;
         if (!id.IsEmpty())
         {
            indication = db.IndicationDal.PrimaryGet(id);
         }

         return PartialView(indication);
      }

      [HttpPost]
      public ActionResult IndicationEdit(Indication indication)
      {
         var rs = indication.Validate();
         if (!rs.IsSuccess)
         {
            return Json(new
            {
               result = AjaxResults.Error,
               msg = rs.Msg
            });
         }

         if (indication.IndicationId.IsEmpty())
         {
            indication.IndicationId = Guid.NewGuid();
            indication.CreaterId = GetUserInfo().UserId;
            indication.CreateDate = DateTime.Now;
            indication.IndicationStatus = IndicationKeys.EnabelStatus;

            db.IndicationDal.Insert(indication);
         }
         else
         {
            if (indication.IsInUse())
            {
               return Json(new
               {
                  result = AjaxResults.Error,
                  msg = Errors.Indicaiton.IS_IN_USE
               });
            }

            db.IndicationDal.Update(indication);
         }

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Indicaiton.EDIT_SUCCESS
         });
      }

      [HttpPost]
      public ActionResult IndicationStatusChange(Guid id)
      {
         var result = Result.Initial();

         var indication = db.IndicationDal.PrimaryGet(id);
         if (indication == null)
         {
            result.IsSuccess = false;
            result.Msg = Errors.Indicaiton.IS_NULL;
         }

         if (indication.IsInUse())
         {
            result.IsSuccess = false;
            result.Msg = Errors.Indicaiton.IS_IN_USE;
         }

         var currentStatus = indication.IndicationStatus;
         currentStatus = currentStatus == IndicationKeys.DisableStatus ? IndicationKeys.EnabelStatus : IndicationKeys.DisableStatus;
         db.IndicationDal.UpdatePartial(id, new { IndicationStatus = currentStatus });


         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Indicaiton.EDIT_SUCCESS
         });
      }


      // GET: EvalBuild/EvalTableList
      // POST-Ajax: EvalBuild/EvalTableList

      public ActionResult EvalTableList()
      {
         var currentRole = RoleHelper.GetUserRoles(GetUserInfo().UserId, db);

         if (currentRole == null || currentRole.Count <= 0) throw new ApplicationException("当前用户没有设置任务角色！");

         ViewBag.CurrentRole = currentRole.First();

         return View();
      }

      [HttpPost]
      public ActionResult EvalTableList(int current, int rowCount, AjaxOrder sort, string searchPhrase)
      {
         var er = APDBDef.EvalResult;
         var query = APQuery.select(evt.TableId, evt.TableName, evt.Description, evt.FullScore,
            evt.TableType, evt.TableStatus, evt.CreateDate, u.UserName, er.TableId)
            .from(evt,
             u.JoinInner(u.UserId == evt.CreaterId),
             er.JoinLeft(er.TableId == evt.TableId)
            )
            .group_by(evt.TableId, evt.TableName, evt.Description, evt.FullScore,
                      evt.TableType, evt.TableStatus, evt.CreateDate, u.UserName, er.TableId)
            .primary(evt.TableId)
            .skip((current - 1) * rowCount);
         //.where(evt.IndicationStatus==IndicationKeys.EnabelStatus);


         //过滤条件
         //模糊搜索用户名、实名进行

         searchPhrase = searchPhrase.Trim();
         if (searchPhrase != "")
         {
            query.where_and(evt.TableName.Match(searchPhrase));
         }


         //获得查询的总数量

         var total = EvalTable.ConditionQueryCount(null); //db.ExecuteSizeOfSelect(query);


         //查询结果集

         var result = query.query(db, rd =>
         {
            var typeNum = evt.TableType.GetValue(rd);
            var statusNum = evt.TableStatus.GetValue(rd);

            return new
            {
               id = evt.TableId.GetValue(rd),
               name = evt.TableName.GetValue(rd),
               descripiton = evt.Description.GetValue(rd),
               fullScore = evt.FullScore.GetValue(rd),
               type = typeNum,
               status = statusNum,
               typeName = EvalTableKeys.GetTableTypeByValue(typeNum),
               satusName = EvalTableKeys.GetTableStatusByValue(statusNum),
               creatDate = evt.CreateDate.GetValue(rd),
               creator = u.UserName.GetValue(rd),
               isUsed = !er.TableId.GetValue(rd).IsEmpty(),
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


      // GET: EvalBuild/IndicationEdit
      // POST-Ajax: EvalBuild/IndicationEdit
      // POST-Ajax: EvalBuild/EvalTableStatusChange

      public ActionResult EvalTableEdit(Guid id)
      {
         var r = APDBDef.Role;

         EvalTable table = new EvalTable();
         if (!id.IsEmpty())
         {
            table = db.EvalTableDal.PrimaryGet(id);
         }

         //var roles = db.RoleDal.ConditionQuery(r.RoleType == RoleKeys.SystemType, null, null, null);

         //ViewBag.AccessorRoles = GetSelectRoleItem(roles, table.AccessorRoleIds);

         //ViewBag.MemberRoles = GetSelectRoleItem(roles.DeepClone(), table.MemberRoleIds);


         return PartialView(table);
      }

      [HttpPost]
      public ActionResult EvalTableEdit(EvalTable table)
      {
         var rs = table.Validate();
         if (!rs.IsSuccess)
         {
            return Json(new
            {
               result = AjaxResults.Error,
               msg = rs.Msg
            });
         }

         if (table.TableId.IsEmpty())
         {
            table.TableId = Guid.NewGuid();
            table.CreaterId = GetUserInfo().UserId;
            table.CreateDate = DateTime.Now;
            table.TableStatus = EvalTableKeys.ProcessStatus;

            db.EvalTableDal.Insert(table);
         }
         else
         {
            if (table.IsInUse())
            {
               return Json(new
               {
                  result = AjaxResults.Error,
                  msg = Errors.EvalTable.IS_IN_USE
               });
            }
            table.ModifierId = GetUserInfo().UserId;
            table.ModifyDate = DateTime.Now;

            db.EvalTableDal.Update(table);
         }

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Indicaiton.EDIT_SUCCESS
         });
      }

      [HttpPost]
      public ActionResult EvalTableStatusChange(Guid id)
      {
         var result = Result.Initial();

         var evalTable = db.EvalTableDal.PrimaryGet(id);
         if (evalTable == null)
         {
            result.IsSuccess = false;
            result.Msg = Errors.EvalTable.IS_NULL;
         }

         db.EvalTableDal.UpdatePartial(id, new { TableStatus = evalTable.PerviouslyTableStatus, PerviouslyTableStatus = evalTable.TableStatus });


         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Indicaiton.EDIT_SUCCESS
         });
      }


      // GET: EvalBuild/EvalTableManagement
      // POST-Ajax: EvalBuild/EvalIndicationBuild
      // POST-Ajax: EvalBuild/EvalIndicationRemove
      // POST-Ajax: EvalBuild/EvalTableDone
      // GET: EvalBuild/EvalTableBuildEdit
      // GET: EvalBuild/EvalTablePreview

      public ActionResult EvalTableBuild(Guid id)
      {
         var table = db.EvalTableDal.PrimaryGet(id);
         if (table.IsInUse())
         {
            throw new NullReferenceException("考核表已被使用");
         }

         var subQuery = APQuery.select(evi.IndicationId).from(evi).where(evi.TableId == id);
         var indications = db.IndicationDal.ConditionQuery(
            i.IndicationType == IndicationKeys.SubjectType &
            i.IndicationStatus == IndicationKeys.EnabelStatus &
            i.IndicationId.NotIn(subQuery), null, null, null);

         var evalIndications = APQuery.select(evi.Asterisk,i.IndicationName.As("IndicationName"),i.IndicationType)
                                      .from(evi, i.JoinInner(i.IndicationId==evi.IndicationId))
                                      .where(evi.TableId == id)
                                      .query(db, rd =>
                                       {
                                          var ev = new EvalIndication();
                                          evi.Fullup(rd, ev, false);                                        
                                          ev.IndicationName = i.IndicationName.GetValue(rd, "IndicationName");
                                          ev.IndicationType = i.IndicationType.GetValue(rd);
                                          return ev;
                                       })
                                      .ToList();

         var models = new EvalBuilderViewModel() { EvalTable = table, Indications = indications,EvalIndications=evalIndications };

         return View(models);
      }

      [HttpPost]
      public ActionResult EvalIndicationEdit(EvalIndication item)
      {
         if (item == null || !item.Validate().IsSuccess)
         {
            return Json(new
            {
               result = AjaxResults.Error,
               msg = Errors.Indicaiton.EDIT_FAIL
            });
         }

         if (item.Id.IsEmpty())
         {
            item.Id = Guid.NewGuid();
            db.EvalIndicationDal.Insert(item);
         }
         else
         {
            db.EvalIndicationDal.Update(item);
         }

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Indicaiton.EDIT_SUCCESS
         });
      }

      [HttpPost]
      public ActionResult EvalIndicationRemove(Guid tableId, Guid indicationId)
      {
         var exist = db.EvalIndicationDal.ConditionQueryCount(evi.TableId == tableId & evi.IndicationId == indicationId) > 0;
         if (exist)
         {
            var subQuery = APQuery.select(evi.Id)
                                        .from(evi,
                                              evt.JoinInner(evi.TableId == tableId),
                                              i.JoinInner(evi.IndicationId == indicationId));

            db.BeginTrans();

            try
            {
               db.EvalIndicationItemDal.ConditionDelete(evii.EvalIndicationId.In(subQuery));
               db.EvalIndicationDal.ConditionDelete(evi.Id.In(subQuery));

               db.Commit();
            }
            catch
            {
               db.Rollback();

               return Json(new
               {
                  result = AjaxResults.Error,
                  msg = Errors.Indicaiton.EDIT_FAIL
               });
            }
         }

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Indicaiton.EDIT_SUCCESS
         });
      }

      [HttpPost]
      public ActionResult EvalTableDone(Guid id)
      {
         //var evalIndications = db.EvalIndicationDal.ConditionQuery(evi.TableId == id, null, null, null);
         //var table = db.EvalTableDal.PrimaryGet(id);
         //var sumScore = BuilderManager.Builders[table.TableType].GetFullScore(evalIndications);

         //if (table.FullScore != sumScore)
         //{
         //   return Json(new
         //   {
         //      result = AjaxResults.Error,
         //      msg = Errors.EvalTable.EVALINDICATION_SUMSCORE_SHOULD_SAME_WITH_TABLE_SCORE
         //   });
         //}


         db.EvalTableDal.UpdatePartial(id, new { TableStatus = EvalTableKeys.DoneStatus });


         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Indicaiton.EDIT_SUCCESS
         });
      }

      public ActionResult EvalTableBuildEdit(Guid id)
      {
         db.EvalTableDal.UpdatePartial(id, new { TableStatus = EvalTableKeys.ProcessStatus });

         return RedirectToAction("EvalTableBuild", new { id = id });
      }

      public ActionResult EvalTablePreview(Guid id, Guid roleId)
      {
         //var r = APDBDef.Role;

         //var roles = db.RoleDal.ConditionQuery(r.RoleType == RoleKeys.SystemType, null, null, null);
         //var table = db.EvalTableDal.PrimaryGet(id);

         //if (roles == null || roles.Count <= 0) throw new NullReferenceException();
         //if (table == null || string.IsNullOrEmpty(table.AccessorRoleIds)) throw new NullReferenceException();

         //table.BuildAccessorRoles(roles);

         //roleId = roleId.IsEmpty() ? table.AccessorRoleIds.Split(',').First().ToGuid(Guid.Empty) : roleId;

         //var result = APQuery
         //   .select(evi.Asterisk, evii.Asterisk, i.IndicationName.As("IndicationName"), i.Description)
         //   .from(evii,
         //         evi.JoinInner(evi.Id == evii.EvalIndicationId & evi.TableId == id & evi.AccessorRoleId == roleId),
         //         i.JoinInner(evi.IndicationId == i.IndicationId))
         //         .query(db, rd =>
         //          {
         //             var item = new EvalIndicationItem();
         //             evii.Fullup(rd, item, false);
         //             var evalIndication = new EvalIndication();
         //             evi.Fullup(rd, evalIndication, false);
         //             evalIndication.IndicationName = i.IndicationName.GetValue(rd, "IndicationName");

         //             return new
         //             {
         //                EvalIndication = evalIndication,
         //                EvalIndicationItem = item
         //             };
         //          }).ToList();

         //var temp = new List<EvalIndication>();

         //foreach (var item in result)
         //{
         //   var evalIndication = item.EvalIndication;

         //   if (!temp.Contains(evalIndication))
         //      temp.Add(evalIndication);
         //   else
         //      evalIndication = temp.Find(x => x.Id == evalIndication.Id);

         //   if (evalIndication.Items == null)
         //      evalIndication.Items = new List<EvalIndicationItem>();
         //   evalIndication.Items.Add(item.EvalIndicationItem);
         //}

         //return View(new PreviewViewModel
         //{
         //   AccessorRoles = table.AccessorRoles,
         //   EvalTable = table,
         //   EvalIndications = temp,
         //   CurrentAccessorRole = roles.Find(role => role.RoleId == roleId)
         //});

         return null;
      }

   }

}