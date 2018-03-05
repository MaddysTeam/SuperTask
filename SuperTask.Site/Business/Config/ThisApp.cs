using System;

namespace Business.Config
{

	public static partial class ThisApp
	{

      // 稳定的数据缓存时间（分钟）
      public const int StableCacheMinutes = 20;

		// 不稳定数据缓存时间（分钟）
		public const int UnstableCacheMinutes = 2;

		//	文件上传路径 （暂时）
		public const string UploadFilePath = "/Attachments/";

		//	文件服务器路径 （暂时）
		public const string FileServerPath = "http://localhost:33965";

		//	用户信息session key
		public const string UserInfo = "UserInfo";

		// 系统开发商 ID
		public const long AppUser_Designer_Id = 1;
		public const long AppUser_Admin_Id = 2;

		// 缺省用户密码
		public const string DefaultPassword = "123456";

		// 缺省邮箱后缀
		public const string DefaultEmailSuffix = "@enroll.edu.cn";

      // 网站默认Url
      public const string DefaultUrl = "http://localhost:26542/home/index";

      // 默认GUID
		public static Guid DefaultId = Guid.Parse("00000000-0000-0000-0000-000000000000");

      // 等部门人员控件做好再删除
      public static Guid DefaultDepartmentId = Guid.Parse("AB1C7452-1F34-4A5B-A315-03BC755FF50C");

      // 内容显示长度
      public static int ContentDisplayLength = 20;//

      // 当前APP类型为项目管理系统，在AppLibrary里指定
      public static Guid ThisAPPType = Guid.Parse("dd3adc7f-a55c-3c58-9caf-d3a2b7a9dd8b");

      public static string SelectAll = "-1";

      public static DateTime StartDayPerMonth = DateTime.Now.AddMonths(-1);

      public static DateTime EndDayPerMonth = DateTime.Now;

      // 默认任务标准复杂度
      public static int DefalutTaskStandardComplexity = 3;

      // 默认任务标准复杂度范围
      public static int[] DefaultTaskComplexities = { 1, 2, 3, 4, 5 };

   }
}