using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Helper
{

   public static class JournalKeys
   {

      public static Guid SearchToday => Guid.Parse("0AC3F3D4-D999-4249-923D-DF2C0DEC2072"); //TODO Add in dic

      public static Guid SearchDateRange => Guid.Parse("6FB81BDA-66B0-B7B6-5C3A-6726F5F670AC");//TODO Add in dic


      public static Guid SaveStatus => Guid.Parse("218836E3-17B4-EE90-CC1A-E5866AF6DF3B");//TODO Add in dic

      public static Guid RecordedStatus => Guid.Parse("9A22002B-BBA5-1DB5-C3B9-282DC9BE45A9");//TODO Add in dic

      public static Guid UnRecordedStatus => Guid.Parse("9444FA64-8D8B-9CCC-1A0D-A1CF90B4F4EF");//TODO Add in dic

      public static Guid ForbiddenStatus => Guid.Parse("4163ED9B-9BEF-0415-7743-8386917389E6");//TODO Add in dic


      public static Guid StartDateSelect => Guid.Parse("8DB4029D-9393-846A-4B0E-B27D3CFFDF92");//TODO Add in dic

      public static Guid EndDateSelect => Guid.Parse("CCA09755-E0DD-3BD0-572A-E9DC28FA3291");//TODO Add in dic


      public static Guid AutoRecordType => Guid.Parse("6A8EC199-1840-32C4-EB3A-A7069A041AA4");//TODO Add in dic

      public static Guid ManuRecordType => Guid.Parse("B6275D45-45E9-4E19-7700-FF4981D3B644");//TODO Add in dic

   }


   //public enum JournalSearchType
   //{
   //   Today=1,
   //   DateRange=2
   //}

   //public enum DateSelectType
   //{
   //   startDate=1,
   //   endDate=2,
   //   week=3,
   //   month=4
   //}


   //public enum JournalStatus
   //{
   //   Save=1,
   //   Recorded=2,
   //   UnRecord=3,
   //   Forbidden=0
   //}


   //public enum JournalReocrdType
   //{
   //	Manual=1,
   //	Auto=2
   //}


   public enum JournalQuality
   {
      Good = 1,
      Normal = 2,
      Bad = 3
   }

}
