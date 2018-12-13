using Symber.Web.Data;
using System.Linq;
using System.Web.Mvc;

namespace TheSite.ModelBinder
{

	public class AjaxOrderModelBinder : DefaultModelBinder
	{

		public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			var col = controllerContext.HttpContext.Request.Form;
			var name = bindingContext.ModelName + "[";

			var p = (from key in col.Keys.Cast<string>()
						from value in col.GetValues(key)
						where key.StartsWith(name)
						select new AjaxOrder
						{
							ID = key.Substring(name.Length, key.Length - name.Length - 1),
							According = value == "asc" ? APSqlOrderAccording.Asc : APSqlOrderAccording.Desc
						}).FirstOrDefault();

			return p;
		}

	}

}