using Business.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Helper
{

	public static class TaskKeys
	{
		/// <summary>
		/// 默认任务名
		/// </summary>
		public static string DefaultTaskName = "新建任务";


		/// <summary>
		/// 临时的节点任务
		/// </summary>
		public static string TempNodeTaskName = "节点任务";

		/// <summary>
		/// 任务类型
		/// </summary>
		public static Guid TypeGuid = Guid.Parse("DD3ADC7F-A56C-3C58-9CAF-C2E3D6C9DC6B");

		/// <summary>
		/// 任务状态
		/// </summary>
		public static Guid StatusGuid = Guid.Parse("dd3adc7f-a55c-3c58-9caf-d3a2b7a9dc7b");

		/// <summary>
		/// 任务文档类型,需要Dictionary表里创建，需要用于下拉框的数据
		/// </summary>
		public static Guid FileTypeGuid = Guid.Parse("DE3ADC7F-A55C-3C58-9CAF-D3A2B7A9DC1B");

		/// <summary>
		/// 任务范围类型,需要Dictionary表里创建，需要用于下拉框的数据
		/// </summary>
		public static Guid RangeTypeGuid = Guid.Parse("94DECD1D-8C60-4C85-8F00-E635C1D5540A");

		/// <summary>
		/// 任务级别,需要Dictionary表里创建，需要用于下拉框的数据
		/// </summary>
		public static Guid LevelGuid = Guid.Parse("c1587ca0-0aba-4cd4-ae21-46e81e3caa79");


		public static Guid SelectAll => AppKeys.SelectAll;


		public static string GetTypeKeyByValue(Guid val) => DictionaryHelper.GetDicById(TypeGuid, val).Title;

		public static string GetStatusKeyByValue(Guid val) => DictionaryHelper.GetDicById(StatusGuid, val).Title;

		public static string GetFileTypeKeyByValue(Guid val) => DictionaryHelper.GetDicById(FileTypeGuid, val).Title;

		public static string GetV2LevelByValue(Guid val) => DictionaryHelper.GetDicById(LevelGuid, val).Title;


		public static Guid DeleteStatus => Guid.Parse("CC3CDC7F-A46D-3C58-9CAF-D3A2B7C9AB8D");

		public static Guid PlanStatus => Guid.Parse("CC3CDC7F-A46D-3C58-9CAF-D3A2B7C9AB5D");

		public static Guid CompleteStatus => Guid.Parse("CC3CDC7F-A46D-3C58-9CAF-D3A2B7C9AB7D");

		public static Guid ProcessStatus => Guid.Parse("CC3CDC7F-A46D-3C58-9CAF-D3A2B7C9AB6D");

		public static Guid TaskTempEditStatus => Guid.Parse("CC3CDC7F-A46D-3C58-9CAF-D3A2B7C9AB9D");

		public static Guid ReviewStatus => Guid.Parse("CC3CDC7F-A46D-3C58-9CAF-D3A2B7C9AC1D");

		public static Guid CloseStatus => Guid.Parse("CC3CDC7F-A46D-3C58-9CAF-D3A2B7C9AB9D");


		public static Guid ProjectTaskType => Guid.Parse("DD3ABC1D-A33C-3C68-8ACF-C2E5D6C9DC8B");

		public static Guid PlanTaskTaskType = Guid.Parse("DD4BBC8D-A53C-3D68-8ACF-C2E5D7C9DC8B");

		public static Guid TempTaskType => Guid.Parse("DD3ABC1D-A33C-3C68-8ACF-C2E5D6C9DC7B");

		public static Guid MaintainedTaskType => Guid.Parse("DD3ABC1D-A33C-3C68-8ACF-C2E5D6C9DC9C");

		public static Guid DocumentTaskType = Guid.Parse("DE3ABC1D-A33C-3C68-8ACF-C2E5D6C9DC9C");

		public static Guid ManageTaskType = Guid.Parse("DF3ABC1D-A33C-3C68-8ACF-C2E5D6C9DC9C");

		public static Guid DesignTaskType = Guid.Parse("2e01bb7f-19ce-4e07-96a2-76575d5a1d2b");

		public static Guid DevelopTaskType = Guid.Parse("68cd7291-bb5a-4024-848a-66eb79c64dc8");

		public static Guid TestTaskType = Guid.Parse("28c7ec86-a1e7-41d1-b9af-38fea64017d7");

		public static Guid DeployTaskType = Guid.Parse("52e91879-ed8b-4fbc-a7a8-9be809cda7fe");

		public static Guid OfficeTaskType = Guid.Parse("03aadd19-1697-4ad1-a77c-cc1ed95a28a9");

		public static Guid DefaultType => Guid.Parse("77AAD589-E95B-839E-C1F5-55027AE3BC14");

		//节点任务类型
		public static Guid NodeTaskType => Guid.Parse("77AAD559-E95B-849E-C1F5-56027AE3BC18");


		public static int TaskNameDisplayLength = 10;

		public static double DefaultEstimateHours = 0;


		public static Guid RequreFileType => Guid.Parse("0E559F87-3B5A-5C23-B724-EE79E7AADFFC"); //TODO Add in dic

		public static Guid DesignFileType => Guid.Parse("982973B1-33DC-F846-C6F4-504EE65DF887"); //TODO Add in dic

		public static Guid CheckAndAcceptFileType => Guid.Parse("4BA83CAC-853A-676B-2E04-896D722098AF"); //TODO Add in dic

		public static Guid DeliverFileType => Guid.Parse("729A87BE-3B00-16FD-FC5E-05046FB630BB"); //TODO Add in dic

		public static Guid DefaultFileType => Guid.Parse("729A87CE-3B01-16FD-FC5E-05046FB630BB"); //TODO Add in dic


		public static Guid SendToMeRangeType => Guid.Parse("94DECD2D-8C60-4C85-8F00-E635C1D5640C");

		public static Guid SendByMeRangeType => Guid.Parse("94DECD3D-8C60-4C85-8F00-E635C1D5640D");

		public static Guid ReviewByMeRangeType => Guid.Parse("94DECD3D-8C60-4C85-8F00-E635C1D5740E");


		public static Guid SearchByDetaultType => Guid.Parse("FEB2DF18-4A57-0BE6-B44B-1C16EE6528BC"); //TODO Add in dic

		public static Guid SearchByProject => Guid.Parse("3836EA7E-906C-5388-7D6B-274FA203C569"); //TODO Add in dic

		public static Guid SearchByPersonal => Guid.Parse("C0797B23-521C-E8B5-041A-C039ABB17C20"); //TODO Add in dic

		// 2020-07-27 by huachao

		public static Guid StartActionGuid => Guid.Parse("407dc6e0-8a81-4ab9-b405-3b5e98da749a");

		public static Guid CompleteActionGuid => Guid.Parse("ccc99cd2-9495-4d82-9f89-da50d71123c4");

		public static Guid CloseActionGuid => Guid.Parse("cfc4d21e-8997-4a83-87a0-25c8a9bcc9a5");

		// 2020-08-05 by huachao

		/// <summary>
		/// 任务类型V2
		/// </summary>
		public static Guid TypeV2Guid = Guid.Parse("b962ea4f-bd37-4f4a-8674-9e29c08467b5");

		public static Guid MeetingType = Guid.Parse("d0456dc4-427d-4f54-9bea-5cb6958194c6");
		public static Guid DocumentType = Guid.Parse("31cc299d-b499-4f3c-aeaf-951a8afea61b");
		public static Guid DesignType = Guid.Parse("6cf723ba-461c-409b-8693-a114fe561111");
		public static Guid Frontend = Guid.Parse("6984f239-85e4-4bad-afda-42650b767920");
		public static Guid ServerSide = Guid.Parse("4a7c77fd-ebc6-46b3-8ce4-c10050ecbd3c");
		public static Guid Test = Guid.Parse("64088f74-f437-442b-8800-3f730ab408b0");
		public static Guid Maintain = Guid.Parse("6da4b291-bbdf-4785-b704-f2c45709b512");
		public static Guid Business = Guid.Parse("4d598e03-b903-47e6-9f48-9ebbea9fecb2");
		public static Guid Inter = Guid.Parse("45acdc5f-9aae-4b1e-aa69-1236a9e08717");
		public static Guid Management = Guid.Parse("c1aa7c1c-10d9-4283-ad38-30bf8dd89e78");
		public static Guid Others = Guid.Parse("c3ab2686-a424-41d8-968d-8d1c21b515c9");

		public static string GetTypeV2KeyByValue(Guid val) => DictionaryHelper.GetDicById(TypeV2Guid, val).Title;

		public static Dictionary<Guid, string> OperationResultDic = new Dictionary<Guid, string>
	   {
		 { StartActionGuid,"启动了任务" },
		 { CompleteActionGuid,"完成了任务及其子任务" },
		 { CloseActionGuid, "关闭了任务" },
         //{ Close, "关闭发布" },
         //{RelativeGuid,"变更了发布关联" }
       };


	}

}
