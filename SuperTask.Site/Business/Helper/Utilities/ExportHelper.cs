
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;

namespace Business.Helper
{

   public class ExportHelper
   {

      public static HSSFWorkbook ExportToExcel<T>(List<T> data, string[] columns=null,string sheetName="Sheet1") where T : class
      {
         HSSFWorkbook book = new HSSFWorkbook();
         ISheet sheet1 = book.CreateSheet(sheetName);

         #region [头部]

         var headerRow = sheet1.CreateRow(0);
         var properties = typeof(T).GetProperties();

         if (columns == null || columns.Length <= 0)
         {
            for (int i = 0; i < properties.Length; i++)
               headerRow.CreateCell(i).SetCellValue(properties[i].Name);
         }
         else
         {
            for (int i = 0; i < columns.Length; i++)
               headerRow.CreateCell(i).SetCellValue(columns[i]);
         }

         #endregion

         #region [内容]

         for (int i = 0; i < data.Count; i++)
         {
            var dataRow = sheet1.CreateRow(i + 1);
            var item = data[i];
            if (item != null)
            {
               object o = null;
               for (int j = 0; j < properties.Length; j++)
               {
                  o = properties[j].GetValue(item);
                  if (o == null) continue;

                  dataRow.CreateCell(j).SetCellValue(o.ToString());
               }
            }
         }

         return book;
         #endregion
      }

   }

}