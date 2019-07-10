using System.IO;
using System.Web;

namespace Business
{

	public class PDFHandler : IHandler<Attachment, PDFContext>
   {

      public virtual void Handle(Attachment orginal, PDFContext context)
      {
         var attachment = orginal;
         var server = context.Server;
         var filePath = server.MapPath("~" + attachment.Url);
         var pdfVirtualPath = attachment.Url.Replace(attachment.FileExtName, ".pdf");
         var pdfPath = server.MapPath("~" + pdfVirtualPath);
         if (!System.IO.File.Exists(pdfPath))
         {
            ConverToPDF(filePath, pdfPath);
         }

         attachment.Url = pdfVirtualPath;
      }

      protected virtual void ConverToPDF(string filePath,string pdfPath)
      {
         using (FileStream fs = File.Open(filePath, FileMode.Open))
         {
            var ext = Path.GetExtension(filePath);
            var stream = fs.ConvertToPDF(ext);
            using (FileStream pdfStream = File.Create(pdfPath))
            {
               stream.Seek(0, SeekOrigin.Begin);
               stream.CopyTo(pdfStream);
            }
         }
      }

   }


   public class WordToPDFHandler : PDFHandler
   {
      
   }


   public class ExcelToPDFHandler : PDFHandler
   {
      
   }

   public class PPTToPDFHandler : PDFHandler
   {
     
   }


   public class PDFContext
   {
      public HttpServerUtilityBase Server { get; set; }
   }

}
