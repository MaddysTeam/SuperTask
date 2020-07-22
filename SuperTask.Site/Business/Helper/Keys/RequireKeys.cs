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

		/// <summary>
		/// 需求来源
		/// </summary>
		public static Guid SourceGuid = Guid.Parse("53be8f58-8bbe-4f8a-a367-daa48de728cc");

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
      public static Guid ReviewHandleGuid = Guid.Empty;




      public static string GetTypeKeyByValue(Guid val) => DictionaryHelper.GetDicById(TypeGuid, val).Title;

		public static string GetStatusKeyByValue(Guid val) => DictionaryHelper.GetDicById(StatusGuid, val).Title;

		public static string GetLevelByValue(Guid val) => DictionaryHelper.GetDicById(LevelGuid, val).Title;

		public static string GetReviewResultByValue(Guid val) => DictionaryHelper.GetDicByValue(ReviewResultGuid, val).Title;


		public static Dictionary<Guid, string> OperationResultDic = new Dictionary<Guid, string>
		{
		 { RequireKeys.ReviewWaiting,"待评审" },
		  { RequireKeys.ReviewSuccess,"评审通过" },
		  { RequireKeys.ReviewFail, "评审不通过" },
		  //{ RequireKeys.Resolved, "已经处理了这个bug" },
		  //{ RequireKeys.Resolving, "正在处理这个bug" }
		};

	}

}
