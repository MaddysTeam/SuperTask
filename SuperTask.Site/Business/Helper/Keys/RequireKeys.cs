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
		public static Guid Success = Guid.Parse("D7357391-F060-4B59-8720-F5947320E650");
		public static Guid Fail = Guid.Parse("F65A5645-F48F-4262-80B5-F3BD78DE5FEF");
		public static Guid Close = Guid.Parse("93d2126a-5aae-4fba-9a89-8c11125ed3b5");
		public static Guid InTest = Guid.Parse("A1C5A8EF-789C-4F43-89F7-2A83488837C3");
		public static Guid InDev = Guid.Parse("5ABFFC7F-FF5B-4D7A-BB7B-972DB18EC70F");
		public static Guid Done = Guid.Parse("F5EE91BB-54C0-43B8-8B5D-F2BF25705A3F");

		/// <summary>
		/// 需求来源
		/// </summary>
		public static Guid SourceGuid = Guid.Parse("53be8f58-8bbe-4f8a-a367-daa48de728cc");
		//public static Guid CorpGuid = Guid.Parse("");
		//public static Guid DepartGuid = Guid.Parse("");
		//public static Guid TeamGuid = Guid.Parse("");
		//public static Guid ThiredPartyGuid = Guid.Parse("");

		/// <summary>
		/// 评审结果
		/// </summary>
		public static Guid ReviewResultGuid = Guid.Parse("3b3d0ea9-6f74-4ece-b54e-9ea5243f7e6f");
		public static Guid ReviewWaiting = Guid.Parse("d110708b-0780-41a2-83be-f460216e6f7c");
		public static Guid ReviewSuccess = Guid.Parse("2f7cff64-9e82-4890-a660-dadcd01b65c2");
		public static Guid ReviewFail = Guid.Parse("78909727-4d47-4799-88a9-ac140968ec49");

		/// <summary>
		/// 需求处理
		/// </summary>
		public static Guid HandleGuid = Guid.Parse("13715337-a68a-4f32-a8cc-e0cd60fab789");
		public static Guid HandleDev = Guid.Parse("eb4e5efa-4c51-4e24-aa4e-96d882af5039");
		public static Guid HandleTest = Guid.Parse("65903a6f-7fac-4d32-b5e6-2ab2a9376056");
		public static Guid HandleDone = Guid.Parse("e7b17554-66cc-4823-bd94-ab4764ab0817");
		public static Guid HandleClose = Guid.Parse("4d5cb32a-d30c-47fc-a254-e5bee1ed1695");

		/// <summary>
		/// 加急
		/// </summary>
		public static Guid Hurryup = Guid.Parse("51ccd649-016c-402e-87cd-f1df8f3aa293");

		/// <summary>
		/// 新增和编辑
		/// </summary>
		public static Guid CreateGuid = Guid.Parse("0c83ac97-68ae-44fc-920f-1c7a18231a35");
		public static Guid EditGuid = Guid.Parse("0bc252ca-e4e2-4980-a69c-e28e4fe0fc59");

		public static string GetTypeKeyByValue(Guid val) => DictionaryHelper.GetDicById(TypeGuid, val).Title;

		public static string GetStatusKeyByValue(Guid val) => DictionaryHelper.GetDicById(StatusGuid, val).Title;

		public static string GetLevelByValue(Guid val) => DictionaryHelper.GetDicById(LevelGuid, val).Title;

		public static string GetReviewResultByValue(Guid val) => DictionaryHelper.GetDicByValue(ReviewResultGuid, val).Title;


		public static Dictionary<Guid, Guid> KeysMapping => new Dictionary<Guid, Guid>
	  {
		 {HandleDev,InDev }, {HandleTest,InTest }, {HandleDone,Done}, {ReviewSuccess, Success}, {ReviewFail,Fail }, { ReviewWaiting,readyToReview}
	  };

		public static Dictionary<Guid, string> OperationResultDic = new Dictionary<Guid, string>
	  {
		{ CreateGuid,"新增了需求" },
		{ EditGuid,"编辑了需求" },
		{ ReviewWaiting,"将状态改为了【待评审】" },
		{ ReviewSuccess,"评审通过" },
		{ ReviewFail, "评审不通过" },
		{ HandleClose, "关闭了需求" },
		{ InTest,"更改了需求状态，当前状态：测试中"},
		{ HandleDev,"将状态改为了【开发中】" },
		{ HandleTest,"将状态改为了【测试中】" },
		{ HandleDone,"将状态改为了【已完成】" },
		{ Hurryup,"发起了加急审批" }
	  };


	}

}
