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

			var project = ProjectrHelper.GetCurrentProject(projectId.Value);
			return View(project.FolderId);
		}

		[HttpPost]
		public ActionResult Search(Guid parentId, string phrase)
		{
			double fileCount = 0;
			var allFolders = GetAllFolders();
			var folders = new List<FolderViewModel>();
			var files = GetFiles(phrase);
			var parents = allFolders.FindAll(f => f.id == parentId);
			var children = FindChildFolders(parentId, GetAllFolders(), new List<FolderViewModel>(), files, ref fileCount);
			if (children != null && children.Count > 0)
			{
				folders.AddRange(parents.Concat(children));
			}
			else //如果没有子文件夹则向上搜索
			{
				var currentFolder = allFolders.Find(x => x.id == parentId);
				while (true)
				{
					folders.Add(currentFolder);

					if (currentFolder.parentId.IsEmpty())
						break;

					currentFolder = allFolders.Find(x => x.id == currentFolder.parentId);
				}
			}

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

		public ActionResult FileEdit(Guid? id, Guid? folderId)
		{
			FileViewModel model = new FileViewModel();
			if (id != null)
				model = GetFiles().FirstOrDefault(x => x.id == id);

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
			double count = 0;
			var delfolders = FindChildFolders(id,
			   GetAllFolders(),
			   new List<FolderViewModel>(),
			   null
			   , ref count);

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


		// GEt: ShareFolder/SetPermission 
		// POST-Ajax: ShareFolder/SetPermission 
		public ActionResult SetPermission(Guid id, Guid? fileId)
		{
			// all users
			ViewBag.Users = UserHelper.GetAvailableUser(db);
			// all permission from dictionary
			ViewBag.Permissions = DictionaryHelper.GetSubTypeDics(ShareFolderKeys.Permission);

			Folder folder = Folder.PrimaryGet(id);
			FolderPemissionViewModel viewModel = new FolderPemissionViewModel();
			List<FolderPermission> models = new List<FolderPermission>();
			if (fileId == null || fileId.Value.IsEmpty())
				models = db.FolderPermissionDal.ConditionQuery(fp.FolderId == id, null, null, null);
			else
				models = db.FolderPermissionDal.ConditionQuery(fp.FileId == fileId, null, null, null);

			if (models.Count > 0)
			{
				viewModel.FileId = models[0].FileId;
				viewModel.PermissionIds = string.Join(",", models.Select(x => x.PermissionId));
				viewModel.UserIds = string.Join(",", models.Select(x => x.UserId));
			}

			viewModel.FolderId = folder.FolderId;
			viewModel.FolderName = folder.FolderName;

			return PartialView("_permission", viewModel);
		}


		/// <summary>
		/// recursively set permission to folder and its file ,
		/// be caution if permission exits in current child folder or files which need to be retaind
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult SetPermission(FolderPemissionViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return Json(new
				{
					result = AjaxResults.Error
				});
			}

			var allFolders = GetAllFolders();
			// var allPermissions = FolderPermission.GetAll();
			var currentFolder = allFolders.Find(x => x.id == model.FolderId);
			var parentId = model.FolderId;
			double fileCount = 0;

			// set permission by recursive
			if (model.FileId.IsEmpty() && !parentId.IsEmpty()) // set permissio	n for folder only
			{
				var children = FindChildFolders(parentId, GetAllFolders(), new List<FolderViewModel>(), null, ref fileCount);
				if (children.Count > 0)
					db.FolderPermissionDal.ConditionDelete(fp.FolderId.In(children.Select(x => x.id).ToArray()));

				foreach (var item in children)
					foreach (var userId in model.UserGuids)
						foreach (var permissionId in model.PermissionGuids)
							db.FolderPermissionDal.Insert(new FolderPermission(item.id, model.FileId, userId, permissionId));
			}
			// set folder file permissoin 
			else if (!model.FileId.IsEmpty())
			{
				db.FolderPermissionDal.ConditionDelete(fp.FileId == model.FileId);

				foreach (var userId in model.UserGuids)
					foreach (var permissionId in model.PermissionGuids)
						db.FolderPermissionDal.Insert(new FolderPermission(model.FolderId, model.FileId, userId, permissionId));
			}

			return Json(new
			{
				result = AjaxResults.Success
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


		private List<FolderViewModel> FindChildFolders(Guid parentId, List<FolderViewModel> folders, List<FolderViewModel> result, List<FileViewModel> allFiles, ref double fileCount)
		{
			FolderViewModel folder = null;
			for (var i = 0; i < folders.Count; i++)
			{
				folder = folders[i];
				folder.coverPath = HasChildItem(folder.id, allFiles, folders) ? AttahmentKeys.DefaultFolderIcoPath : AttahmentKeys.EmptyFolderIcoPath;
				if (folder.parentId == parentId)
				{
					if (!result.Exists(x => x.id == folder.id))
						result.Add(folders[i]);

					FindChildFolders(folder.id, folders, result, allFiles, ref fileCount);
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
							 var ext = at.FileExtName.GetValue(r);
							 var cover = !string.IsNullOrEmpty(ext) && AttahmentKeys.FileIcos.ContainsKey(ext) ?
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