using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business
{

   public static class DateTimeExtensions
   {

      public static bool IsToday(this DateTime date)
      {
         return date.Subtract(DateTime.Now).Days == 0;
      }


      public static DateTime TodayStart(this DateTime date)
      {
         return DateTime.Parse(date.ToString("yyyy-MM-dd") + "  00:00:00");
      }


      public static DateTime TodayEnd(this DateTime date)
      {
         return DateTime.Parse(date.AddDays(1).ToString("yyyy-MM-dd") + "  00:00:00");
      }


      public static DateTime MonthStart(this DateTime date)=>new  DateTime(date.Year, date.Month, 1);


      public static DateTime MonthEnd(this DateTime date) => date.AddMonths(1).AddDays(-1);


      public static DateTime GetNextMondayIfIsWeekend(this DateTime date)
      {
         var tomorrow = date.AddDays(1).DayOfWeek;
         if (tomorrow == DayOfWeek.Saturday)
            return DateTime.Now.AddDays(3);
         else if (tomorrow == DayOfWeek.Sunday)
            return DateTime.Now.AddDays(2);
         else if (tomorrow == DayOfWeek.Monday)
            return DateTime.Now.AddDays(1);

         return date;
      }


      public static int GetWorkDays(this DateTime start, DateTime end)
      {
         TimeSpan span = end - start;
         int AllDays = Convert.ToInt32(span.TotalDays) + 1;//差距的所有天数
         int totleWeek = AllDays / 7;//差别多少周
         int yuDay = AllDays % 7; //除了整个星期的天数
         int lastDay = 0;
         if (yuDay == 0) //正好整个周
         {
            lastDay = AllDays - (totleWeek * 2);
         }
         else
         {

            int weekDay = 0;
            int endWeekDay = 0;  //多余的天数有几天是周六或者周日
            switch (start.DayOfWeek)
            {
               case DayOfWeek.Monday:
                  weekDay = 1;
                  break;
               case DayOfWeek.Tuesday:
                  weekDay = 2;
                  break;
               case DayOfWeek.Wednesday:
                  weekDay = 3;
                  break;
               case DayOfWeek.Thursday:
                  weekDay = 4;
                  break;
               case DayOfWeek.Friday:
                  weekDay = 5;
                  break;
               case DayOfWeek.Saturday:
                  weekDay = 6;
                  break;
               case DayOfWeek.Sunday:
                  weekDay = 7;
                  break;
            }

            if ((weekDay == 6 && yuDay >= 2) || (weekDay == 7 && yuDay >= 1) || (weekDay == 5 && yuDay >= 3) || (weekDay == 4 && yuDay >= 4) || (weekDay == 3 && yuDay >= 5) || (weekDay == 2 && yuDay >= 6) || (weekDay == 1 && yuDay >= 7))
            {
               endWeekDay = 2;
            }
            if ((weekDay == 6 && yuDay < 1) || (weekDay == 7 && yuDay < 5) || (weekDay == 5 && yuDay < 2) || (weekDay == 4 && yuDay < 3) || (weekDay == 3 && yuDay < 4) || (weekDay == 2 && yuDay < 5) || (weekDay == 1 && yuDay < 6))
            {
               endWeekDay = 1;
            }

            lastDay = AllDays - (totleWeek * 2) - endWeekDay;
         }

         return lastDay;
      }


      public static IEnumerable<DateTime> DaysBetween(this DateTime start, DateTime end)
      {
         var current = start;
         if (current != current.Date) 
            current = current.AddDays(1).Date;
         while (current < end)
         {
            yield return current;
            current = current.AddDays(1);
         }
      }


      public static IEnumerable<DateTime> WorkDayBetween(this DateTime start, DateTime end)
      {
         return DaysBetween(start, end)
             .Where(date => IsWorkDay(date));
      }


      private static bool IsWorkDay(this DateTime date)
      {
         return date.DayOfWeek != DayOfWeek.Saturday
                         && date.DayOfWeek != DayOfWeek.Sunday;
      }


      public static int GetHoursBetweenTwoDate(this DateTime start, DateTime end)
      {
         var days = (end - start).Days;

         return days <= 0 ? 24 : (days + 1) * 24;
      }

      public static bool IsEmpty(this DateTime date)
      {
         return date == DateTime.MinValue;
      }


      public static string ConvertToString(this DateTime date)
      {
         return date.IsEmpty()? "-":date.ToString() ;
      }

   }

}
