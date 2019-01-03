using Business.Helper;
using System;

namespace Business
{

   public partial class ProjectMileStone
   {

      public string StoneName { get; set; }
      public Guid StoneType { get; set; }
      public bool IsDefaultType => StoneType == MilestoneKeys.DefaultType;

   }

}