using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Business.Helper
{

   public static class AttahmentKeys
   {

      public static string DefaultFolderIcoPath => GetVirtualPath("folder_full.gif");
      public static string DefaultFileIcoPath => GetVirtualPath("");
      public static string ImageSuffix= ".jpg,.gif,.jpeg,.png,.bnp";
      public static string documentSuffix = ".doc,.docx,.xls,.xlsx,.pdf,.pptx";
      public static Dictionary<string, string> FileIcos => new Dictionary<string, string>
         {
           {".jpg",GetVirtualPath("jpeg.png")},
           {".gif",GetVirtualPath("gif.png")},
           {".jpeg",GetVirtualPath("jpeg.png")},
           {".png",GetVirtualPath("png.png")},
           {".bmp",GetVirtualPath("bmp.png")},

           {".rar",GetVirtualPath("rar.png")},
           {".zip",GetVirtualPath("zip.png")},

           {".txt",GetVirtualPath("text.png")},
           {".doc",GetVirtualPath("docx_win.png")},
           {".docx",GetVirtualPath("docx_win.png")},
           {".xlsx",GetVirtualPath("xlsx_win.png")},
           {".wps",GetVirtualPath("docx_win.png")},
           {".rtf",GetVirtualPath("docx_win.png")},
           {".pdf",GetVirtualPath("pdf.png")},
           {".html",GetVirtualPath("html.png")},
           {".css",GetVirtualPath("css.png")},
           {".pptx",GetVirtualPath("pptx_win.png")},

           {".mp3",GetVirtualPath("mp3.png")},
           {".wav",GetVirtualPath("wav.png")},
           {".wma",GetVirtualPath("wma.png")},

           {".mp4",GetVirtualPath("mp3.png")},
           {".flv",GetVirtualPath("fla.png")},
           {".swf",GetVirtualPath("fla.png")},
           {".wmv",GetVirtualPath("wmv.png")},
           {".avi",GetVirtualPath("avi.png")},
        };

      public static Dictionary<string, string> FolderCoverPath => new Dictionary<string, string>
         {
           {"notEmpty",GetVirtualPath("folder_full.gif")},
           {"empty",GetVirtualPath("folder.gif")},
        };

      private static string GetVirtualPath(string fileName) => $"/assets/img/icon/{fileName}";

      public static Guid DefaultCategoryId = Guid.Parse("CD4BDC7F-A56D-3C59-9CAF-D3A2B7C9AB6D");

   }

}