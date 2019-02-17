using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Helper
{

   public static partial class Errors
   {

      public class Indicaiton
      {
         public const string NOT_ALLOWED_NAME_NULL = "指标名称不能为空";
         public const string NOT_ALLOWED_ID_NULL = "指标ID不能为空";
         public const string IS_IN_USE = "指标已经被使用，无法修改！";
         public const string IS_NULL = "考核指标为空";
         public const string EDIT_FAIL = "考核指标编辑失败";
      }


      public class EvalTable
      {
         public const string NOT_ALLOWED_NAME_NULL = "考核表名称不能为空";
         public const string NOT_ALLOWED_ID_NULL = "考核表ID不能为空";
         public const string NOT_ALLOWED_SCORE_LESS_THAN_ZERO = "总分设置不能小于等于0";
         public const string EVALINDICATION_SUMSCORE_SHOULD_SAME_WITH_TABLE_SCORE = "指标总分之和不等于考核表总分！";
         public const string IS_IN_USE = "考核表已经被使用，无法修改！";
         public const string IS_NULL = "考核表为空";
         public const string WITHOUT_ANY_TABLES = "没有考核表";
         public const string SCORE_MUST_GREATER_THAN_ZERO = "总分必须输入数字";
      }

      public class EvalTableGroup
      {
         public const string NOT_EXIST_ANY_TABLES = "考核表组内不存在任何考核表";
         public const string SUM_OF_TALBES_PROPERTION_MUST_BE_100 = "考核表组占比之和必须是100%";
      }

      public class EvalGroup
      {
         public const string BIND_MEMBER_FAIL = "绑定小组成员失败";
         public const string BIND_ACCESSOR_FAIL = "绑定考核人失败";
         public const string BIND_TARGET_FAIL = "绑定被考核人失败";
         public const string ACCESSOR_ISEXISTS = "考核者已经存在";
         public const string NOT_ALLOWED_ACCESSOR_NULL = "考核者不能为空";
         public const string NOT_ALLOWED_NAME_NULL = "考核表名称不能为空";
      }


      public class Eval
      {
         public const string NOT_IN_PERIOD = "当前不再考核期!";
         public const string NOT_FOUND_TABLE = "当前考核对象没有考核表";
         public const string NOT_ANY_PERIOD = "没有任何考核周期，请设置当前考核周期!";
      }

      public class EvalTargetTablePropertion
      {
         public const string NOT_ALLOWED_WEIGHT_LESS_THAN_ZERO = "权重设置不能为0";
         public const string WEIGHT_MUST_GREATER_THAN_ZERO = "权重必须输入数字";
      }

   }

   public static partial class Success
   {

      public class Indicaiton
      {
         public const string EDIT_SUCCESS = "指标编辑成功！";
      }


      public class EvalGroup
      {
         public const string EDIT_SUCCESS = "考核组编辑成功";
         public const string BIND_MEMBER_SUCCESS = "成功绑定组成员";
         public const string REMOVE_TARGET_TABLE_SUCCESS = "删除成功";
         public const string ACCESSOR_EDIT_SUCCESS = "编辑成功";
         public const string TARGET_EDIT_SUCCESS = "编辑成功";
         public const string BIND_TARGET_SUCCESS = "成功绑定考核对象";
      }

      public class EvalResult
      {
         public const string EDIT_SUCCESS = "操作成功";
         public const string Adjust_SUCCESS = "调整分成功";
      }

   }

   public static partial class Confirm
   {

   }

}
