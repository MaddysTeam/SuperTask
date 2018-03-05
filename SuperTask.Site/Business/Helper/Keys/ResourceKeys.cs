using Business.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Business
{

   public class ResourceKeys
   {

      public static Guid DefaultSearchType => Guid.Parse("3A7B1F54-B875-3AF4-FA6E-A080E8E1E7EF");


      public static Guid EditableStatus => Guid.Parse("F9F90F41-EC48-CFE4-3824-BC61DBA5F63C");

      public static Guid ReadonlyStatus => Guid.Parse("1BF223E3-ED45-4F93-2388-82FB6300204F");

      public static Guid DeletedStatus => Guid.Parse("CD4CAFB3-CC42-7EC2-935D-810F79BB8BE8");


      public static Guid PMType => Guid.Parse("D832C25C-B4A2-40B6-8D25-28B1A54D2855");// (int)ResourceType.pm;

      public static Guid HeaderType => Guid.Parse("40952B32-0521-4072-9AC4-14C69A7A482F");//(int)ResourceType.header;

      public static Guid TechManager => Guid.Parse("E4CE70F5-E2FB-4E26-9FE2-5712DC6C0F19");//(int)ResourceType.header;

      public static Guid OtherType => Guid.Parse("4FDB1AEC-0B46-403E-B434-D471E508F18F");//(int)ResourceType.other;

   }

   //public enum ResourceSearchType
   //{
   //   Default = 1
   //}

   //public enum ResourceStatus
   //{
   //   DeleteStatus = 0,
   //   EditableStatus = 1,
   //   ReadonlyStatus = 2,
   //}

}