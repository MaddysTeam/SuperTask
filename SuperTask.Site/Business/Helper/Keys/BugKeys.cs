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

	}

}
