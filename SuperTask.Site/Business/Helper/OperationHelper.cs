﻿using RoadFlow.Data.MSSQL;
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


		/// <summary>
		/// get operation history view models data
		/// </summary>
		/// <param name="id">item id like task, require , bug and publish </param>
		/// <param name="user"></param>
		/// <param name="result"></param>
		/// <param name="db"></param>
		/// <returns></returns>
		public static List<OperationHistoryViewModel> GetOperationHistoryViewModels(Guid id, UserInfo user, Func<Operation, string> result, APDBDef db)
		{
			var o = APDBDef.Operation;
			var operations = db.OperationDal.ConditionQuery(o.ItemId == id, o.OperationDate.Desc, null, null);
			var operationHistory = new List<OperationHistoryViewModel>();
			foreach (var item in operations)
			{
				operationHistory.Add(new OperationHistoryViewModel
				{
					Date = item.OperationDate.ToyyMMddHHmmss(),
					Operator = user.RealName,
					ResultId = item.OperationResult,
					Result = result(item)
				}
			  );
			}
			return operationHistory;
		}

	}

}