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


		public static Guid SelectAll = Guid.Parse("F96E81D7-5B44-A26C-BE35-45FCBA6BE8DE");


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

	}

}
