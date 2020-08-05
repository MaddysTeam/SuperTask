using System;
using System.Collections.Generic;

namespace Business.Helper
{

	public class ShareFolderHelper
	{
		static APDBDef.FolderTableDef f = APDBDef.Folder;

		public static Folder CreateFolder(Guid folderId, string folderName, Guid parentId, Guid type, Guid operatorId, APDBDef db)
		{
			var folder = new Folder
			{
				FolderId = folderId.IsEmpty() ? Guid.NewGuid() : folderId,
				FolderName = folderName,
				OperatorId = operatorId,
				ParentId = parentId,
				FolderType = type
			};

			db.FolderDal.Insert(folder);

			return folder;
		}


		public static void UploadFolderFile(Guid folderId, Guid fileId, APDBDef db)
		=> db.FolderFileDal.Insert(new FolderFile(Guid.NewGuid(), folderId, fileId));


		public static List<Folder> GetSubFolders(Guid parentId, APDBDef db)
		=> db.FolderDal.ConditionQuery(f.ParentId == parentId, null, null, null)?? new List<Folder>();


	}

}