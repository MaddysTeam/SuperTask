using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Business.Helper
{

   public static class DictionaryKeys
   {
      public const string RootId = "2A2BBEE3-9883-4185-A64C-4430AA20E0CB";

      public static Guid TaskTypeId = Guid.Parse("DD3ADC7F-A56C-3C58-9CAF-C2E3D6C9DC6B");
      public static Guid DocumentTaskTypeId = Guid.Parse("DE3ABC1D-A33C-3C68-8ACF-C2E5D6C9DC9C");
      public static Guid MaintanceTaskTypeId = Guid.Parse("DD3ABC1D-A33C-3C68-8ACF-C2E5D6C9DC9C");
      public static Guid ManageTaskTypeId = Guid.Parse("DF3ABC1D-A33C-3C68-8ACF-C2E5D6C9DC9C");
   }

}