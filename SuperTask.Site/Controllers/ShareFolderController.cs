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

      static APDBDef.FolderTableDef f = APDBDef.Folder;
      static APDBDef.FolderFileTableDef ff = APDBDef.FolderFile;
      static APDBDef.AttachmentTableDef at = APDBDef.Attachment;
      static APDBDef.FolderPermissionTableDef fp = APDBDef.FolderPermission;

      // GET: ShareFolder/Index

      public ActionResult Index(Guid? projectId)
      {
         if (null != projectId &&
            !ResourceHelper.HasPermission(GetUserInfo().UserId, projectId.Value, "P_10006", new APDBDef()))
         {
            throw new ApplicationException(Errors.Project.NOT_ALLOWED_VISIT_FOLDER);
         }

         if (projectId == null)
         {
            return View(Guid.Empty);
         }

         var project = ProjectHelper.GetCurrentProject(projectId.Value);

         return View(project.FolderId);
      }

      [HttpPost]
      public ActionResult Search(Guid parentId, string projectId, string phrase)
      {
         var userId = GetUserInfo().UserId;
         var allFolders = GetAllFolders();
         var folders = new List<FolderViewModel>();
         var files = GetFiles(phrase);
         var parents = allFolders.FindAll(f => f.id == parentId);

         // if project is null or empty ,then means show all  folders but without project folder
         Guid? neglectId = null;
         if (string.IsNullOrEmpty(projectId))        
            neglectId = ShareFolderKeys.RootProjectFolderId;

         var children = FindChildFolders(parentId, GetAllFolders(), new List<FolderViewModel>(), files, neglectId);
         if (children != null && children.Count > 0)
         {
            folders.AddRange(parents.Concat(children));
         }
         if (!parentId.IsEmpty()) //如果不是root再找其祖先节点
         {
            var currentFolder = allFolders.Find(x => x.id == parentId);
            while (true)
            {
               if (!folders.Exists(x => x.id == currentFolder.id))
                  folders.Add(currentFolder);

               if (currentFolder == null || currentFolder.parentId.IsEmpty())
                  break;

               currentFolder = allFolders.Find(x => x.id == currentFolder.parentId);
            }
         }

         if (neglectId != null)
            folders.RemoveAll(x => x.id == neglectId);

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

            if (!HasFolderPermission(GetUserInfo().UserId, folder.FolderId, ShareFolderKeys.EditPermission))
               return PartialView("_noPermission", Errors.FolderPermission.NOT_ALLOWED_EDIT);
         }
         else
         {
            folder = new Folder { ParentId = parentId == null ? Guid.Empty : parentId.Value };

            if (!HasFolderPermission(GetUserInfo().UserId, folder.ParentId, ShareFolderKeys.EditPermission))
               return PartialView("_noPermission", Errors.FolderPermission.NOT_ALLOWED_EDIT);
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

      public ActionResult FileEdit(Guid? id, Guid? folderId)
      {
         FileViewModel model = new FileViewModel();
         if (id != null)
         {
            model = GetFiles().FirstOrDefault(x => x.id == id);
            if (!HasFilePermission(GetUserInfo().UserId, id.Value, ShareFolderKeys.EditPermission))
               return PartialView("_noPermission", Errors.FilePermission.NOT_ALLOWED_EDIT);
         }
         else
         {
            if (!HasFolderPermission(GetUserInfo().UserId, folderId.Value, ShareFolderKeys.UploadPermission))
               return PartialView("_noPermission", Errors.FolderPermission.NOT_ALLOWED_UPLOAD);
         }

         model.folderId = folderId ?? Guid.Empty;

         return PartialView(model);
      }

      [HttpPost]
      public ActionResult FileEdit(FileViewModel file)
      {
         if (file.id.IsEmpty())
         {
            if (GetFiles(file.name, file.folderId).Count > 0)
            {
               return Json(new
               {
                  result = AjaxResults.Error,
                  msg = Errors.Files.DUPLICATE_FILE_NAME
               });
            }

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

         //TODO: add permission from parent folder

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Folder.EDIT_SUCCESS
         });
      }


      // Post-ajax: ShareFolder/Del 

      public ActionResult Delete()
      {
         var folderId = Request["id"].ToGuid(Guid.Empty);
         if (!HasFolderPermission(GetUserInfo().UserId, folderId, ShareFolderKeys.DeletePermission))
            return PartialView("_noPermission", Errors.FolderPermission.NOT_ALLOWED_DELETE);

         return PartialView("_deleteFolder", folderId.ToString());
      }

      [HttpPost]
      public ActionResult Delete(Guid id)
      {
         var delfolders = FindChildFolders(id,
            GetAllFolders(),
            new List<FolderViewModel>(),
            null);

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

      public ActionResult DelFile()
      {
         var fileId = Request["id"].ToGuid(Guid.Empty);
         if (!HasFolderPermission(GetUserInfo().UserId, fileId, ShareFolderKeys.DeletePermission))
            return PartialView("_noPermission", Errors.FilePermission.NOT_ALLOWED_DELETE);

         return PartialView("_deleteFile", fileId.ToString());
      }

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


      [HttpPost]
      public ActionResult PasteFile(Guid id)
      {
         return null;
      }


      // Get: ShareFolder/SendToFolder 
      // Get: ShareFolder/SendFileToFolder 

      public ActionResult SendToFolder(Guid itemId, Guid originFolderId)
      {
         return PartialView("_folders");
      }

      public ActionResult SendFileToFolder(Guid folderFileId, Guid originFolderId, Guid targetFolderId)
      {
         var folderFile = db.FolderFileDal.PrimaryGet(folderFileId);
         var attachment = db.AttachmentDal.PrimaryGet(folderFile.AttachmentId);
         if (attachment != null && GetFiles(attachment.RealName, targetFolderId).Count > 0)
         {
            return Json(new
            {
               result = AjaxResults.Error,
               msg = Errors.Files.DUPLICATE_FILE_NAME
            });
         }

         db.BeginTrans();

         try
         {
            db.FolderFileDal.ConditionDelete(ff.AttachmentId == attachment.AttachmentId & ff.FolderId == originFolderId);
            db.FolderFileDal.Insert(new FolderFile { FolderFileId = Guid.NewGuid(), AttachmentId = attachment.AttachmentId, FolderId = targetFolderId });

            db.Commit();
         }
         catch (Exception e)
         {
            db.Rollback();

            return Json(new
            {
               result = AjaxResults.Error,
               msg = e.Message,
            });
         }

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.File.EDIT_SUCCESS
         });
      }


      // GEt: ShareFolder/SetFolderPermission 
      // Get: ShareFolder/SetFilePermission
      // POST-Ajax: ShareFolder/SetFolderPermission 
      // POST-Ajax: ShareFolder/SetFilePermission 

      public ActionResult SetFolderPermission(Guid id)
      {
         if (!HasFolderPermission(GetUserInfo().UserId, id, ShareFolderKeys.SetPermission))
            return PartialView("_noPermission", Errors.FolderPermission.NOT_ALLOWED_SET_PERMISSION);

         // all users
         ViewBag.Users = UserHelper.GetAvailableUser(db);
         // all permission from dictionary
         ViewBag.Permissions = DictionaryHelper.GetSubTypeDics(ShareFolderKeys.Permission);

         FolderPemissionViewModel viewModel = new FolderPemissionViewModel();
         var models = db.FolderPermissionDal.ConditionQuery(fp.FolderId == id, null, null, null);

         if (models.Count > 0)
         {
            viewModel.PermissionIds = string.Join(",", models.Select(x => x.PermissionId));
            viewModel.UserIds = string.Join(",", models.Select(x => x.UserId));
         }

         Folder folder = Folder.PrimaryGet(id);
         viewModel.FolderId = folder.FolderId;
         viewModel.FolderName = folder.FolderName;

         return PartialView("_permission", viewModel);
      }


      public ActionResult SetFilePermission(Guid id)
      {
         if (!HasFilePermission(GetUserInfo().UserId, id, ShareFolderKeys.SetPermission))
            return PartialView("_noPermission", Errors.FilePermission.NOT_ALLOWED_SET_PERMISSION);

         // all users
         ViewBag.Users = UserHelper.GetAvailableUser(db);
         // all permission from dictionary
         ViewBag.Permissions = DictionaryHelper.GetSubTypeDics(ShareFolderKeys.Permission);

         FolderPemissionViewModel viewModel = new FolderPemissionViewModel();
         var models = db.FolderPermissionDal.ConditionQuery(fp.FileId == id, null, null, null);
         if (models.Count > 0)
         {
            viewModel.PermissionIds = string.Join(",", models.Select(x => x.PermissionId));
            viewModel.UserIds = string.Join(",", models.Select(x => x.UserId));
         }
         var subQuery = APQuery.select(ff.AttachmentId).from(ff).where(ff.FolderFileId == id);
         Attachment file = db.AttachmentDal.ConditionQuery(at.AttachmentId.In(subQuery), null, null, null).FirstOrDefault();

         viewModel.FileName = file.RealName;
         viewModel.FileId = id;

         return PartialView("_permission", viewModel);
      }

      /// <summary>
      /// recursively set permission to folder and its file ,
      /// be caution if permission exists in current child folder or files which need to be retaind
      /// </summary>
      /// <param name="model"></param>
      /// <returns></returns>
      [HttpPost]
      public ActionResult SetFolderPermission(FolderPemissionViewModel model)
      {
         var allFolders = GetAllFolders();
         // var allPermissions = FolderPermission.GetAll();
         var currentFolder = allFolders.Find(x => x.id == model.FolderId);
         var parentId = model.FolderId;
         var folders = FindChildFolders(parentId, GetAllFolders(), new List<FolderViewModel>(), null);
         folders.Add(currentFolder);
         var folderIds = folders.Select(x => x.id).ToArray();
         var files = db.FolderFileDal.ConditionQuery(ff.FolderId.In(folderIds), null, null, null);

         // delete permission
         db.FolderPermissionDal.ConditionDelete(fp.FolderId.In(folders.Select(x => x.id).ToArray()) | fp.FileId.In(files.Select(x => x.FolderFileId).ToArray()));

         if (folders.Count > 0)
         {
            foreach (var item in folders)
               foreach (var userId in model.UserGuids)
                  foreach (var permissionId in model.PermissionGuids)
                     db.FolderPermissionDal.Insert(new FolderPermission(item.id, Guid.Empty, userId, permissionId));
         }

         if (files.Count > 0)
         {
            foreach (var item in files)
               foreach (var userId in model.UserGuids)
                  foreach (var permissionId in model.PermissionGuids)
                     db.FolderPermissionDal.Insert(new FolderPermission(item.FolderId, item.FolderFileId, userId, permissionId));
         }

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Folder.EDIT_SUCCESS
         });
      }

      [HttpPost]
      public ActionResult SetFilePermission(FolderPemissionViewModel model)
      {
         if (model.FileId.IsEmpty())
            return Json(new
            {
               result = AjaxResults.Error,
               msg = Errors.Files.FILE_ID_IS_REQUIRED
            });

         db.FolderPermissionDal.ConditionDelete(fp.FileId == model.FileId);

         foreach (var userId in model.UserGuids)
            foreach (var permissionId in model.PermissionGuids)
               db.FolderPermissionDal.Insert(new FolderPermission(model.FolderId, model.FileId, userId, permissionId));

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.File.EDIT_SUCCESS
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

         if (!HasFilePermission(GetUserInfo().UserId, id.Value, ShareFolderKeys.PreviewPermission))
            return PartialView("_noPermission", Errors.FilePermission.NOT_ALLOWED_PREVIEW);

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


      #region [ private ]

      private List<FolderViewModel> FindChildFolders(Guid parentId, List<FolderViewModel> folders, List<FolderViewModel> result, List<FileViewModel> allFiles, Guid? neglectId = null)
      {
         FolderViewModel folder = null;
         for (var i = 0; i < folders.Count; i++)
         {
            folder = folders[i];
            folder.coverPath = HasChildItem(folder.id, allFiles, folders) ? AttahmentKeys.DefaultFolderIcoPath : AttahmentKeys.EmptyFolderIcoPath;
            if (folder.parentId == parentId && parentId != neglectId)
            {
               if (!result.Exists(x => x.id == folder.id))
                  result.Add(folders[i]);

               FindChildFolders(folder.id, folders, result, allFiles, neglectId);
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

      private bool HasChildItem(Guid parentFolderId, List<FileViewModel> files, List<FolderViewModel> folders)
      {
         var fileCount = files == null ? 0 : files.Count(x => x.folderId == parentFolderId);
         var folderCount = folders == null ? 0 : folders.Count(x => x.parentId == parentFolderId);

         return fileCount + folderCount > 0;
      }

      private List<FileViewModel> GetFiles(string phrase = null, Guid? folderId = null)
      {
         var query = APQuery.select(ff.FolderId, ff.FolderFileId.As("folderFileId"), at.Asterisk)
                .from(ff, at.JoinLeft(at.AttachmentId == ff.AttachmentId)
                ).order_by(at.RealName.Asc);

         if (folderId != null)
         {
            query = query.where(ff.FolderId == folderId);
         }

         if (!string.IsNullOrEmpty(phrase))
         {
            query = query.where_and(at.RealName.Match(phrase)
                 | at.FileExtName.Match(phrase));
         }

         var result = query.query(db, r =>
                 {
                    var path = at.Url.GetValue(r);
                    var ext = at.FileExtName.GetValue(r);
                    if (string.IsNullOrEmpty(ext))
                       ext = System.IO.Path.GetExtension(path);
                    var cover = !string.IsNullOrEmpty(ext) && AttahmentKeys.FileIcos.ContainsKey(ext) ?
                    AttahmentKeys.FileIcos[ext] : AttahmentKeys.DefaultFileIcoPath;
                    return new FileViewModel
                    {
                       id = ff.FolderFileId.GetValue(r, "folderFileId"),
                       attachmentId = at.AttachmentId.GetValue(r),
                       name = at.RealName.GetValue(r),
                       path = path,
                       coverPath = cover,
                       folderId = ff.FolderId.GetValue(r),
                       isMyFile = at.PublishUserId.GetValue(r) == GetUserInfo().UserId
                    };
                 }).ToList();

         return result;
      }

      private bool HasFolderPermission(Guid Userid, Guid folderId, Guid permissionId)
      {
         var folder = Folder.PrimaryGet(folderId);
         return (folder != null && folder.OperatorId == Userid) ||
              db.FolderPermissionDal.ConditionQueryCount(fp.UserId == Userid & fp.FolderId == folderId & fp.PermissionId == permissionId) > 0;
      }

      private bool HasFilePermission(Guid Userid, Guid fileId, Guid permissionId)
      {
         var subQuery = APQuery.select(ff.AttachmentId).from(ff).where(ff.FolderFileId == fileId);
         var attachment = Attachment.ConditionQuery(at.AttachmentId.In(subQuery), null).FirstOrDefault();

         return (attachment != null && attachment.PublishUserId == Userid) ||
                db.FolderPermissionDal.ConditionQueryCount(fp.UserId == Userid & fp.FileId == fileId & fp.PermissionId == permissionId) > 0;
      }

      #endregion

   }

}