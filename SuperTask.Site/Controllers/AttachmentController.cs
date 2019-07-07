using Business.Config;
using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using TheSite.Models;

namespace TheSite.Controllers
{

   public class AttachmentController : BaseController
   {

      [HttpPost]
      public ActionResult UploadFile(HttpPostedFileBase file)
      {
         ThrowNotAjax();

         try
         {
            string filename = DateTime.Now.ToString("yyyyMMddHHmmssffff") + file.FileName.Substring(file.FileName.IndexOf('.'));
            string mappedDir = Server.MapPath("~" + GetDirForSaveing());
            if (!Directory.Exists(mappedDir))
               Directory.CreateDirectory(mappedDir);

            var fileUrl= Path.Combine(mappedDir, filename);
            file.SaveAs(fileUrl);

            // 返回结果
            return Json(new
            {
               result = AjaxResults.Success,
               url = fileUrl,
               filename = file.FileName,
               size = file.ContentLength,
               ext = Path.GetExtension(file.FileName),
               msg = "文件已保存成功"
            });
         }
         catch (Exception ex)
         {
            // 返回结果
            return Json(new
            {
               result = AjaxResults.Error,
               msg = ex.Message
            });
         }

      }

      
      private string GetDirForSaveing()
      {
         return ThisApp.UploadFilePath + DateTime.Now.ToString("yyyyMMdd");
      }

   }

}