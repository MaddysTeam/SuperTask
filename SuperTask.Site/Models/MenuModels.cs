using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheSite.Models
{

   public class MenuItem
   {

      public Guid Id { get; set; }

      public Guid ParentId { get; set; }

      public string Title { get; set; }

      public string Url { get; set; }

      public string Icon { get; set; }

      public bool IsChecked { get; set; }

      public bool IsVisible { get; set; }

      public string FullTitle { get; set; }

   }

}