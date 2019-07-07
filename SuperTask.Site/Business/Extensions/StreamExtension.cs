using System.IO;

namespace Business
{

   public static class StreamExtension
   {

      public static Stream ConvertToPDF(this Stream stream, string ext)
      {
         Stream pdfStream = null;
         if (ext.ToLowerInvariant() == ".pptx" || ext.ToLowerInvariant() == ".ppt")
         {
            pdfStream = Util.ThirdParty.Aspose.PPTConverter.ConvertoPdf(stream);
         }
         else if (ext.ToLowerInvariant() == ".xlsx" || ext.ToLowerInvariant() == ".xls" || ext.ToLowerInvariant() == ".csv")
         {
            pdfStream = Util.ThirdParty.Aspose.ExcelConverter.ConvertoPdf(stream);
         }
         else if (ext.ToLowerInvariant() == ".doc" || ext.ToLowerInvariant() == ".docx")
         {
            pdfStream = Util.ThirdParty.Aspose.WordConverter.ConvertoPdf(stream);
         }

         return pdfStream;
      }

   }

}
