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
		public static Guid LevelGuid = Guid.Parse("5ef78a98-8cfd-4d32-ae6d-bbf8c2a0149b");


		/// <summary>
		/// 需求类型
		/// </summary>
		public static Guid TypeGuid = Guid.Parse("de32ab4b-5256-4e37-8a5a-27b03cde3d6c");


		/// <summary>
		/// 需求状态
		/// </summary>
		public static Guid StatusGuid = Guid.Parse("d420df47-9a81-484f-a514-66f1c6e150e9");
		public static Guid readyToReview = Guid.Parse("ba73ce64-5cf7-4192-bc78-9a4c206c84f1");
		//public static Guid readyToResolve = Guid.Parse("75548d58-c8a4-49ba-8b34-bf4b8d01d806");
		//public static Guid hasResolve = Guid.Parse("abc0668e-15e7-4662-b478-3bd39fb28fbe");
		//public static Guid hasClose = Guid.Parse("584eec94-dc05-4aa7-9365-02a43e93b2e8");

		/// <summary>
		/// 需求来源
		/// </summary>
		public static Guid SourceGuid = Guid.Parse("53be8f58-8bbe-4f8a-a367-daa48de728cc");



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
