using Business;
using Business.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TheSite.Controllers
{
   public class JsonController : BaseController
   {

      public ActionResult GetFolder()
      {
       
         var list = new List<json_treenode>();

         list.Add(new json_treenode { id = TaskKeys.RequreFileType.ToString(), text = "需求类", type = json_treenode_types.folder, children = new List<json_treenode>() });
         list.Add(new json_treenode { id = TaskKeys.DesignFileType.ToString(), text = "设计类", type = json_treenode_types.folder, children = new List<json_treenode>() });
         list.Add(new json_treenode { id = TaskKeys.CheckAndAcceptFileType.ToString(), text = "验收类", type = json_treenode_types.folder, children = new List<json_treenode>() });
         list.Add(new json_treenode { id = TaskKeys.DeliverFileType.ToString(), text = "交付类", type = json_treenode_types.folder, children = new List<json_treenode>() });

         var root = new json_treenode { id = Guid.NewGuid().ToString(), text = "文档分类", type = json_treenode_types.folder, children = list };

         return Json(root, JsonRequestBehavior.AllowGet);
      }

   }
}