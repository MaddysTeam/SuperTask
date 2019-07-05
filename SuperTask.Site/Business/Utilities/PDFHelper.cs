//using Microsoft.Office.Interop.Excel;
//using Microsoft.Office.Interop.Word;
//using ppt = Microsoft.Office.Interop.PowerPoint;
//using System;
//using System.Reflection;
//using Microsoft.Office.Core;
//using System.Diagnostics;

//namespace Business.Utilities
//{

//   public class PDFHelper
//   {

//      public static bool WordToPDF(string sourcePath, string targetPath)
//      {
//         bool result = false;
//         Microsoft.Office.Interop.Word.Application application;
//         try
//         {
//            application = (Microsoft.Office.Interop.Word.Application)
//                Microsoft.VisualBasic.Interaction.GetObject(null, "Word.Application");
//         }
//         catch
//         {
//            application = new Microsoft.Office.Interop.Word.Application();
//         }

//         Document document = null;
//         try
//         {

//            application.Visible = false;
//            document = application.Documents.Open(sourcePath);
//            document.ExportAsFixedFormat(targetPath, WdExportFormat.wdExportFormatPDF);
//            result = true;
//         }
//         catch (Exception e)
//         {
//            Console.WriteLine(e.Message);
//            result = false;
//         }
//         finally
//         {
//            if (document != null)
//               document.Close();
//            if (application != null)
//            {
//               application.Quit();
//               application = null;
//            }
//         }

//         return result;
//      }


//      public static bool ExcelToPdf(string sourcePath, string targetPath)
//      {
//         bool result = false;
//         object missing = Type.Missing;
//         Microsoft.Office.Interop.Excel.Application applicationClass = null;
//         Workbook workbook = null;
//         //try
//         //{
//         applicationClass = new Microsoft.Office.Interop.Excel.Application();
//         string inputfileName = sourcePath;//需要转格式的文件路径
//         string outputFileName = targetPath;//转换完成后PDF文件的路径和文件名名称
//         XlFixedFormatType xlFixedFormatType = XlFixedFormatType.xlTypePDF;//导出文件所使用的格式
//         XlFixedFormatQuality xlFixedFormatQuality = XlFixedFormatQuality.xlQualityStandard;//1.xlQualityStandard:质量标准，2.xlQualityMinimum;最低质量
//         bool includeDocProperties = false;//如果设置为True，则忽略在发布时设置的任何打印区域。
//         bool openAfterPublish = false;//发布后不打开
//         workbook = applicationClass.Workbooks.Open(inputfileName, missing, missing, missing, missing, missing, missing, missing, missing, missing, missing, missing, missing, missing, missing);
//         if (workbook != null)
//         {
//            workbook.ExportAsFixedFormat(xlFixedFormatType, outputFileName, xlFixedFormatQuality, includeDocProperties, openAfterPublish, missing, missing, missing, missing);
//         }
//         result = true;
//         //}
//         //catch(Exception e)
//         //{
//         //   result = false;
//         //}
//         //finally
//         //{
//         //   if (workbook != null)
//         //   {
//         //      workbook.Close(true, missing, missing);
//         //      workbook = null;
//         //   }
//         //   if (applicationClass != null)
//         //   {
//         //      applicationClass.Quit();
//         //      applicationClass = null;
//         //   }
//         //}
//         if (workbook != null)
//         {
//            workbook.Close(true, missing, missing);
//            workbook = null;
//         }
//         if (applicationClass != null)
//         {
//            applicationClass.Quit();
//            applicationClass = null;
//         }

//         return result;
//      }


//      public static bool PPTToPDF(string sourcePath, string targetPath)
//      {
//         bool result;
//         ppt.PpSaveAsFileType targetFileType = ppt.PpSaveAsFileType.ppSaveAsPDF;
//         object missing = Type.Missing;
//         ppt.Application application = null;
//         ppt.Presentation persentation = null;
//         //try
//         //{
//         application = new ppt.Application();
//         persentation = application.Presentations.Open(sourcePath, MsoTriState.msoTrue, MsoTriState.msoFalse, MsoTriState.msoFalse);
//         persentation.SaveAs(targetPath, targetFileType, MsoTriState.msoTrue);

//         result = true;
//         //}

//         if (persentation != null)
//         {
//            persentation.Close();
//            persentation = null;
//         }
//         if (application != null)
//         {
//            application.Quit();
//            application = null;
//         }
//         GC.Collect();
//         GC.WaitForPendingFinalizers();
//         GC.Collect();
//         GC.WaitForPendingFinalizers();
//         //catch
//         //{
//         //   result = false;
//         //}
//         //finally
//         //{
//         //   if (persentation != null)
//         //   {
//         //      persentation.Close();
//         //      persentation = null;
//         //   }
//         //   if (application != null)
//         //   {
//         //      application.Quit();
//         //      application = null;
//         //   }
//         //   GC.Collect();
//         //   GC.WaitForPendingFinalizers();
//         //   GC.Collect();
//         //   GC.WaitForPendingFinalizers();
//         //}
//         return result;
//      }

//   }

//}