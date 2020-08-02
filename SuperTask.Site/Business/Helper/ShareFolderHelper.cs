using System;

namespace Business.Helper
{

   public class ShareFolderHelper
   {

      public static Folder CreateFolder(Guid folderId, string folderName, Guid parentId, Guid operatorId, APDBDef db)
      {
         var mileStonFolder = new Folder
         {
            FolderId = folderId.IsEmpty() ? Guid.NewGuid() : folderId,
            FolderName = folderName,
            OperatorId = operatorId,
            ParentId = parentId
         };

         db.FolderDal.Insert(mileStonFolder);

         return mileStonFolder;
      }

   }

}