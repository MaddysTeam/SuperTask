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
		public static Guid LevelGuid = Guid.Parse("");


		/// <summary>
		/// bug类型
		/// </summary>
		public static Guid TypeGuid = Guid.Parse("");


		/// <summary>
		/// bug状态
		/// </summary>
		public static Guid StatusGuid = Guid.Parse("");

	}

}
