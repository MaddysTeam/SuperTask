using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using O2S.Components.PDFRender4NET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Utilities
{

   public static class FormatConverter
   {

      /// <summary>
      /// html convert to pdf
      /// </summary>
      /// <param name="htmlText"></param>
      /// <returns></returns>
      public static byte[] ConvertHtmlTextToPDF(string htmlText)
      {
         if (string.IsNullOrEmpty(htmlText))
         {
            return null;
         }

         htmlText = "<p>" + htmlText + "</p>";

         MemoryStream outputStream = new MemoryStream();//要把PDF寫到哪個串流
         byte[] data = Encoding.UTF8.GetBytes(htmlText);//字串轉成byte[]
         MemoryStream msInput = new MemoryStream(data);

         Document doc = new Document(PageSize.A4);//要寫PDF的文件，建構子沒填的話預設直式A4
         PdfWriter writer = PdfWriter.GetInstance(doc, outputStream);
         writer.PageEvent = new PdfPageEvent();
         //指定文件預設開檔時的縮放為100%
         PdfDestination pdfDest = new PdfDestination(PdfDestination.XYZ, 0, doc.PageSize.Height, 1f);
         //開啟Document文件 
         doc.Open();
         //使用XMLWorkerHelper把Html parse到PDF檔裡
         XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, msInput, null, Encoding.UTF8);
         //將pdfDest設定的資料寫到PDF檔
         PdfAction action = PdfAction.GotoLocalPage(1, pdfDest, writer);
         writer.SetOpenAction(action);
         doc.Close();
         msInput.Close();
         outputStream.Close();
         //回傳PDF檔案 
         return outputStream.ToArray();
      }


      /// <summary>
      /// 页面完成时触发
      /// </summary>
      public class PdfPageEvent: PdfPageEventHelper, IPdfPageEvent
      {
         public override void OnEndPage(PdfWriter writer, Document document)
         {
            base.OnEndPage(writer, document);

            int pageNumber = writer.PageNumber;
            //if (pageNumber > 2)
            //{
               PdfContentByte cbs = writer.DirectContent;
               BaseFont bsFont = BaseFont.CreateFont(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "simsun.ttc") + ",0", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
               iTextSharp.text.Font fontfooter = new iTextSharp.text.Font(bsFont, 10, iTextSharp.text.Font.BOLD);
               Phrase footer = new Phrase((pageNumber).ToString(), fontfooter);
               ColumnText.ShowTextAligned(cbs, Element.ALIGN_CENTER, footer,
                         document.Right / 2, document.Bottom - 20, 0);
            //}
         }
      }


      /// <summary>
      /// 将PDF文档转换为图片的方法
      /// </summary>
      /// <param name="pdfInputPath">PDF文件路径</param>
      /// <param name="imageName">生成图片的名字</param>
      /// <param name="startPageNum">从PDF文档的第几页开始转换</param>
      /// <param name="endPageNum">从PDF文档的第几页开始停止转换</param>
      /// <param name="imageFormat">设置所需图片格式</param>
      /// <param name="definition">设置图片的清晰度，数字越大越清晰</param>
      public static Stream ConvertPDF2Image(Stream stream,
          string imageName, int startPageNum, int endPageNum, ImageFormat imageFormat, Definition definition=Definition.Ten)
      {
         PDFFile pdfFile = PDFFile.Open(stream);
      
         // validate pageNum
         if (startPageNum <= 0)
         {
            startPageNum = 1;
         }
         if (endPageNum > pdfFile.PageCount)
         {
            endPageNum = pdfFile.PageCount;
         }
         if (startPageNum > endPageNum)
         {
            int tempPageNum = startPageNum;
            startPageNum = endPageNum;
            endPageNum = startPageNum;
         }

         var ms = new MemoryStream();
         // start to convert each page
         for (int i = startPageNum; i <= endPageNum; i++)
         {
            Bitmap pageImage = pdfFile.GetPageImage(i - 1, 40 * (int)definition);
            pageImage.Save(ms, imageFormat);
            pageImage.Dispose();
         }

         pdfFile.Dispose();

         return ms;
      }

      public enum Definition
      {
         One = 1, Two = 2, Three = 3, Four = 4, Five = 5, Six = 6, Seven = 7, Eight = 8, Nine = 9, Ten = 10
      }

   }

}
