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
      static APDBDef.EvalGroupTargetTableDef egt = APDBDef.EvalGroupTarget;
      static APDBDef.EvalAccessorTargetTableDef eat = APDBDef.EvalAccessorTarget;


      //	GET: EvalGroup/AccessorList
      //	POST-Ajax: EvalGroup/AccessorList

      public ActionResult AccessorList(Guid groupId)
      {
         return View();
      }

      [HttpPost]
      public ActionResult AccessorList(Guid groupId, int current, int rowCount, AjaxOrder sort, string searchPhrase)
      {
         var query = APQuery.select(ega.GroupAccessorId, ega.IsLeader, ega.GroupId, ega.ModifyDate, ega.AccessorId, u.UserName.As("UserName"))
                            .from(ega
                                  , u.JoinInner(u.UserId == ega.AccessorId))
                            .where(ega.GroupId == groupId);

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
               //role=r.RoleName.GetValue(rd, "RoleName"),
               modifyDate = ega.ModifyDate.GetValue(rd),
               isLeader = ega.IsLeader.GetValue(rd) ? "是" : "否",
               accessorId = ega.AccessorId.GetValue(rd),

            });


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
         var query = APQuery.select(egt.GroupTargetId, egt.GroupId, egt.ModifyDate, u.UserName.As("UserName"))
                   .from(egt
                         , u.JoinInner(u.UserId == egt.MemberId))
                   .where(egt.GroupId == groupId);

         query.primary(egt.MemberId)
            .skip((current - 1) * rowCount)
             .take(rowCount);


         //获得查询的总数量

         var total = db.ExecuteSizeOfSelect(query);


         var result = query.query(db, rd => new
         {

            id = egt.GroupTargetId.GetValue(rd),
            name = u.UserName.GetValue(rd, "UserName"),
            modifyDate = egt.ModifyDate.GetValue(rd),

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
         var accessor = new EvalGroupAccessor() { GroupId = EvalGroupConfig.DefaultGroupId.ToGuid(Guid.Empty) };

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

            ViewBag.EvalTables = GetSelectTableItem(tables, accessor.TableIds);
         }
         else
         {
            ViewBag.EvalTables = GetSelectTableItem(tables, string.Empty).ToList();
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
         if(isExists)
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
            msg = Success.EvalGroup.Accessor_EDIT_SUCCESS
         });
      }


      //	GET: EvalGroup/BindTargets
      //	POST-Ajax: EvalGroup/BindTargets

      public ActionResult BindTargets(Guid accessorId)
      {
         if (accessorId.IsEmpty())
            throw new ApplicationException();

         var at = APDBDef.EvalAccessorTarget;

         var reuslts = APQuery.select(at.TargetId, u.UserName)
            .from(at, u.JoinInner(at.TargetId == u.UserId))
            .where(at.AccessorId == accessorId & at.GroupId == EvalGroupConfig.DefaultGroupId.ToGuid(Guid.Empty))
            .query(db, r => new EvalGroupTarget { MemberId = at.TargetId.GetValue(r), TargetName = u.UserName.GetValue(r) })
            .ToList();

         ViewBag.AllUsers = UserInfo.GetAll().Select(x => new EvalGroupTarget { MemberId = x.UserId, TargetName = x.UserName }).ToList();

         ViewBag.SelectUsers = reuslts;

         return PartialView();
      }

      [HttpPost]
      public ActionResult BindTargets(Guid accessorId, string targetIds)
      {
         var at = APDBDef.EvalAccessorTarget;

         if (string.IsNullOrEmpty(targetIds))
            return Json(new
            {
               result = AjaxResults.Success,
               msg = Success.EvalGroup.Bind_Target_SUCCESS
            });


         var targets = EvalGroupTarget.GetAll();
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
                    GroupId = EvalGroupConfig.DefaultGroupId.ToGuid(Guid.Empty),
                    EvalType = EvalKeys.SubjectType,
                    ModifyDate = DateTime.Now,
                    TargetId = targetId
                 });

               if (targets.Find(x => x.MemberId == targetId) == null)
                  db.EvalGroupTargetDal.Insert(new EvalGroupTarget
                  {
                     GroupId = EvalGroupConfig.DefaultGroupId.ToGuid(Guid.Empty),
                     GroupTargetId = Guid.NewGuid(),
                     MemberId = targetId,
                     TableIds = string.Empty,
                     ModifyDate = DateTime.Now,
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
            msg = Success.EvalGroup.Bind_Target_SUCCESS
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
            msg = Success.EvalGroup.Accessor_EDIT_SUCCESS
         });
      }


      //	GET: EvalGroup/TargetMemberEdit
      //	POST-Ajax: EvalGroup/TargetMemberEdit

      public ActionResult TargetMemberEdit(Guid groupTargetId)
      {
         var tables = db.EvalTableDal.ConditionQuery(et.TableStatus != EvalTableKeys.DisableStatus, null, null, null);
         var users = db.UserInfoDal.ConditionQuery(null, null, null, null);
         var target = new EvalGroupTarget() { GroupId = EvalGroupConfig.DefaultGroupId.ToGuid(Guid.Empty) };

         if (!groupTargetId.IsEmpty())
         {
            target = APQuery.select(egt.Asterisk, u.UserName)
                                  .from(egt, u.JoinInner(u.UserId == egt.MemberId))
                                  .where(egt.GroupTargetId == groupTargetId)
                                  .query(db, rd =>
                                  {
                                     var tgt = new EvalGroupTarget();
                                     egt.Fullup(rd, tgt, false);
                                     return tgt;
                                  }
                                  ).FirstOrDefault();

            ViewBag.EvalTables = GetSelectTableItem(tables, target.TableIds);
         }
         else
         {
            ViewBag.EvalTables = GetSelectTableItem(tables, string.Empty).ToList();
         }

         ViewBag.Users = SelectListHelper.GetSelectItems(users, "UserName", "UserId");


         return PartialView(target);
      }

      [HttpPost]
      public ActionResult TargetMemberEdit(EvalGroupTarget target)
      {
         target.ModifyDate = DateTime.Now;

         var existTargets = EvalGroupTarget.ConditionQuery(egt.MemberId == target.MemberId, null);

         if (target.GroupTargetId.IsEmpty()
            && existTargets.Count <= 0)
         {
            target.GroupTargetId = Guid.NewGuid();

            //如果是新的考核对象，根据角色自动默认选择考核表
            if (string.IsNullOrEmpty(target.TableIds))
            {
               //根据角色自动默认选择考核表
               var tables = db.EvalTableDal.ConditionQuery(et.TableStatus != EvalTableKeys.DisableStatus, null, null, null);
               var roles = RoleHelper.GetUserRoles(target.MemberId, db);
               var targetTables = GetTablesByRoles(roles, tables);

               if (targetTables.Count > 0)
                  target.TableIds = string.Join(",", targetTables.Select(x => x.TableId).ToArray());
            }

            db.EvalGroupTargetDal.Insert(target);
         }
         else
         {
            db.EvalGroupTargetDal.Update(target);
         }


         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.EvalGroup.Target_EDIT_SUCCESS
         });
      }


      //	GET: EvalGroup/RemoveTargetMember
      //	POST-Ajax: EvalGroup/RemoveTargetMember

      [HttpPost]
      public ActionResult RemoveTargetMember(Guid groupTargetId)
      {
         var groupTarget = EvalGroupTarget.PrimaryGet(groupTargetId);
         if (groupTarget != null)
         {
            db.EvalAccessorTargetDal.ConditionDelete(eat.TargetId == groupTarget.MemberId);
            db.EvalGroupTargetDal.ConditionDelete(egt.GroupTargetId == groupTargetId);
         }
         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.EvalGroup.Accessor_EDIT_SUCCESS
         });
      }


      private IEnumerable<SelectListItem> GetSelectTableItem(List<EvalTable> tables, string tableIdStr)
      {
         if (tables == null && tables.Count <= 0)
         {
            return null;
         }

         var evalTableItems = tables.Select(item => new SelectListItem { Text = item.TableName, Value = item.TableId.ToString() });

         var evalTables = string.IsNullOrEmpty(tableIdStr) ? null : tableIdStr.Split(',');

         if (evalTables != null && evalTables.Count() > 0)
         {
            foreach (var item in evalTableItems)
               item.Selected = evalTables.Contains(item.Value, StringComparison.InvariantCultureIgnoreCase);
         }

         return evalTableItems;
      }


      private List<EvalTable> GetTablesByRoles(List<Role> roles, List<EvalTable> allTables)
      {
         var tempTables = new List<EvalTable>();
         foreach (var item in roles)
         {
            foreach (var subItem in allTables)
            {
               if (subItem.MemberRoleIds.Contains(item.RoleId.ToString()))
                  tempTables.Add(subItem);
            }
         }

         return tempTables;
      }

   }

}