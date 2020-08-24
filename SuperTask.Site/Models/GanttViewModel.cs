using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace TheSite.Models
{

   public class GanttViewModel
   {

      public int id { get; set; }

      public string name { get; set; }

      public GanttViewModel(int id, string name, DateTime start, DateTime end,string color)
      {
         this.id = id;
         this.name = name;
         series.Add(new GanttItemViewModel(name,start,end, color));
      }

      public List<GanttItemViewModel> series = new List<GanttItemViewModel>();
   }

   public class GanttItemViewModel
   {
      public string name { get; set; }
      public string start { get; set; }
      public string end { get; set; }
	  public string color { get; set; } = "#e0e0e0";


	  public GanttItemViewModel(string name, DateTime start, DateTime end,string color)
      {
         this.name = name;
         this.start = start.ToString("yyyy-MM-dd");
         this.end = end.ToString("yyyy-MM-dd");
		 this.color = color;
      }
   }


   public static class DateTimeExtension
   {
      public static string ToJsDate(this DateTime date)
      {
         string fmtDate = "ddd MMM d HH:mm:ss 'UTC'zz'00' yyyy";
         CultureInfo ciDate = CultureInfo.CreateSpecificCulture("en-US");

         return date.ToString(fmtDate, ciDate);
      }

   }

}
