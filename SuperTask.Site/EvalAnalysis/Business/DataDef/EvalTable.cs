using Business.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheSite.Models;

namespace Business
{

   public partial class EvalTable
   {

      public Dictionary<Indication, List<EvalIndication>> EvalIndications { get; set; }

      public List<Role> AccessorRoles { get; protected set; }

      public List<Role> MemberRoles { get; set; }

      public string AccessorRoleNames { get; set; }

      public bool IsSelected { get; set; }

      public Result Validate()
      {
         var result = new Result { IsSuccess = true, Msg = Success.Task.EDIT_SUCCESS };

         if (string.IsNullOrEmpty(TableName))
         {
            result.Msg = Errors.EvalTable.NOT_ALLOWED_NAME_NULL;
            result.IsSuccess = false;
         }

         if (FullScore <= 0)
         {
            result.Msg = Errors.EvalTable.NOT_ALLOWED_SCORE_LESS_THAN_ZERO;
            result.IsSuccess = false;
         }

         return result;
      }

      public bool IsBuildDone => TableStatus == EvalTableKeys.DoneStatus;

      public void BuildAccessorRoles(List<Role> roles)
      {
         AccessorRoles = new List<Role>();

         //if (!string.IsNullOrEmpty(AccessorRoleIds))
         //{
         //   AccessorRoleIds.Split(',').ToList().ForEach(ro =>
         //   {
         //      var role = roles.Find(x => x.RoleId == ro.ToGuid(Guid.Empty));
         //      if (role != null)
         //         AccessorRoles.Add(role);
         //   });
         //}
      }

      public bool IsInUse()
      {
         var er = APDBDef.EvalResult;

         return EvalResult.ConditionQueryCount(er.TableId==TableId)>0;
      }

      //public static List<EvalIndication> GetEvalIndications(Guid tableId,APDBDef db=null)
      //{
      //   var ei = APDBDef.EvalIndication;

      //   db = db ?? new APDBDef();

      //   return EvalIndication.ConditionQuery(ei.TableId == tableId, null);
      //}

   }

}