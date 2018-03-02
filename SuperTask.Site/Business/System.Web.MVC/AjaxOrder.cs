using Symber.Web.Data;

namespace System.Web.Mvc
{

	public class AjaxOrder
	{

		public string ID { get; set; }


		public APSqlOrderAccording According { get; set; }


		public APSqlOrderPhrase OrderBy(APSqlOperateExpr column)
			=> new APSqlOrderPhrase(column, According);

	}

}