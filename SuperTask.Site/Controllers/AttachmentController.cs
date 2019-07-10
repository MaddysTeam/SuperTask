using Business;
using Business.Config;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TheSite.Models;

namespace TheSite.Controllers
{

	public class AttachmentController : BaseController
	{
		static APDBDef.AttachmentTableDef at = APDBDef.Attachment;

		// AJAX-POST: /Attachment/UploadFile

		[HttpPost]
		public ActionResult UploadFile(HttpPostedFileBase file)
		{
			ThrowNotAjax();

			try
			{
				string savepath = GetDirForSaveing();
				string filename = DateTime.Now.ToString("yyyyMMddHHmmssffff") + file.FileName.Substring(file.FileName.IndexOf('.'));
				string mappedDir = Server.MapPath("~" + GetDirForSaveing());
				if (!Directory.Exists(mappedDir))
					Directory.CreateDirectory(mappedDir);

				file.SaveAs(Path.Combine(mappedDir, filename));

				// 返回结果
				return Json(new
				{
					result = AjaxResults.Success,
					url = savepath + "/" + filename,
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


		// GET: /Attachment/Preview

		public ActionResult Preview(Guid? id, string url = null)
		{
			Attachment attachment = null;
			if (id == null)
			{
				attachment = db.AttachmentDal.ConditionQuery(at.Url==url,null,null,null).FirstOrDefault();
			}
			else
			{
				attachment = db.AttachmentDal.PrimaryGet(id.Value);
			}

			if(attachment==null)
				throw new ApplicationException("file not exist");

			if (HandleManager.PDFHandlers.ContainsKey(attachment.FileExtName))
				HandleManager.PDFHandlers[attachment.FileExtName].Handle(attachment, new PDFContext { Server = Server });

			return PartialView(new FileViewModel
			{
				attachmentId = attachment.AttachmentId,
				fileExtName = attachment.FileExtName,
				name = attachment.RealName,
				path = attachment.Url
			});
		}


		private string GetDirForSaveing()
		{
			return ThisApp.UploadFilePath + DateTime.Now.ToString("yyyyMMdd");
		}

	}

}