using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheSite.Models
{

   public class ArrangeTaskViewModel
   {

      public string id { get; set; }
      public string name { get; set; }
      public string code { get; set; }
      public int level { get; set; }
      public string stat { get; set; }
      public double duration { get; set; }
      public string start { get; set; }
      public string end { get; set; }
      public bool hasChild { get; set; }
      public string projectId { get; set; }
      public int sortId { get; set; }
      public string depends { get; set; }
      public string parentId { get; set; }
      public bool isMine { get; set; }
      public double hours { get; set; }
      public double workhours { get; set; }
      public double progress { get; set; }
      public string description { get; set; }
      public bool isPreview { get; set; }
      public double maxHours { get; set; }
      public string executorId { get; set; }
      public string reviewerId { get; set; }
      public string createrId { get; set; }
      //运维任务逻辑添加
      public string taskType { get; set; }
      public int serviceCount { get; set; }

      public bool IsParent { get; set; }
      public string subType { get; set; }
      public double subTypeValue { get; set; }
   }

}
