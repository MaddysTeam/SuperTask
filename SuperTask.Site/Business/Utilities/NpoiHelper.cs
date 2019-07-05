using NPOI.HSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Business.Utilities
{

	public class NPOIHelper
	{

		public static HSSFWorkbook CreateBook<T>(Dictionary<long, T> dic) where T : class
		{
			//创建Excel文件的对象
			NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
			//添加一个sheet
			NPOI.SS.UserModel.ISheet sheet1 = book.CreateSheet("Sheet1");

			#region [头部设计]

			var i = 0;
			//给sheet1添加第一行的头部标题
			NPOI.SS.UserModel.IRow row1 = sheet1.CreateRow(0);
			foreach (var item in typeof(T).GetProperties())
			{
				if (item.PropertyType == typeof(string) || item.PropertyType == typeof(double))
				{
					var display = item.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();
					var header = display==null? string.Empty:display.Name;
					row1.CreateCell(i).SetCellValue(header);
					i++;
				}
			}

			#endregion

			i = 0;
			foreach (var item in dic.Values)
			{
				i++;
				NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i);
				var properties = item.GetType().GetProperties();
				var j = 0;
				foreach (var subItem in properties)
				{
					if (subItem.PropertyType == typeof(string) || subItem.PropertyType == typeof(double))
					{
						var sub = subItem.GetValue(item, null);
						if(sub is double)
							sub = Math.Round((double)sub, 2);

						sub = sub ?? string.Empty;
						rowtemp.CreateCell(j).SetCellValue(sub.ToString());
						j++;
					}
				}
			}

			return book;
		}

	}

}