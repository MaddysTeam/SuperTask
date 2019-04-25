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


      //	GET: EvalGroup/BindGroupMember
      //	POST-Ajax: EvalGroup/BindGroupMember

      public ActionResult BindGroupMembers(Guid groupId)
      {
         //TODO: 过滤
         var period = EvalPeriod.GetCurrentPeriod(db).FirstOrDefault();

         var results = APQuery.select(egm.GroupMemberId, u.UserId.As("userId"), u.UserName)
                              .from(u, egm.JoinLeft(u.UserId == egm.MemberId & egm.GroupId == groupId))
                              .where(u.IsDelete==false)
            .query(db, r => new EvalGroupMember
            {
               GroupMemberId = egm.GroupMemberId.GetValue(r),
               MemberId = u.UserId.GetValue(r, "userId"),
               MemberName = u.UserName.GetValue(r),
               GroupId = groupId
            }).ToList();

         return PartialView(results);
      }

      [HttpPost]
      public ActionResult BindGroupMembers(Guid groupId, string memberIds)
      {
         if (string.IsNullOrEmpty(memberIds))
         {
            return Json(new
            {
               result = AjaxResults.Success,
               msg = Errors.EvalGroup.BIND_MEMBER_FAIL
            });
         }

         //TODO: 如果accessorTarget  表中的targetId 已经绑定了memberId 则不能加入任何组

         var ids = new List<Guid>();
         foreach (var id in memberIds.Split(','))
         {
            ids.Add(id.ToGuid(Guid.Empty));
         }

         if (!string.IsNullOrEmpty(memberIds))
         {
            db.EvalGroupMemberDal.ConditionDelete(egm.GroupId == groupId);
         }

         foreach (var memberId in ids)
         {
            db.EvalGroupMemberDal.Insert(new EvalGroupMember { GroupMemberId = Guid.NewGuid(), MemberId = memberId, GroupId = groupId });
         }

         //TODO 如果是新组员需要复制考核表比重信息（evalAccessorTarget 表）

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

         var results = APQuery.select(ega.GroupAccessorId, u.UserId.As("userId"), u.UserName)
                              .from(u, ega.JoinLeft(u.UserId == ega.AccessorId & ega.GroupId == groupId))
                              .where(u.IsDelete == false)
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
         var groupAccessorTargets = db.EvalAccessorTargetDal.ConditionQuery(eat.TargetId == groupId & eat.PeriodId == period.PeriodId, null, null, null);
         var members = db.EvalGroupMemberDal.ConditionQuery(egm.GroupId == groupId, null, null, null);
         var ids = new List<Guid>();
         foreach (var id in accessorIds.Split(','))
         {
            ids.Add(id.ToGuid(Guid.Empty));
         }

         db.EvalGroupAccessorDal.ConditionDelete(ega.GroupId == groupId);
         var subquery = APQuery.select(eat.AccessorTargetId)
                               .from(eat)
                               .where(eat.TargetId == groupId & eat.PeriodId == period.PeriodId & eat.AccessorId.NotIn(ids.ToArray()));

         if (members.Count > 0)
         {
            var tagetIds = members.Select(x => x.MemberId).ToArray();
            db.EvalTargetTablePropertionDal.ConditionDelete(ettp.TargetId.In(tagetIds) & ettp.PeriodId == period.PeriodId);

            subquery.where_or(eat.TargetId.In(tagetIds) & eat.PeriodId == period.PeriodId);
         }

         db.EvalAccessorTargetDal.ConditionDelete(eat.AccessorTargetId.In(subquery));

         foreach (var accessorId in ids)
         {
            //放入大组
            if (!allAccessors.Exists(a => a.AccessorId == accessorId && a.GroupId == Guid.Empty))
               db.EvalGroupAccessorDal.Insert(new EvalGroupAccessor { GroupAccessorId = Guid.NewGuid(), AccessorId = accessorId });

            //添加小组考核人
            db.EvalGroupAccessorDal.Insert(new EvalGroupAccessor { GroupAccessorId = Guid.NewGuid(), AccessorId = accessorId, GroupId = groupId });

            // 新增考核人和组之间的绑定
            if (!groupAccessorTargets.Exists(e => e.AccessorId == accessorId && e.TargetId == groupId && e.PeriodId == period.PeriodId))
               db.EvalAccessorTargetDal.Insert(new EvalAccessorTarget { AccessorTargetId = Guid.NewGuid(), TargetId = groupId, AccessorId = accessorId, PeriodId = period.PeriodId });

            foreach (var member in members)
            {
               db.EvalAccessorTargetDal.Insert(new EvalAccessorTarget { AccessorTargetId = Guid.NewGuid(), TargetId = member.MemberId, AccessorId = accessorId, PeriodId = period.PeriodId });
            }
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
                               .where(eat.TargetId == groupId & eat.PeriodId == period.PeriodId & u.IsDelete==false)
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

         //query.primary(eat.AccessorTargetId)
         //   .skip((current - 1) * rowCount)
         //    .take(rowCount);


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
         var u = APDBDef.UserInfo;
         var tables = db.EvalTableDal.ConditionQuery(et.TableStatus != EvalTableKeys.DisableStatus, null, null, null);
         var users = db.UserInfoDal.ConditionQuery(u.IsDelete==false, null, null, null);
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
            .where(u.IsDelete==false)
            .query(db, r => new EvalAccessorTarget { AccessorTargetId = at.AccessorTargetId.GetValue(r), TargetId = u.UserId.GetValue(r, "userId"), TargetName = u.UserName.GetValue(r) })
            .ToList();

         return PartialView(reuslts);
      }

      // 注意！这里设置的比考核小组优先级高，所以一旦绑定该被考核人必须离开小组
      [HttpPost]
      public ActionResult BindTargets(Guid accessorId, string targetIds)
      {
         if (string.IsNullOrEmpty(targetIds))
            return Json(new
            {
               result = AjaxResults.Success,
               msg = Errors.EvalGroup.BIND_TARGET_FAIL
            });

         var at = APDBDef.EvalAccessorTarget;
         var period = EvalPeriod.GetCurrentPeriod(db).FirstOrDefault();
         var accessorTargets = EvalAccessorTarget.ConditionQuery(at.AccessorId == accessorId & at.PeriodId == period.PeriodId, null);
         var ids = targetIds.Split(',').ToList();
         var delIds = new List<Guid>();
         var newIds = new List<Guid>();
         foreach (var item in accessorTargets)
         {
            if (!ids.Exists(x => x.ToGuid(Guid.Empty) == item.TargetId))
               delIds.Add(item.AccessorTargetId);
         }
         foreach (var item in ids)
         {
            var newId = item.ToGuid(Guid.Empty);
            if (!accessorTargets.Exists(x => x.TargetId == newId))
               newIds.Add(newId);
         }

        
         db.BeginTrans();

         try
         {
            if (delIds.Count > 0)
            {
               EvalAccessorTarget.ConditionDelete(at.AccessorTargetId.In(delIds.ToArray()));
               var subquery = APQuery.select(ettp.PropertionID).where(ettp.PeriodId == period.PeriodId & ettp.TargetId.In(delIds.ToArray()), null);
               EvalTargetTablePropertion.ConditionDelete(ettp.PropertionID.In(subquery));
            }

            //TODO:如果当前被考核人在小组里则从小组里移除
            EvalGroupMember.ConditionDelete(egm.MemberId.In(newIds.ToArray()));

            foreach (var targetId in newIds)
            {
               db.EvalAccessorTargetDal.Insert(
                 new EvalAccessorTarget
                 {
                    AccessorId = accessorId,
                    AccessorTargetId = Guid.NewGuid(),

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

         var subquery = APQuery.select(eat.TableId).from(eat).where(eat.TargetId == targetId & eat.PeriodId==period.PeriodId );
         var tables = db.EvalTableDal.ConditionQuery(et.TableStatus != EvalTableKeys.DisableStatus & et.TableId.NotIn(subquery), null, null, null); //左侧可用考核列表
         var tablePropertion = db.EvalTargetTablePropertionDal.ConditionQuery(ettp.TargetId == targetId & ettp.PeriodId == period.PeriodId, null, null, null);
         var accessorTargets = APQuery.select(eat.Asterisk, u.UserName.As("accessor"), tu.UserName.As("target"), et.TableId.As("tableId"), et.TableName.As("tableName")) // 评审人
                               .from(eat,
                                       et.JoinLeft(et.TableId == eat.TableId),
                                       u.JoinInner(u.UserId == eat.AccessorId),
                                       tu.JoinInner(tu.UserId == eat.TargetId)
                                       )
                               .where(eat.TargetId == targetId & eat.PeriodId == period.PeriodId & u.IsDelete==false)
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


      // 注意：考核对象分为考核人和考核小组两种类型，如果是考核小组的话，则必须要将每个组员作为考核对象

      [HttpPost]
      public ActionResult TargetMemberEdit(EvalTargetEditViewModels model)
      {
         //当前周期
         var period = EvalPeriod.GetCurrentPeriod(db).FirstOrDefault();
         var targetId = model.TablePropertion[0].TargetId;
         var tableId = model.TablePropertion[0].TableId;
         var originalTargetList = db.EvalAccessorTargetDal.ConditionQuery(eat.PeriodId == period.PeriodId & eat.TargetId == targetId & eat.TableId == tableId, null, null, null);
         var originalTargetTablePropertionList = db.EvalTargetTablePropertionDal.ConditionQuery(ettp.PeriodId == period.PeriodId & ettp.TargetId == targetId & ettp.TableId == tableId, null, null, null);

         if (originalTargetList.Count > 0)
         {
            var ids = originalTargetList.Select(x => x.AccessorTargetId).ToArray();
            db.EvalAccessorTargetDal.ConditionDelete(eat.AccessorTargetId.In(ids) & eat.TableId != Guid.Empty);
         }

         if (originalTargetTablePropertionList.Count > 0)
         {
            var ids = originalTargetTablePropertionList.Select(x => x.PropertionID).ToArray();
            db.EvalTargetTablePropertionDal.ConditionDelete(ettp.PropertionID.In(ids));
         }

         foreach (var item in model.AccessorsAndTargets)
         {
            item.AccessorTargetId = Guid.NewGuid();
            item.PeriodId = period.PeriodId;
            // tableid 为空表示 考核人-被考核对象之间关系的数据，所以排除在外，不用添加
            if (!item.TableId.IsEmpty())
               db.EvalAccessorTargetDal.Insert(item);
         }
         foreach (var item in model.TablePropertion)
         {
            item.PropertionID = Guid.NewGuid();
            item.PeriodId = period.PeriodId;
            db.EvalTargetTablePropertionDal.Insert(item);
         }

         // TODO: 如果targetId 是小组id则需要递归添加组内成员的考核表权重绑定信息
         if (model.TargetIsGroup)
         {
            var members =
            TargetMemberEdit(new EvalTargetEditViewModels { Tables = model.Tables, TargetIsGroup = false, });
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