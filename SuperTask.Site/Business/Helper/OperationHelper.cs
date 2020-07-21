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
			
	}

}