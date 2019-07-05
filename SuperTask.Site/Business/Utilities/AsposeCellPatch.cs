using System;
using System.Reflection;

namespace Business.Utilities
{

	public class AsposeCellPatch
	{

		/// <summary>
		/// Aspose.Cells 是一个 Excel 文件的读写库
		/// Aspose 还有一大堆其他的库（Work，PPT，PDF，Mail，...），破解应该是差不多的，不过我现在只用到这个啦 ~
		/// Aspose 官网：http://www.aspose.com/
		/// Aspose.Cells 官网：http://www.aspose.com/.net/excel-component.aspx
		/// 注意：以下代码只保证适用于 Aspose.Cells, 8.6.3 - 8.7.1 其他版本的文件我没看！
		/// 只需在使用之前运行一次这段 Hot Patch 即可
		/// </summary>
		internal static void InitializeAsposeCells()
		{
			const BindingFlags BINDING_FLAGS_ALL = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

			const string CLASS_LICENSER = "\u0092\u0092\u0008.\u001C";
			const string CLASS_LICENSERHELPER = "\u0011\u0001\u0006.\u001A";
			const string ENUM_ISTRIAL = "\u0092\u0092\u0008.\u001B";

			const string FIELD_LICENSER_CREATED_LICENSE = "\u0001";     // static
			const string FIELD_LICENSER_EXPIRY_DATE = "\u0002";         // instance
			const string FIELD_LICENSER_ISTRIAL = "\u0001";             // instance

			const string FIELD_LICENSERHELPER_INT128 = "\u0001";        // static
			const string FIELD_LICENSERHELPER_BOOLFALSE = "\u0001";     // static

			const int CONST_LICENSER_ISTRIAL = 1;
			const int CONST_LICENSERHELPER_INT128 = 128;
			const bool CONST_LICENSERHELPER_BOOLFALSE = false;

			//- Field setter for convinient
			Action<FieldInfo, Type, string, object, object> setValue =
				delegate (FieldInfo field, Type chkType, string chkName, object obj, object value)
				{
					if ((field.FieldType == chkType) && (field.Name == chkName))
					{
						field.SetValue(obj, value);
					}
				};


			//- Get types
			Assembly assembly = Assembly.GetAssembly(typeof(Aspose.Cells.License));
			Type typeLic = null, typeIsTrial = null, typeHelper = null;
			foreach (Type type in assembly.GetTypes())
			{
				if ((typeLic == null) && (type.FullName == CLASS_LICENSER))
				{
					typeLic = type;
				}
				else if ((typeIsTrial == null) && (type.FullName == ENUM_ISTRIAL))
				{
					typeIsTrial = type;
				}
				else if ((typeHelper == null) && (type.FullName == CLASS_LICENSERHELPER))
				{
					typeHelper = type;
				}
			}
			if (typeLic == null || typeIsTrial == null || typeHelper == null)
			{
				throw new Exception();
			}

			//- In class_Licenser
			object license = Activator.CreateInstance(typeLic);
			foreach (FieldInfo field in typeLic.GetFields(BINDING_FLAGS_ALL))
			{
				setValue(field, typeLic, FIELD_LICENSER_CREATED_LICENSE, null, license);
				setValue(field, typeof(DateTime), FIELD_LICENSER_EXPIRY_DATE, license, DateTime.MaxValue);
				setValue(field, typeIsTrial, FIELD_LICENSER_ISTRIAL, license, CONST_LICENSER_ISTRIAL);
			}

			//- In class_LicenserHelper
			foreach (FieldInfo field in typeHelper.GetFields(BINDING_FLAGS_ALL))
			{
				setValue(field, typeof(int), FIELD_LICENSERHELPER_INT128, null, CONST_LICENSERHELPER_INT128);
				setValue(field, typeof(bool), FIELD_LICENSERHELPER_BOOLFALSE, null, CONST_LICENSERHELPER_BOOLFALSE);
			}
		}

	}

}