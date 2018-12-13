using Business;
using Business.Helper;
using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TheSite.Models;

namespace TheSite.Controllers
{

   public class ShareFolderController : BaseController
   {

      APDBDef.FolderTableDef f = APDBDef.Folder;
      APDBDef.FolderFileTableDef ff = APDBDef.FolderFile;
      APDBDef.AttachmentTableDef at = APDBDef.Attachment;


      // GET: ShareFolder/Index

      public ActionResult Index()
      {
         return View();
      }

      [HttpPost]
      public ActionResult Search(string phrase)
      {
         var folders = GetAllFolders();
         var files = GetFiles(phrase);

         return Json(new
         {
            rows = new { folders = folders, files = files },
         });
      }

      // GET: ShareFolder/Edit
      // Post-ajax: ShareFolder/Edit 

      public ActionResult Edit(Guid? parentId, Guid? id)
      {
         Folder folder = null;
         if (id != null)
         {
            folder = Folder.PrimaryGet(id.Value);
         }
         else
         {
            folder = new Folder { ParentId = parentId == null ? Guid.Empty : parentId.Value };
         }

         return PartialView(folder);
      }

      [HttpPost]
      public ActionResult Edit(Folder folder)
      {
         var parentFolder = Folder.PrimaryGet(folder.ParentId);
         if (folder.FolderId.IsEmpty())
         {
            folder.FolderId = Guid.NewGuid();
            folder.FolderPath = parentFolder == null ? $"{folder.FolderName}" : $"{parentFolder.FolderPath} > {folder.FolderName}";
            folder.OperatorId = GetUserInfo().UserId;

            db.FolderDal.Insert(folder);
         }
         else
         {
            folder.OperatorId = GetUserInfo().UserId;
            db.FolderDal.Update(folder);
         }

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Folder.EDIT_SUCCESS
         });
      }


      // GET: ShareFolder/FileEdit
      // Post-ajax: ShareFolder/FileEdit 

      public ActionResult FileEdit(Guid? id,Guid? folderId)
      {
         FileViewModel model = new FileViewModel();
         if (id != null)
            model = GetFiles().FirstOrDefault(x => x.id == id);

         folderId = folderId ?? Guid.Empty;
         model.folderId = folderId.Value;

         return PartialView(model);
      }

      [HttpPost]
      public ActionResult FileEdit(FileViewModel file)
      {
         if (file.id.IsEmpty())
         {
            var attachmentId = Guid.NewGuid();
            var foldFile = new FolderFile { FolderFileId = Guid.NewGuid(), FolderId = file.folderId, AttachmentId = attachmentId };
            var attachment = new Attachment
            {
               AttachmentId = attachmentId,
               Url = file.path,
               RealName = file.name,
               FileExtName = file.fileExtName,
               PublishUserId = GetUserInfo().UserId,
               UploadDate = DateTime.Now,
            };

            db.AttachmentDal.Insert(attachment);
            db.FolderFileDal.Insert(foldFile);
         }
         else
         {
            db.AttachmentDal.UpdatePartial(file.attachmentId, new { RealName = file.name });
         }

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Folder.EDIT_SUCCESS
         });
      }


      // Post-ajax: ShareFolder/Del 

      [HttpPost]
      public ActionResult Delete(Guid id)
      {
         var delfolders = FindChildFolders(id,
            GetAllFolders(),
            new List<FolderViewModel>());

         delfolders.Add(new FolderViewModel { id = id });


         db.BeginTrans();

         try
         {
            foreach (var item in delfolders)
            {
               db.FolderFileDal.ConditionDelete(ff.FolderId == item.id);
               db.FolderDal.PrimaryDelete(item.id);
            }

            db.Commit();
         }
         catch
         {
            db.Rollback();
         }

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Folder.EDIT_SUCCESS
         });
      }

      // Post-ajax: ShareFolder/DelFile 

      [HttpPost]
      public ActionResult DelFile(Guid id)
      {
         var file = db.FolderFileDal.PrimaryGet(id);

         db.BeginTrans();

         try
         {
            db.AttachmentDal.ConditionDelete(at.AttachmentId == file.AttachmentId);
            db.FolderFileDal.ConditionDelete(ff.FolderFileId == id);

            db.Commit();
         }
         catch
         {
            db.Rollback();
         }

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.File.DELETE_SUCCESS
         });
      }


      // GET: ShareFolder/FileReview

      public ActionResult FilePreview(Guid? id)
      {
         if (id == null)
            throw new ApplicationException("file id is null");

         var file = db.FolderFileDal.PrimaryGet(id.Value);
         if (file == null)
            throw new ApplicationException("file instance is null");

         var attachment = db.AttachmentDal.PrimaryGet(file.AttachmentId);

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


      private List<FolderViewModel> FindChildFolders(Guid parentId, List<FolderViewModel> folders, List<FolderViewModel> result)
      {
         FolderViewModel folder = null;
         for (var i = 0; i < folders.Count; i++)
         {
            folder = folders[i];
            if (folder.parentId == parentId)
            {
               result.Add(folders[i]);
               FindChildFolders(folder.id, folders, result);
            }
         }

         return result;
      }

      private List<FolderViewModel> GetAllFolders()
      {
         return APQuery.select(f.Asterisk)
                             .from(f)
                             .order_by(f.FolderName.Asc)
                             .query(db, r =>
                             {
                                return new FolderViewModel
                                {
                                   id = f.FolderId.GetValue(r),
                                   parentId = f.ParentId.GetValue(r),
                                   name = f.FolderName.GetValue(r),
                                   path = f.FolderPath.GetValue(r),
                                   coverPath = AttahmentKeys.DefaultFolderIcoPath,
                                   sortId = f.SortId.GetValue(r),
                                   isMyFolder = f.OperatorId.GetValue(r) == GetUserInfo().UserId
                                };
                             }).ToList();
      }

      private List<FileViewModel> GetFiles(string phrase = null)
      {
         var query =APQuery.select(ff.FolderId, ff.FolderFileId.As("folderFileId"), at.Asterisk)
                        .from(ff, at.JoinLeft(at.AttachmentId == ff.AttachmentId)
                        ).order_by(at.RealName.Asc);

         if (!string.IsNullOrEmpty(phrase))
         {
            query = query.where(at.RealName.Match(phrase)
                      | at.FileExtName.Match(phrase));
         }

         var result = query.query(db, r =>
                          {
                             var ext = at.FileExtName.GetValue(r);
                             var cover = AttahmentKeys.FileIcos.ContainsKey(ext) ?
                                         AttahmentKeys.FileIcos[ext] : AttahmentKeys.DefaultFileIcoPath;
                             return new FileViewModel
                             {
                                id = ff.FolderFileId.GetValue(r, "folderFileId"),
                                attachmentId = at.AttachmentId.GetValue(r),
                                name = at.RealName.GetValue(r),
                                path = at.Url.GetValue(r),
                                coverPath = cover,
                                folderId = ff.FolderId.GetValue(r),
                                isMyFile = at.PublishUserId.GetValue(r) == GetUserInfo().UserId
                             };
                          }).ToList();

         return result;
      }

   }

}