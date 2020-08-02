using Business.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Business
{

   public partial class Folder
   {

      public bool HasPermission(Guid userId, Guid folderId)
      {
         return true;
      }

   }


   public partial class FolderFile
   {

      public bool HasPermission()
      {
         return true;
      }

   }

}