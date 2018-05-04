using Business;
using Business.Helper;
using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheSite.Models;

namespace TheSite.EvalAnalysis
{
   public interface IEvalMemeberHandler
   {
      List<EvalMember> GetTargetMembers(Guid accessorId, Guid accessorRoleId,APDBDef db);

      List<EvalMember> GetTargetMembers(Guid accessorId, Guid accessorRoleId,Guid periodId, APDBDef db);

   }

   public class EvalMemberHandler: IEvalMemeberHandler
   {
      protected static APDBDef.ResourceTableDef r = APDBDef.Resource;
      protected static APDBDef.ProjectTableDef p = APDBDef.Project;
      protected static APDBDef.UserInfoTableDef u = APDBDef.UserInfo;
      protected static APDBDef.EvalResultTableDef er = APDBDef.EvalResult;
      protected static APDBDef.EvalGroupTargetTableDef egt = APDBDef.EvalGroupTarget;
      protected static APDBDef.UserRoleTableDef ur = APDBDef.UserRole;
      protected static APDBDef.RoleTableDef ro = APDBDef.Role;
      protected static APDBDef.EvalAccessorTargetTableDef ett = APDBDef.EvalAccessorTarget;

      protected EvalManager _evalManager;

      public virtual List<EvalMember> GetTargetMembers(Guid accessorId, Guid accessorRoleId,APDBDef db)
      {
         throw new NotImplementedException();
      }

      public virtual List<EvalMember> GetTargetMembers(Guid accessorId, Guid accessorRoleId, Guid periodId, APDBDef db)
      {
         throw new NotImplementedException();
      }

   }


   /// <summary>
   ///  这里是自动量考业务逻辑，自动量考已整个考核组为单元进行,量表从当前考核期内查找
   ///  由于一个用户对应多个角色，所以需要accessorRoleId 来确定使用哪一个角色
   /// </summary>
   public class GroupMemberHandler : EvalMemberHandler
   {

      static APDBDef.EvalTableTableDef et = APDBDef.EvalTable;

      public override List<EvalMember> GetTargetMembers(Guid accessorId, Guid accessorRoleId, APDBDef db)
      {
         var accessorName = UserInfo.PrimaryGet(accessorId).UserName;
         var currentPeriods = EvalPeriod.GetCurrentPeriod(db);

         accessorRoleId = accessorRoleId.IsEmpty() ? EvalConfig.AutoAccessorRoleId.ToGuid(Guid.Empty) : accessorRoleId;

         var subQuery = APQuery.select(ett.TargetId).from(ett).where(ett.AccessorId==accessorId); // 搜索成员小组id
         var results = APQuery.select(egt.MemberId, egt.TableIds, u.UserName, er.ResultId.Count().As("EvalCount"), ro.RoleName.As("RoleName"), ro.RoleId.As("TargetRoleId"))
                         .from(egt,
                         ur.JoinInner(egt.MemberId == ur.UserId),
                         ro.JoinInner(ur.RoleId == ro.RoleId),
                         u .JoinInner(u.UserId == egt.MemberId),
                         er.JoinLeft(egt.MemberId == er.TargetId
                                            & er.AccesserId == accessorId
                                            & er.AccesserRoleId == accessorRoleId
                                            & er.TargetRoleId== ro.RoleId
                                            & er.EvalType == EvalTableKeys.AutoType))
                          .where(egt.MemberId.In(subQuery))
                          .group_by(egt.MemberId, egt.TableIds, u.UserName, ur.RoleId, ro.RoleName, ro.RoleId)
                          .query(db, r =>
                          {
                             var tableIds = egt.TableIds.GetValue(r);
                             return new EvalMember
                             {
                                MemberId = egt.MemberId.GetValue(r),
                                MemberName = u.UserName.GetValue(r),
                                AccessorId = accessorId,
                                AccessorName = accessorName,
                                TargetRoleId = ro.RoleId.GetValue(r, "TargetRoleId"),
                                TargetRoleName = ro.RoleName.GetValue(r, "RoleName"),
                                TableIds = tableIds, //GetAutoTableIds(tableIds,tables.ToList()),
                                EvalCount = (int)(r["EvalCount"]),
                                PeriodNames = string.Join(",", currentPeriods.Select(p => p.Name).ToArray())
                             };
                          }).ToList();

         return results;
      }

      public override List<EvalMember> GetTargetMembers(Guid accessorId, Guid accessorRoleId, Guid periodId, APDBDef db)
      {
         var ep = APDBDef.EvalPeriod;
         var ac = APDBDef.UserInfo.As("Accessor");

         accessorRoleId = accessorRoleId.IsEmpty() ? EvalConfig.AutoAccessorRoleId.ToGuid(Guid.Empty) : accessorRoleId;

         var results = APQuery.select(egt.MemberId, egt.TableIds, u.UserName, er.ResultId.Count().As("EvalCount"), ro.RoleName.As("RoleName"), ro.RoleId.As("TargetRoleId")
            , ep.Name.As("PeriodName"), ac.UserName.As("AccessorName"))
                         .from(egt,
                                ur.JoinInner(egt.MemberId == ur.UserId),
                                ro.JoinInner(ur.RoleId == ro.RoleId),
                                u.JoinInner(u.UserId == egt.MemberId),
                                er.JoinLeft(egt.MemberId == er.TargetId
                                            & er.AccesserId == accessorId
                                            & er.AccesserRoleId == accessorRoleId
                                            & er.TargetRoleId == ro.RoleId
                                            & er.EvalType == EvalTableKeys.AutoType),
                                ep.JoinLeft(ep.PeriodId == periodId),
                                ac.JoinLeft(ac.UserId == er.AccesserId))
                          .where(er.PeriodId==periodId)
                          .group_by(egt.MemberId, egt.TableIds, u.UserName, ep.Name, ac.UserName, ur.RoleId, ro.RoleName, ro.RoleId)
                          .query(db, r =>
                          {
                             return new EvalMember
                             {
                                MemberId = egt.MemberId.GetValue(r),
                                MemberName = u.UserName.GetValue(r),
                                AccessorId = accessorId,
                                AccessorName = ac.UserName.GetValue(r, "AccessorName"),
                                TargetRoleId = ro.RoleId.GetValue(r, "TargetRoleId"),
                                TargetRoleName = ro.RoleName.GetValue(r, "RoleName"),
                                TableIds = egt.TableIds.GetValue(r),
                                EvalCount = (int)(r["EvalCount"]),
                                PeriodNames = ep.Name.GetValue(r, "PeriodName")
                             };
                          }).ToList();

         return results;
      }

   }


   /// <summary>
   /// 主观考最新需求，考核对象从AccessorTarget中获取
   /// </summary>
   public class AccessorTargetsHandler: EvalMemberHandler
   {

      static APDBDef.EvalAccessorTargetTableDef at = APDBDef.EvalAccessorTarget;

      public override List<EvalMember> GetTargetMembers(Guid accessorId, Guid accessorRoleId, APDBDef db)
      {
         var accessorName = UserInfo.PrimaryGet(accessorId).UserName;
         var currentPeriods = EvalPeriod.GetCurrentPeriod(db);
         if (currentPeriods == null || currentPeriods.Count <= 0) throw new ApplicationException(Errors.Eval.NOT_IN_PERIOD);

         var subquery = APQuery.select(at.TargetId.As("EvalTargetId")).where(at.AccessorId== accessorId 
            & at.GroupId==EvalGroupConfig.DefaultGroupId.ToGuid(Guid.Empty));

         var results = APQuery.select(egt.MemberId.As("TargetId"), egt.TableIds, u.UserName, er.ResultId.Count().As("EvalCount"),ro.RoleName.As("RoleName"),ro.RoleId.As("TargetRoleId"))
                       .from(egt,
                             ur.JoinInner(egt.MemberId == ur.UserId),
                             ro.JoinInner(ur.RoleId == ro.RoleId),
                             at.JoinLeft(at.TargetId==egt.MemberId),
                             er.JoinLeft(egt.MemberId == er.TargetId
                                         & er.AccesserId == accessorId
                                         & er.AccesserRoleId == accessorRoleId
                                         & er.TargetRoleId==ro.RoleId
                                         & er.EvalType == EvalTableKeys.SubjectType),
                             u.JoinLeft(egt.MemberId == u.UserId)
                             
                             )
                        .where(at.AccessorId== accessorId & at.AccessorId==accessorId)
                        .group_by(egt.MemberId, egt.TableIds, u.UserName,ur.RoleId,ro.RoleName,ro.RoleId)
                        .query(db, r =>
                        {
                           return new EvalMember
                           {
                              MemberId = egt.MemberId.GetValue(r, "TargetId"),
                              MemberName = u.UserName.GetValue(r),
                              AccessorId = accessorId,
                              AccessorName = accessorName,
                              TargetRoleId = ro.RoleId.GetValue(r, "TargetRoleId"),
                              TargetRoleName = ro.RoleName.GetValue(r, "RoleName"),
                              TableIds = egt.TableIds.GetValue(r),
                              EvalCount = (int)(r["EvalCount"]),
                              PeriodNames = string.Join(",", currentPeriods.Select(p => p.Name).ToArray())
                           };
                        }).ToList();

         return results;
      }

      public override List<EvalMember> GetTargetMembers(Guid accessorId, Guid accessorRoleId, Guid periodId, APDBDef db)
      {
         var accessorName = UserInfo.PrimaryGet(accessorId).UserName;
         var currentPeriods = EvalPeriod.GetCurrentPeriod(db);
         if (currentPeriods == null || currentPeriods.Count <= 0) throw new ApplicationException(Errors.Eval.NOT_IN_PERIOD);

         var subquery = APQuery.select(at.TargetId.As("EvalTargetId")).where(at.AccessorId == accessorId
            & at.GroupId == EvalGroupConfig.DefaultGroupId.ToGuid(Guid.Empty));

         var results = APQuery.select(egt.MemberId.As("TargetId"), egt.TableIds, u.UserName, er.ResultId.Count().As("EvalCount"), ro.RoleName.As("RoleName"), ro.RoleId.As("TargetRoleId"))
                       .from(egt,
                             ur.JoinInner(egt.MemberId == ur.UserId),
                             ro.JoinInner(ur.RoleId == ro.RoleId),
                             at.JoinLeft(at.TargetId == egt.MemberId),
                             er.JoinLeft(egt.MemberId == er.TargetId
                                         & er.AccesserId == accessorId
                                         & er.AccesserRoleId == accessorRoleId
                                         & er.EvalType == EvalTableKeys.SubjectType
                                         & er.TargetRoleId == ro.RoleId),
                             u.JoinLeft(egt.MemberId == u.UserId))
                        .where(at.AccessorId == accessorId & at.AccessorId == accessorId)
                        .group_by(egt.MemberId, egt.TableIds, u.UserName,ro.RoleName, ro.RoleId)
                        .query(db, r =>
                        {
                           return new EvalMember
                           {
                              MemberId = egt.MemberId.GetValue(r, "TargetId"),
                              MemberName = u.UserName.GetValue(r),
                              AccessorId = accessorId,
                              AccessorName = accessorName,
                              TargetRoleId= ro.RoleId.GetValue(r, "TargetRoleId"),
                              TargetRoleName = ro.RoleName.GetValue(r, "RoleName"),
                              TableIds = egt.TableIds.GetValue(r),
                              EvalCount = (int)(r["EvalCount"]),
                              PeriodNames = string.Join(",", currentPeriods.Select(p => p.Name).ToArray())
                           };
                        }).ToList();

         return results;
      }
   }
}