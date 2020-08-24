using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Helper
{

   public class RoleKeys
   {

      public const string ADMIN = "系统管理员";
      public const string LEADER = "部门领导";
      public const string HEADER = "项目负责人";
      public const string PROGRAME_MANAGER = "项目经理";
      public const string TECH_LEADER = "技术主管";
      public const string DEVELOPER = "开发人员";
      public const string TECH_SUPPORTER = "技术支持";
      public const string EVAL_MANAGER = "考核管理员";

      //System role ids

      public const string EvalManagerId = "42E3876A-CE07-4704-89F6-B50DE951BB92";
      public const string WorkMateRoleId = "66374270-2B1D-49DF-87FB-0FD5912A38E5";
      public const string ProjectManagerId = "6BCDF064-EFDA-4C5B-B3BE-B822C2CBB748";
	  public const string BossRoleId = "10266E63-2FB2-4663-98A2-3B9A63FFF3EC";

      public static int SystemType =>(int)RoleType.System;

      public static int ProjectType =>(int)RoleType.Porject;

      public static Dictionary<int, string> Types = new Dictionary<int, string>
      {
         { (int)RoleType.System , "System"  },
         { (int)RoleType.Porject , "Project"  }
      };

   }


   public enum RoleType
   {
      System=1,
      Porject=2
   }

}
