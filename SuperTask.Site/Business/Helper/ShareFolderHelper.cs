using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using TheSite.Models;

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