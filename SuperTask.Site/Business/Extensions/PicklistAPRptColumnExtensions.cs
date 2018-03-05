using Symber.Web.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Business
{

	public static class PicklistAPRptColumnExtensions
	{

		//public static IEnumerable<SelectListItem> GetSelectList(
		//	this PicklistAPRptColumn column,
		//	Func<PicklistItem, bool> filter = null,
		//	Func<PicklistItem, string> grouper = null,
		//	string noneLabel = null)
		//{
		//	var cacheUnit = PicklistCache.Cached(column.InnerKey);
		//	var defaultItem = cacheUnit.DefaultItem;
		//	List<SelectListGroup> groups = grouper != null ? new List<SelectListGroup>() : null;

		//	if (noneLabel != null)
		//		yield return new SelectListItem() { Value = "0", Text = noneLabel };

		//	foreach (var item in cacheUnit.Items)
		//	{
		//		if (filter == null || filter.Invoke(item))
		//		{
		//			SelectListGroup group = null;
		//			if (grouper != null)
		//			{
		//				string groupName = grouper(item);
		//				group = groups.FirstOrDefault(g => String.Equals(g.Name, groupName, StringComparison.InvariantCulture));
		//				if (group == null)
		//				{
		//					group = new SelectListGroup() { Name = groupName };
		//					groups.Add(group);
		//				}
		//			}

		//			yield return new SelectListItem()
		//			{
		//				Value = item.PicklistItemId.ToString(),
		//				Text = item.Name,
		//				Selected = item == defaultItem,
		//				Group = group
		//			};
		//		}
		//	}
		//}

	}

}