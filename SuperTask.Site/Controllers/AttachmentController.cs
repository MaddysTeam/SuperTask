using Business;
using Business.Config;
using Symber.Web.Data;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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
            string savepath = GetDirForSaveing();

            string mappedDir = Server.MapPath("~" + savepath);
            if (!Directory.Exists(mappedDir))
               Directory.CreateDirectory(mappedDir);


            file.SaveAs(Path.Combine(mappedDir, filename));

            string url = savepath + "/" + filename;

            // 返回结果
            return Json(new
            {
               result = AjaxResults.Success,
               url = url,
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