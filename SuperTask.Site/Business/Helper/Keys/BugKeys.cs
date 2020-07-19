using Business.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Helper
{

	public static class BugKeys
	{

		public static Guid SelectAll => AppKeys.SelectAll;

		/// <summary>
		/// Bug级别,需要Dictionary表里创建，需要用于下拉框的数据
		/// </summary>
		public static Guid LevelGuid = Guid.Parse("c2ec33b5-2207-4af9-b610-4d2c61f02859");


		/// <summary>
		/// bug类型
		/// </summary>
		public static Guid TypeGuid = Guid.Parse("ce381d2c-7785-48f9-829f-eacb392679e0");


		/// <summary>
		/// bug状态
		/// </summary>
		public static Guid StatusGuid = Guid.Parse("584eec94-dc05-4aa7-9365-02a43e93b2e8");
      public static Guid readyToConfirm = Guid.Parse("4f82664e-7642-4abe-a106-43c15f9bd6ba");
      public static Guid readyToResolve = Guid.Parse("75548d58-c8a4-49ba-8b34-bf4b8d01d806");
      public static Guid hasResolve = Guid.Parse("abc0668e-15e7-4662-b478-3bd39fb28fbe");
      public static Guid hasClose = Guid.Parse("584eec94-dc05-4aa7-9365-02a43e93b2e8");

      /// <summary>
      /// 浏览器
      /// </summary>
      public static Guid BrowserGuid = Guid.Parse("ef30641f-fd3e-4311-a501-a63468803f9f");


		/// <summary>
		/// 操作系统
		/// </summary>
		public static Guid SystemGuid = Guid.Parse("ac04cc26-2550-48ea-8857-a459ffe4a3e0");


      /// <summary>
		/// bug确认
		/// </summary>
      public static Guid BugConfirm = Guid.Parse("074a8898-b8d4-4482-b6be-0347513e9a23");

      public static Guid ConfrimYes = Guid.Parse("e9db6b14-1985-4a1d-926e-8c35ca821deb");

      public static Guid ConfirmNo = Guid.Parse("66ad02c8-5933-4480-a0e6-3378c1cc1f38");


      /// <summary>
      /// bug解决
      /// </summary>
      public static Guid BugResolve = Guid.Parse("30feb559-70a8-4c02-bc61-d319b349bd94");

      public static Guid Resolved = Guid.Parse("bd923fc0-0fb8-42f2-971a-f6b69dc2a330");

      public static Guid Resolving = Guid.Parse("66837bba-c0e2-4516-8dd1-04ca778c5737");


      public static string GetTypeKeyByValue(Guid val) => DictionaryHelper.GetDicById(TypeGuid, val).Title;

      public static string GetStatusKeyByValue(Guid val) => DictionaryHelper.GetDicById(StatusGuid, val).Title;

      public static string GetLevelByValue(Guid val) => DictionaryHelper.GetDicById(LevelGuid, val).Title;

      public static string GetSystemByValue(Guid val) => DictionaryHelper.GetDicById(SystemGuid, val).Title;

      public static string GetBrowserByValue(Guid val) => DictionaryHelper.GetDicById(BrowserGuid, val).Title;

   }

}
