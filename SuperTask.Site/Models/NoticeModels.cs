using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheSite.Models
{

   public class NoticeModel
   {
      public int DisplayCount { get; set; }

      public string Message { get; set; }
   }


   public enum NoticeType
   {
      Project=1,
      Task=2,
      Journal=3,
      Reivew=4
   }

}
