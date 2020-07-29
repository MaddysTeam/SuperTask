﻿
using System;

namespace business.helper
{

   public class RTPBRelationKeys
   {
      public static Guid TaskWithPublish = System.Guid.Parse("532a7d98-00ec-40f6-97a1-8cf5b149511c");
      public static Guid TaskWithRequire = System.Guid.Parse("bebbaed2-0276-4ec5-a4dd-d3f598a8c687");
      public static Guid TaskWithBug= System.Guid.Parse("c38ca3ef-c13f-401e-8b77-df4ec576695d");
	  public static Guid BugWithRequire = Guid.Parse("34b3af7e-1a4e-4681-803b-a279018f021b");
		public static Guid PublishWithRequire = Guid.Parse("4af2d57c-4154-4ed7-80f5-276c7c8069db");
   }

}
