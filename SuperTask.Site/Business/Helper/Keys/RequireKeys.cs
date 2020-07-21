using Business.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Helper
{

	public static class RequireKeys
	{

		public static Guid SelectAll => AppKeys.SelectAll;

		/// <summary>
		/// 需求级别,需要Dictionary表里创建，需要用于下拉框的数据
		/// </summary>
		public static Guid LevelGuid = Guid.Parse("");


		/// <summary>
		/// 需求类型
		/// </summary>
		public static Guid TypeGuid = Guid.Parse("");


		/// <summary>
		/// 需求状态
		/// </summary>
		public static Guid StatusGuid = Guid.Parse("");
		//public static Guid readyToConfirm = Guid.Parse("4f82664e-7642-4abe-a106-43c15f9bd6ba");
		//public static Guid readyToResolve = Guid.Parse("75548d58-c8a4-49ba-8b34-bf4b8d01d806");
		//public static Guid hasResolve = Guid.Parse("abc0668e-15e7-4662-b478-3bd39fb28fbe");
		//public static Guid hasClose = Guid.Parse("584eec94-dc05-4aa7-9365-02a43e93b2e8");

		/// <summary>
		/// 需求来源
		/// </summary>
		public static Guid SourceGuid = Guid.Parse("");



		public static string GetTypeKeyByValue(Guid val) => DictionaryHelper.GetDicById(TypeGuid, val).Title;

		public static string GetStatusKeyByValue(Guid val) => DictionaryHelper.GetDicById(StatusGuid, val).Title;

		public static string GetLevelByValue(Guid val) => DictionaryHelper.GetDicById(LevelGuid, val).Title;


		//public static Dictionary<Guid, string> OperationResultDic = new Dictionary<Guid, string>
		//{
		//  { BugKeys.ConfrimYes,"确定了这个bug" },
		//  { BugKeys.ConfirmNo, "否定了这个bug" },
		//  { BugKeys.Resolved, "已经处理了这个bug" },
		//  { BugKeys.Resolving, "正在处理这个bug" }
		//};

	}

}
