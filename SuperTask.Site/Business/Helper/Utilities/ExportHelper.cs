
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;

namespace Business.Helper
{

   public class ExportHelper
   {

      public static void ExportToExcel<T>(List<T> data,string sheetName="Sheet1") where T : class
      {
         HSSFWorkbook book = new HSSFWorkbook();
         ISheet sheet1 = book.CreateSheet(sheetName);

         #region [头部]

         var headerRow = sheet1.CreateRow(0);
         var properties = typeof(T).GetProperties();
         for (int i = 0; i <= properties.Length; i++)
            headerRow.CreateCell(i).SetCellValue(properties[i].Name);

         #endregion

         #region [内容]

         for (int i = 0; i < data.Count; i++)
         {
            var dataRow = sheet1.CreateRow(i + 1);
            var item = data[0];
            if (item != null)
            {
               for (int j = 0; j <= properties.Length; j++)
               {
                  dataRow.CreateCell(j).SetCellValue(properties[j].GetValue(item).ToString());
               }
            }
         }

         #endregion
      }

   }

}