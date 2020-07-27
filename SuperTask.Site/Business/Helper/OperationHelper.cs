using RoadFlow.Data.MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheSite.Models;

namespace Business.Helper
{

	public class OperationHelper
	{
		static APDBDef.OperationTableDef o = APDBDef.Operation;

		public static Operation GetOperation(Guid itemId, Guid opertaionType, APDBDef db = null)
		{
			db = db ?? new APDBDef();
			return db.OperationDal.ConditionQuery(o.ItemId == itemId & o.OperationType == opertaionType, o.OperationDate.Desc, null, null).FirstOrDefault();
		}

      public static List<OperationHistoryViewModel> GetOperationHistoryViewModels(Guid id, UserInfo user, Func<Guid,string> result, APDBDef db)
      {
         var o = APDBDef.Operation;
         var operations = db.OperationDal.ConditionQuery(o.ItemId == id, o.OperationDate.Desc, null, null);
         var operationHistory = new List<OperationHistoryViewModel>();
         foreach (var item in operations)
         {
            operationHistory.Add(new OperationHistoryViewModel
            {
               Date = item.OperationDate.ToyyMMdd(),
               Operator = user.RealName,
               ResultId = item.OperationResult,
               Result = result(item.OperationType) // 这里比较特殊，任务的操作结果就是操作类型
            }
          );
         }
         return operationHistory;
      }

   }

}