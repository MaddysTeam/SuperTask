using System;

namespace Business
{

   public class ResourceKeys
   {

      public static Guid TempBossId = Guid.Parse("D1E6E02A-40FF-4F5A-80C3-24710996B9AE"); //TODO:懒得关联，先写死 易佳

      public static Guid TempBossId2 = Guid.Parse("47682BDF-9987-4281-9F3F-BDBA4CCA8F2C"); //TODO:懒得关联，先写死 季炜

      public static Guid DefaultSearchType => Guid.Parse("3A7B1F54-B875-3AF4-FA6E-A080E8E1E7EF");


      public static Guid EditableStatus => Guid.Parse("F9F90F41-EC48-CFE4-3824-BC61DBA5F63C");

      public static Guid ReadonlyStatus => Guid.Parse("1BF223E3-ED45-4F93-2388-82FB6300204F");

      public static Guid DeletedStatus => Guid.Parse("CD4CAFB3-CC42-7EC2-935D-810F79BB8BE8");


      public static Guid PMType => Guid.Parse("D832C25C-B4A2-40B6-8D25-28B1A54D2855");

      public static Guid HeaderType => Guid.Parse("40952B32-0521-4072-9AC4-14C69A7A482F");

      public static Guid TechManager => Guid.Parse("E4CE70F5-E2FB-4E26-9FE2-5712DC6C0F19");

      public static Guid OtherType => Guid.Parse("4FDB1AEC-0B46-403E-B434-D471E508F18F");

   }

}