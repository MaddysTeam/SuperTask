﻿using System;

namespace Business.Helper
{

	public static class ShareFolderKeys
	{

		public static Guid RootProjectFolderId = Guid.Parse("64206816-B1C7-403E-B4CF-D797E32FD322");

		/// <summary>
		/// 项目文件夹类型
		/// </summary>
		public static Guid ProjectType = Guid.Parse("0c915c85-ea2a-4980-b903-7c61d994f4b9");

		public static Guid RequiredType = Guid.Parse("68415e39-c582-484d-89a2-4d7a07045304");
		public static Guid DevelopType = Guid.Parse("8d7443f7-ceba-41ce-9602-b575ea9505eb");
		public static Guid TestType = Guid.Parse("2759dd3b-ab77-4669-b712-c357e00dbe96");
		public static Guid MaintainType = Guid.Parse("db17c8bf-1bdf-47ef-8c36-ed9fd9521fe5");
		public static Guid BusinessType = Guid.Parse("9f04ceff-af6c-4977-bd92-9d6e39b8f2a9");

		/// <summary>
		/// 项目文件夹权限
		/// </summary>
		public static Guid Permission = Guid.Parse("0d39ef45-6621-485c-aefd-f1ecb945cf4d");

		public static Guid SearchPermission = Guid.Parse("4a66850a-7026-42ef-b8ca-44bfb2d752ba");
		public static Guid PreviewPermission = Guid.Parse("3901638b-93ae-4a32-becc-4bc369766e9f");
		public static Guid DownloadPermission = Guid.Parse("b8f89e4e-c846-493e-976c-d8e0e0e77082");
		public static Guid EditPermission = Guid.Parse("8b249852-3683-4a71-94aa-b3def9c0c8d9");
		public static Guid UploadPermission = Guid.Parse("436e99a9-0ba0-49d0-97cc-6fb56a163d6d");
		public static Guid DeletePermission = Guid.Parse("f20369d4-c645-4afa-9dcc-3ca32c5a4e6b");
		public static Guid SetPermission = Guid.Parse("937d92b6-0255-4681-9616-0a072e36987f");

	}

}
