using Aspose.Cells;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Util.ThirdParty.Aspose
{

   /// <summary>
   /// word converter 
   /// </summary>
   public static class ExcelConverter
   {

    


      public static Stream ConvertoHtml(Stream source)
      {

         if (source == null) throw new ArgumentNullException("source stream can not be null");
         //byte[] contentBytes = new byte[source.Length];
         //source.Read(contentBytes, 0, contentBytes.Length);
         var stream = ConvertRightNowAspose(source, SaveFormat.Html);

         return stream;
      }

      public static Stream ConvertoPdf(Stream source)
      {

         if (source == null) throw new ArgumentNullException("source stream can not be null");
         //byte[] contentBytes = new byte[source.Length];
         //source.Read(contentBytes, 0, contentBytes.Length);
         var stream = ConvertRightNowAspose(source, SaveFormat.Pdf);

         return stream;
      }


      internal static Stream ConvertRightNowAspose(Stream stream, SaveFormat saveFormat)
      {
         Workbook book = new Workbook(stream);
         var firstStream = new MemoryStream();
         if (saveFormat == SaveFormat.Html)
         {
            //HtmlSaveOptions options = new HtmlSaveOptions(saveFormat);
            //options.ImageSavingCallback = new HandleImageSaving();
            //doc.Save(firstStream, options);
         }
         else
         {
            book.Save(firstStream, saveFormat);
         }

         return firstStream;
      }

      //class HandleImageSaving : IImageSavingCallback
      //{
      //   void IImageSavingCallback.ImageSaving(ImageSavingArgs e)
      //   {
      //      e.ImageStream = new MemoryStream();
      //      e.KeepImageStreamOpen = false;
      //   }
      //}


   }

}


