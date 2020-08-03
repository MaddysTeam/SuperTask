using Business;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TheSite.Models
{

	public class FolderViewModel
	{
		public Guid id { get; set; }
		public string name { get; set; }
		public string path { get; set; }
		public int sortId { get; set; }
		public Guid parentId { get; set; }
		public string coverPath { get; set; }
		public bool isMyFolder { get; set; }
		public double fileCount { get; set; }
	}

	public class FileViewModel
	{
		public Guid id { get; set; }
		[Display(Name = "文件名称")]
		public string name { get; set; }
		public string path { get; set; }
		public Guid folderId { get; set; }
		public string fileExtName { get; set; }
		public Guid attachmentId { get; set; }
		public int size { get; set; }
		public string coverPath { get; set; }
		public bool canPreview { get; set; }
		public bool isMyFile { get; set; }
	}

   public class FolderPemissionViewModel
   {
      public Guid FolderId { get; set; }
      public Guid FileId { get; set; }
      public string UserIds { get; set; }
      public string PermissionIds { get; set; }
      public bool IsRecusive { get; set; }
	  [Display(Name ="文件夹")]
      public string FolderName { get; set; }

      public Guid[] UserGuids => UserIds?.Split(',')?.ConvertToGuidArray();
      public Guid[] PermissionGuids => PermissionIds?.Split(',')?.ConvertToGuidArray();

   }


}