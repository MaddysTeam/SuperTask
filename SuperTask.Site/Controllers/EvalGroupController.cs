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

   public class EvalGroupController : BaseController
   {

      static APDBDef.EvalGroupTableDef eg = APDBDef.EvalGroup;
      static APDBDef.UserInfoTableDef u = APDBDef.UserInfo;
      static APDBDef.EvalTableTableDef et = APDBDef.EvalTable;
      static APDBDef.EvalGroupAccessorTableDef ega = APDBDef.EvalGroupAccessor;
      static APDBDef.EvalAccessorTargetTableDef eat = APDBDef.EvalAccessorTarget;
      static APDBDef.EvalTargetTablePropertionTableDef ettp = APDBDef.EvalTargetTablePropertion;
      static APDBDef.EvalGroupMemberTableDef egm = APDBDef.EvalGroupMember;

      //	GET: EvalGroup/List
      //	POST-Ajax: EvalGroup/List

      public ActionResult List()
      {
         return View();
      }

      [HttpPost]
      public ActionResult List(int current, int rowCount, AjaxOrder sort, string searchPhrase)
      {
         var result = EvalGroup.GetAll();

         if (result.Count > 0)
            result = result.Skip((current - 1) * (rowCount - 1)).Take(rowCount).ToList();


         return Json(new
         {
            rows = result,
            current,
            rowCount,
            total = result.Count
         });
      }


      //	GET: EvalGroup/Edit
      //	POST-Ajax: EvalGroup/Edit

      public ActionResult Edit(Guid? id)
      {
         var group = id == null ? null : EvalGroup.PrimaryGet(id.Value);

         return PartialView(group);
      }

      [HttpPost]
      public ActionResult Edit(EvalGroup group)
      {
         if (group.GroupId.IsEmpty())
         {
            group.GroupId = Guid.NewGuid();

            db.EvalGroupDal.Insert(group);
         }
         else
         {
            db.EvalGroupDal.Update(group);
         }

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.EvalGroup.EDIT_SUCCESS
         });
      }

      [HttpPost]
      public ActionResult BindGroupMember(Guid groupId, string userIds)
      {
         var memberIds = userIds.Split(',').Cast<Guid>().ToArray();

         if (!string.IsNullOrEmpty(userIds))
         {
            db.EvalGroupMemberDal.ConditionDelete(egm.GroupId == groupId & egm.MemberId.In(memberIds));
         }

         foreach (var item in memberIds)
         {
            db.EvalGroupMemberDal.Insert(new EvalGroupMember { GroupMemberId = Guid.NewGuid(), MemberId = item, GroupId = groupId });
         }

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.EvalGroup.BIND_MEMBER_SUCCESS
         });
      }


      //	GET: EvalGroup/BindGroupAccessors
      //	POST-Ajax: EvalGroup/BindGroupAccessors

      public ActionResult BindGroupAccessors(Guid groupId)
      {
         var period = EvalPeriod.GetCurrentPeriod(db).FirstOrDefault();

         var results = APQuery.select(ega.GroupAccessorId, u.UserId.As("userId"), u.UserName).from(u, ega.JoinLeft(u.UserId == ega.AccessorId & ega.GroupId == groupId))
            .query(db, r => new EvalGroupAccessor
            {
               GroupAccessorId = ega.GroupAccessorId.GetValue(r),
               AccessorId = u.UserId.GetValue(r, "userId"),
               AccessorName = u.UserName.GetValue(r),
               GroupId = groupId
            }).ToList();

         return PartialView(results);
      }

      [HttpPost]
      public ActionResult BindGroupAccessors(Guid groupId, string accessorIds)
      {
         if (string.IsNullOrEmpty(accessorIds))
         {
            return Json(new
            {
               result = AjaxResults.Success,
               msg = Errors.EvalGroup.BIND_ACCESSOR_FAIL
            });
         }

         var period = EvalPeriod.GetCurrentPeriod(db).FirstOrDefault();
         var allAccessors = EvalGroupAccessor.GetAll();
         var ids = new List<Guid>();

         foreach (var id in accessorIds.Split(','))
         {
            ids.Add(id.ToGuid(Guid.Empty));
         }

         var subquery = APQuery.select(eat.TargetId).from(eat).where(eat.AccessorId.In(ids.ToArray()));
         var members = db.EvalGroupMemberDal.ConditionQuery(egm.GroupId == groupId & egm.MemberId.NotIn(subquery), null, null, null);
         var groupAccessorTargets = db.EvalAccessorTargetDal.ConditionQuery(eat.TargetId == groupId & eat.PeriodId == period.PeriodId, null, null, null);

         db.EvalGroupAccessorDal.ConditionDelete(ega.GroupId == groupId);

         foreach (var accessorId in ids)
         {
            if (!allAccessors.Exists(a => a.AccessorId == accessorId & a.GroupId == groupId))
               db.EvalGroupAccessorDal.Insert(new EvalGroupAccessor { GroupAccessorId = Guid.NewGuid(), AccessorId = accessorId, GroupId = groupId });

            //放入大组
            if (!allAccessors.Exists(a => a.AccessorId == accessorId & a.GroupId == Guid.Empty))
               db.EvalGroupAccessorDal.Insert(new EvalGroupAccessor { GroupAccessorId = Guid.NewGuid(), AccessorId = accessorId });

            // 新增考核人和组之间的绑定
            if (!groupAccessorTargets.Exists(e => e.AccessorId == accessorId))
               db.EvalAccessorTargetDal.Insert(new EvalAccessorTarget { AccessorTargetId = Guid.NewGuid(), TargetId = groupId, AccessorId = accessorId, PeriodId = period.PeriodId });

            // 新增考核人和被考核人的关系
            //foreach (var member in members)
            //{
            //   db.EvalAccessorTargetDal.Insert(new EvalAccessorTarget { AccessorTargetId = Guid.NewGuid(), TargetId = member.MemberId, AccessorId = accessorId, PeriodId = period.PeriodId });
            //}
         }

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.EvalGroup.BIND_MEMBER_SUCCESS
         });
      }


      //	GET: EvalGroup/BindGroupTablesWeightAndPropertion

      public ActionResult BindGroupTablesWeightAndPropertion(Guid groupId)
      {
         var period = EvalPeriod.GetCurrentPeriod(db).FirstOrDefault();

         var subquery = APQuery.select(eat.TableId).from(eat).where(eat.TargetId == groupId);
         var tables = db.EvalTableDal.ConditionQuery(et.TableStatus != EvalTableKeys.DisableStatus & et.TableId.NotIn(subquery), null, null, null); //左侧可用考核列表
         var tablePropertion = db.EvalTargetTablePropertionDal.ConditionQuery(ettp.TargetId == groupId, null, null, null);
         var accessorTargets = APQuery.select(eat.Asterisk, u.UserName.As("accessor"), eg.GroupName.As("target"), et.TableId.As("tableId"), et.TableName.As("tableName"))
                               .from(eat,
                                       et.JoinLeft(et.TableId == eat.TableId),
                                       u.JoinInner(u.UserId == eat.AccessorId),
                                       eg.JoinInner(eg.GroupId == eat.TargetId)
                                       )
                               .where(eat.TargetId == groupId & eat.PeriodId == period.PeriodId)
                               .query(db, r =>
                               {
                                  var tgt = new EvalAccessorTarget();
                                  eat.Fullup(r, tgt, false);
                                  tgt.TargetName = eg.GroupName.GetValue(r, "target");
                                  tgt.AccessorName = u.UserName.GetValue(r, "accessor");
                                  tgt.TableName = et.TableName.GetValue(r, "tableName");
                                  return tgt;
                               }).ToList();

         var currentTarget = accessorTargets.Find(x => x.TargetId == groupId);

         return View("TargetMemberEdit", new EvalTargetEditViewModels { Tables = tables, AccessorsAndTargets = accessorTargets, TablePropertion = tablePropertion, CurrentTarget = currentTarget });
      }


      //	GET: EvalGroup/AccessorList
      //	POST-Ajax: EvalGroup/AccessorList

      public ActionResult AccessorList()
      {
         return View();
      }

      [HttpPost]
      public ActionResult AccessorList(int current, int rowCount, AjaxOrder sort, string searchPhrase)
      {
         var query = APQuery.select(ega.GroupAccessorId, ega.GroupId, ega.ModifyDate, ega.AccessorId, u.UserName.As("UserName"))
                            .from(ega
                                  , u.JoinInner(u.UserId == ega.AccessorId))
                            .where(ega.GroupId == Guid.Empty); //默认查看大组内的考核人

         query.primary(ega.AccessorId)
            //.order_by(ega.CreateDate.Desc)
            .skip((current - 1) * rowCount)
             .take(rowCount);


         //获得查询的总数量

         var total = db.ExecuteSizeOfSelect(query);


         var result = query.query(db,
            rd => new
            {

               id = ega.GroupAccessorId.GetValue(rd),
               name = u.UserName.GetValue(rd, "UserName"),
               modifyDate = ega.ModifyDate.GetValue(rd),
               accessorId = ega.AccessorId.GetValue(rd),

            }).ToList();


         return Json(new
         {
            rows = result,
            current,
            rowCount,
            total
         });
      }


      //	GET: EvalGroup/TargetMemberList
      //	POST-Ajax: EvalGroup/TargetMemberList

      public ActionResult TargetMemberList(Guid groupId)
      {
         return View();
      }

      [HttpPost]
      public ActionResult TargetMemberList(Guid groupId, int current, int rowCount, AjaxOrder sort, string searchPhrase)
      {
         var query = APQuery.select(eat.TargetId, u.UserName.As("UserName"))
                   .from(eat
                         , u.JoinInner(u.UserId == eat.TargetId))
                   .group_by(eat.TargetId, u.UserName);

         query.primary(eat.AccessorTargetId)
            .skip((current - 1) * rowCount)
             .take(rowCount);


         //获得查询的总数量

         var total = db.ExecuteSizeOfSelect(query);


         var result = query.query(db, rd => new
         {
            id = eat.TargetId.GetValue(rd),
            name = u.UserName.GetValue(rd, "UserName"),
         });


         return Json(new
         {
            rows = result,
            current,
            rowCount,
            total
         });

      }


      //	GET: EvalGroup/AccessorEdit
      //	POST-Ajax: EvalGroup/AccessorEdit

      public ActionResult AccessorEdit(Guid groupAccessorId)
      {
         var tables = db.EvalTableDal.ConditionQuery(et.TableStatus != EvalTableKeys.DisableStatus, null, null, null);
         var users = db.UserInfoDal.ConditionQuery(null, null, null, null);
         var accessor = new EvalGroupAccessor();

         if (!groupAccessorId.IsEmpty())
         {
            accessor = APQuery.select(ega.Asterisk, u.UserName)
                                  .from(ega, u.JoinInner(u.UserId == ega.AccessorId))
                                  .where(ega.GroupAccessorId == groupAccessorId)
                                  .query(db, rd =>
                                  {
                                     var acser = new EvalGroupAccessor();
                                     ega.Fullup(rd, acser, false);
                                     return acser;
                                  }
                                  ).FirstOrDefault();
         }

         ViewBag.Users = SelectListHelper.GetSelectItems(users, "UserName", "UserId");

         return PartialView(accessor);
      }

      [HttpPost]
      public ActionResult AccessorEdit(EvalGroupAccessor accessor)
      {
         if (accessor == null)
            return Json(new
            {
               result = AjaxResults.Error,
               msg = Errors.EvalGroup.NOT_ALLOWED_ACCESSOR_NULL
            });

         var isExists = db.EvalGroupAccessorDal.ConditionQueryCount(ega.AccessorId == accessor.AccessorId) > 0;
         if (isExists)
            return Json(new
            {
               result = AjaxResults.Error,
               msg = Errors.EvalGroup.ACCESSOR_ISEXISTS
            });

         accessor.ModifyDate = DateTime.Now;
         if (accessor.GroupAccessorId.IsEmpty() && !isExists)
         {
            accessor.GroupAccessorId = Guid.NewGuid();

            db.EvalGroupAccessorDal.Insert(accessor);
         }
         else
         {
            db.EvalGroupAccessorDal.Update(accessor);
         }

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.EvalGroup.ACCESSOR_EDIT_SUCCESS
         });
      }


      //	GET: EvalGroup/BindTargets
      //	POST-Ajax: EvalGroup/BindTargets

      public ActionResult BindTargets(Guid accessorId)
      {
         var period = EvalPeriod.GetCurrentPeriod(db).FirstOrDefault();
         if (accessorId.IsEmpty())
            throw new ApplicationException();

         var at = APDBDef.EvalAccessorTarget;

         var reuslts = APQuery.select(at.AccessorTargetId, at.TargetId, u.UserName, u.UserId.As("userId"))
            .from(u, at.JoinLeft(at.TargetId == u.UserId
                               & at.AccessorId == accessorId
                               & at.PeriodId == period.PeriodId
                               & at.TableId == Guid.Empty // table id 为空 表示 accessor 和 target 之间的关系
                                                          // & at.GroupId == EvalGroupConfig.DefaultGroupId.ToGuid(Guid.Empty)
                               ))
            .query(db, r => new EvalAccessorTarget { AccessorTargetId = at.AccessorTargetId.GetValue(r), TargetId = u.UserId.GetValue(r, "userId"), TargetName = u.UserName.GetValue(r) })
            .ToList();

         return PartialView(reuslts);
      }

      [HttpPost]
      public ActionResult BindTargets(Guid accessorId, string targetIds)
      {
         var at = APDBDef.EvalAccessorTarget;
         var period = EvalPeriod.GetCurrentPeriod(db).FirstOrDefault();

         if (string.IsNullOrEmpty(targetIds))
            return Json(new
            {
               result = AjaxResults.Success,
               msg = Errors.EvalGroup.BIND_TARGET_FAIL
            });


         var accessorTargets = EvalAccessorTarget.ConditionQuery(at.AccessorId == accessorId, null);

         db.BeginTrans();

         try
         {
            if (accessorTargets.Count > 0)
               EvalAccessorTarget.ConditionDelete(at.AccessorId == accessorId);

            foreach (var item in targetIds.Split(','))
            {
               var targetId = item.ToGuid(Guid.Empty);
               db.EvalAccessorTargetDal.Insert(
                 new EvalAccessorTarget
                 {
                    AccessorId = accessorId,
                    AccessorTargetId = Guid.NewGuid(),
                    //GroupId = EvalGroupConfig.DefaultGroupId.ToGuid(Guid.Empty),
                    EvalType = EvalKeys.SubjectType,
                    ModifyDate = DateTime.Now,
                    PeriodId = period.PeriodId,
                    TargetId = targetId
                 });
            }

            db.Commit();
         }
         catch
         {
            db.Rollback();
         }


         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.EvalGroup.BIND_TARGET_SUCCESS
         });
      }


      //	POST-Ajax: EvalGroup/RemoveAccessor

      [HttpPost]
      public ActionResult RemoveAccessor(Guid groupAccessorId)
      {
         var groupAccessor = EvalGroupAccessor.PrimaryGet(groupAccessorId);
         if (groupAccessor != null)
         {
            db.EvalAccessorTargetDal.ConditionDelete(eat.AccessorId == groupAccessor.AccessorId);
            db.EvalGroupAccessorDal.ConditionDelete(ega.GroupAccessorId == groupAccessorId);
         }

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.EvalGroup.ACCESSOR_EDIT_SUCCESS
         });
      }


      //	GET: EvalGroup/TargetMemberEdit
      //	POST-Ajax: EvalGroup/TargetMemberEdit

      public ActionResult TargetMemberEdit(Guid targetId)
      {
         var tu = APDBDef.UserInfo.As("targetUserInfo");
         var period = EvalPeriod.GetCurrentPeriod(db).FirstOrDefault();

         var subquery = APQuery.select(eat.TableId).from(eat).where(eat.TargetId == targetId);
         var tables = db.EvalTableDal.ConditionQuery(et.TableStatus != EvalTableKeys.DisableStatus & et.TableId.NotIn(subquery), null, null, null); //左侧可用考核列表
         var tablePropertion = db.EvalTargetTablePropertionDal.ConditionQuery(ettp.TargetId == targetId & ettp.PeriodId == period.PeriodId, null, null, null);
         var accessorTargets = APQuery.select(eat.Asterisk, u.UserName.As("accessor"), tu.UserName.As("target"), et.TableId.As("tableId"), et.TableName.As("tableName")) // 评审人
                               .from(eat,
                                       et.JoinLeft(et.TableId == eat.TableId),
                                       u.JoinInner(u.UserId == eat.AccessorId),
                                       tu.JoinInner(tu.UserId == eat.TargetId)
                                       )
                               .where(eat.TargetId == targetId & eat.PeriodId == period.PeriodId)
                               .query(db, r =>
                               {
                                  var tgt = new EvalAccessorTarget();
                                  eat.Fullup(r, tgt, false);
                                  tgt.TargetName = tu.UserName.GetValue(r, "target");
                                  tgt.AccessorName = u.UserName.GetValue(r, "accessor");
                                  tgt.TableName = et.TableName.GetValue(r, "tableName");
                                  return tgt;
                               }).ToList();

         var currentTarget = accessorTargets.Find(x => x.TargetId == targetId);

         return View(new EvalTargetEditViewModels { Tables = tables, AccessorsAndTargets = accessorTargets, TablePropertion = tablePropertion, CurrentTarget = currentTarget });
      }

      [HttpPost]
      public ActionResult TargetMemberEdit(EvalTargetEditViewModels model)
      {
         //当前周期
         var period = EvalPeriod.GetCurrentPeriod(db).FirstOrDefault();

         // 考核人-考核对象列表
         var accessorTargetList = model.AccessorsAndTargets;
         // 被考核人-考核表之间的权重
         var targetTablePropertions = model.TablePropertion;

         if (accessorTargetList.Count > 0)
         {
            var ids = accessorTargetList.Select(x => x.AccessorTargetId).ToArray();
            // tableid 为空表示 被考核人-考核人之间关系的数据，所以排除在外，不能删除
            db.EvalAccessorTargetDal.ConditionDelete(eat.AccessorTargetId.In(ids) & eat.TableId != Guid.Empty);

         }
         if (targetTablePropertions.Count > 0)
         {
            var ids = targetTablePropertions.Select(x => x.PropertionID).ToArray();
            db.EvalTargetTablePropertionDal.ConditionDelete(ettp.PropertionID.In(ids));
         }

         foreach (var item in accessorTargetList)
         {
            //item.GroupId = EvalGroupConfig.DefaultGroupId.ToGuid(Guid.Empty);
            item.AccessorTargetId = Guid.NewGuid();
            item.PeriodId = period.PeriodId;
            // tableid 为空表示 被考核人-考核人之间关系的数据，所以排除在外，不用添加
            if (!item.TableId.IsEmpty())
               db.EvalAccessorTargetDal.Insert(item);
         }
         foreach (var item in targetTablePropertions)
         {
            item.PropertionID = Guid.NewGuid();
            item.PeriodId = period.PeriodId;
            db.EvalTargetTablePropertionDal.Insert(item);
         }

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.EvalGroup.TARGET_EDIT_SUCCESS
         });
      }


      //	POST-Ajax: EvalGroup/RemoveTargetTable

      public ActionResult RemoveTargetTable(Guid targetId, Guid tableId)
      {
         var period = EvalPeriod.GetCurrentPeriod(db).FirstOrDefault();

         db.EvalAccessorTargetDal.ConditionDelete(eat.TableId == tableId & eat.TargetId == targetId & eat.PeriodId == period.PeriodId);

         db.EvalTargetTablePropertionDal.ConditionDelete(ettp.PeriodId == period.PeriodId & ettp.TargetId == targetId & ettp.TableId == tableId);

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.EvalGroup.REMOVE_TARGET_TABLE_SUCCESS
         });
      }

      //	POST-Ajax: EvalGroup/RemoveTargetMember

      [HttpPost]
      public ActionResult RemoveTargetMember(Guid evalAccessorTargetId)
      {
         db.EvalAccessorTargetDal.PrimaryDelete(evalAccessorTargetId);

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.EvalGroup.ACCESSOR_EDIT_SUCCESS
         });
      }

   }

}