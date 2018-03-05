using Business.Helper;
using Symber.Web.Data;
using System;
using System.Collections.Generic;
using TheSite.Models;
using System.Web;

namespace Business
{

   public class PDFHandler : IHandler<Attachment, PDFContext>
   {
      public virtual void Handle(Attachment orginal, PDFContext context) { }
   }


   public class WordToPDFHandler : PDFHandler
   {
      public override void Handle(Attachment orginal, PDFContext context)
      {
         var attachment = orginal;
         var server = context.Server;
         var filePath = server.MapPath("~" + attachment.Url);
         var pdfVirtualPath = attachment.Url.Replace(attachment.FileExtName, ".pdf");
         var pdfPath = server.MapPath("~" + pdfVirtualPath);
         if (!System.IO.File.Exists(pdfPath))
         {
            PDFHelper.WordToPDF(filePath, pdfPath);
         }

         attachment.Url = pdfVirtualPath;
      }
   }


   public class ExcelToPDFHandler : PDFHandler
   {
      public override void Handle(Attachment orginal, PDFContext context)
      {
         var attachment = orginal;
         var server = context.Server;
         var filePath = server.MapPath("~" + attachment.Url);
         var pdfVirtualPath = attachment.Url.Replace(attachment.FileExtName, ".pdf");
         var pdfPath = server.MapPath("~" + pdfVirtualPath);
         if (!System.IO.File.Exists(pdfPath))
         {
            PDFHelper.ExcelToPdf(filePath, pdfPath);
         }

         attachment.Url = pdfVirtualPath;
      }
   }

   public class PPTToPDFHandler : PDFHandler
   {
      public override void Handle(Attachment orginal, PDFContext context)
      {
         var attachment = orginal;
         var server = context.Server;
         var filePath = server.MapPath("~" + attachment.Url);
         var pdfVirtualPath = attachment.Url.Replace(attachment.FileExtName, ".pdf");
         var pdfPath = server.MapPath("~" + pdfVirtualPath);
         if (!System.IO.File.Exists(pdfPath))
         {
            PDFHelper.PPTToPDF(filePath, pdfPath);
         }

         attachment.Url = pdfVirtualPath;
      }
   }


   public class PDFContext
   {
      public HttpServerUtilityBase Server { get; set; }
   }

}
