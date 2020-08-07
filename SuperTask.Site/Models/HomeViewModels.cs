using Business;
using System.Collections.Generic;

namespace TheSite.Models
{

	public class EmployeeHomeViewModel
	{
		List<WorkTask> Tasks { get; set; }
		List<Bug> Bugs { get; set; }
		List<Require> Requires { get; set; }
	}

}
