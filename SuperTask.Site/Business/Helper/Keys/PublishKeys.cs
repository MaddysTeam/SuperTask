using Business.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Helper
{

	public static class PublishKeys
	{

		public static Guid SelectAll => AppKeys.SelectAll;


		/// <summary>
		/// 发布类型
		/// </summary>
		public static Guid TypeGuid = Guid.Parse("");

		/// <summary>
		/// 发布状态
		/// </summary>
		public static Guid StatusGuid = Guid.Parse("");
		//public static Guid readyToReview = Guid.Parse("ba73ce64-5cf7-4192-bc78-9a4c206c84f1");
		public static Guid Close = Guid.Parse("93d2126a-5aae-4fba-9a89-8c11125ed3b5");

		/// <summary>
		/// 发布处理
		/// </summary>
		public static Guid HandleGuid = Guid.Parse("13715897-a68a-4f32-a8cc-e0cd60fab689");


		public static string GetTypeKeyByValue(Guid val) => DictionaryHelper.GetDicById(TypeGuid, val).Title;

		public static string GetStatusKeyByValue(Guid val) => DictionaryHelper.GetDicById(StatusGuid, val).Title;

		public static string GetReviewResultByValue(Guid val) => DictionaryHelper.GetDicByValue(StatusGuid, val).Title;


		public static Dictionary<Guid, string> OperationResultDic = new Dictionary<Guid, string>
		{
		 //{ RequireKeys.ReviewWaiting,"待评审" },
		 // { RequireKeys.ReviewSuccess,"评审通过" },
		 // { RequireKeys.ReviewFail, "评审不通过" },
		 // { RequireKeys.Close, "关闭了需求" },
		 // { RequireKeys.ReviewHandleGuid,"处理了需求，更改了需求状态"}
		};

	}

}
