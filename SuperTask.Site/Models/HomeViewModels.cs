using Business;
using System.Collections.Generic;

namespace TheSite.Models
{

   public class EmployeeHomeViewModel
   {
      public List<WorkTask> Tasks { get; set; } = new List<WorkTask>();
      public List<Bug> Bugs { get; set; } = new List<Bug>();
      public List<Require> Requires { get; set; } = new List<Require>();
      public List<Require> ReviewRequires { get; set; } = new List<Require>();
      public List<Project> Porjects { get; set; } = new List<Project>();
      public List<UserInfo> Users { get; set; } = new List<UserInfo>();
	}

}
