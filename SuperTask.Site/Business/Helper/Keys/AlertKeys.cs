
namespace Business.Helper
{

   public static partial class Errors
   {

      public class Project
      {
         public const string NOT_ALLOWED_NULL = "项目不能为空";
         public const string NOT_ALLOWED_NAME_NULL = "项目名称不能为空";
         public const string NOT_ALLOWED_Owner_NULL = "项目甲方不能为空";
         public const string NOT_ALLOWED_MANAGER_NULL = "请选择项目负责人！";
         public const string NOT_ALLOWED_SEPCIAL_CHAR = "项目名称不能包含特殊字符";
         public const string NOT_ALLOWED_ID_NULL = "项目ID不能为空";
         public const string NOT_ALLOWED_Complete = "项目无法设置成完成装填，请确保所有任务已经完成";
         public const string NOT_ALLOWED_DATE_INVALIDATE = "日期非法";
         public const string NOT_ALLOWED_DATE_INVALIDATE_RANGE = "请检查日期范围";
         public const string NOT_ALLOWED_START_BEFORE_START_DATE = "该项目还未到开始时间，如要提前启动，则需修改项目开始时间";
         public const string HAS_COMPLETE = "项目已经结束";
         public const string NOT_EXIST = "项目不存在";
         public const string START_FAIL = "项目启动失败";
         public const string PLAN_FAIL = "项目计划编辑失败";
         public const string NOT_IN_PROCESS = "该项目未处于运行状态";
         public const string IN_PROCESS = "该项目处于运行状态";
         public const string EARLER_THAN_CHILD_TASK_COMPLETED = "项目结束日期不能早于任务结束日期 或者 项目开始日期不能晚于任务开始日期";
         public const string NOT_ALLOWED_OPERATOR_NULL = "当前操作者不能为空";
         public const string NOT_ALLOWED_ORIGNAL_NULL = "项目不存在";
      }

      public class Task
      {
         public const string NOT_ALLOWED_ID_NULL = "任务ID不能为空";
         public const string NOT_ALLOWED_NAME_NULL = "任务名称不能为空";
         public const string NOT_ALLOWED_SEPCIAL_CHAR = "任务名称不能包含特殊字符";
         public const string NOT_ALLOWED_DATE_INVALIDATE_RANGE = "请检查日期范围";
         public const string NOT_ALLOWED_MANAGER_NULL = "必须选择执行人";
         public const string NOT_EXIST = "任务不存在";
         public const string NOT_ALLOWED_MULITE_TASKS = "不允许有多个根节点任务";
         public const string TASKS_OUT_OF_PROJECT_RANGE = "任务时间范围不能超过项目时间范围";
         public const string ROOT_TASK_POSITION_NOT_ALLOW_CHANGE = "根任务节点位置无法改变";
         public const string TASK_HOURS_NOT_ALLOWED_ZERO = "有任务还未预估工时";
         public const string TASK_POSITION_ERROR = "任务节点位置错误";
         public const string CHILD_NOT_DONE = "还有子任务未完成";
         public const string PARENT_NOT_ALLOWED_SUBMIT = "父任务不能提交";
         public const string PARENT_NOT_ALLOWED_CHANGE_DATE = "只能修改子任务的时间范围";
         public const string TASK_IN_REVIEW = "任务处于审核状态";
         public const string NOT_ALLOWED_START_DUE_TO_PROJECT_NOT_START = "该任务所属项目还未启动";
         public const string NOT_ALLOWED_ESTIMATE_HOURS_ZERO = "必须设置预估工时！";
         public const string NOT_ALLOWED_DELETE_TASK_WHEN_PROJECT_COMPELETE = "任务所属项目已经完成，无法删除！";
         public const string NOT_ALLOWED_DELETE_IF_WORKHOURS_IS_NOT_ZERO = "已经有实际工时的任务无法删除";
         public const string NOT_ALLOWED_DELETE_IF_ROOT = "根任务无法删除";
         public const string NOT_ALLOWED_EDIT_TASK_WHEN_PROJECT_COMPELETE = "任务所属项目已经完成，无法编辑！";
         public const string NOT_ALLOWED_PARENT_EMPTY = "必须选择父任务";
         public const string EDIT_FAILED = "任务编辑失败";
         public const string NOT_ALLOWED_CHANGE_PARENT = "已经有实际工时或维护任务数量的子任务无法更换父任务";
         public const string NOT_ALLOWED_HAS_CHILD_IF_PARENT_IS_PLANTASK = "计划任务不能作为父任务";
         public const string NOT_ALLOWED_DELETE_IF_HAS_WORKHOUR = "已经有实际工时的任务无法删除";
         public const string NOT_ALLOWED_CHANGE_IF_IS_WORKED = "已经有实际工时或工作数量的任务无法修改";
         public const string NOT_ALLOWED_CHANGE_IF_IS_PLANTASK_IS_PROCESS = "已经启动的计划任务无法修改";
         public const string NOT_ALLOWED_CHANGE_IF_IS_NOT_PLAN_STATUS = "非计划期间计划任务无法被修改";
         public const string NOT_ALLOWED_CHANGE_TYPE_IF_HAS_WORK = "已经有工时或者工作数量的子任务无法修改类型";
         public const string NOT_ALLOWED_BE_PARNET_TYPE_IF_LEAF_TASK= "已经有工时的交付（叶子）任务无法变为父任务";
         public const string ALL_TASKS_STARTED = "该项目所有任务均已启动";
         public const string LEAF_TASK_CANNOT_BE_PARENT = "叶子任务不能作为父任务";
         public const string ROOT_TASK_CANNOT_BE_LEAF = "叶子任务不能作为根任务";
         public const string LEAF_TASK_CANNOT_BE_CHANGE = "无法修改填写过工作数量的子任务";
         public const string MAINTAINED_TASK_NOT_ALLOWED_CHANGE_FROM_OTHER_TASK_TYPE = "有工时的非运维任务无法转为运维任务";
         public const string TEMP_TASK_WHICH_HAS_WORKHOURS_NOT_ALLOWED_CHANG_AS_PROJECT_TASK = "已经有实际工时的临时任务，无法添加为项目任务";
         public const string NOT_HAVE_ANY_TASKS = "项目无任务";
         public const string PARENT_ONLY_ALLOW_DELAY_WHEN_PROJECT_START = "项目启动后，父任务时间只能延期";
      }


      public class WorkJournal
      {
         public const string EDIT_FAILED = "日志编辑失败！";
         public const string DATE_OUTOFRANGE = "工时时间超出范围";
         public const string PROGRESS_OUTOFRANGE = "任务进度超出范围";
         public const string NEED_PREVIEW = "需要审核后进度为100%";
         public const string WORK_HOURS_NEED = "必须填写工时";
         public const string SERVICE_COUNT_NEED = "必须填写运维数量";//运维任务专用
         public const string NOT_ALLOWED_ID_NULL = "任务日志ID不能为空";
      }


      public class Resource
      {
         public const string EDIT_FAIL = "资源编辑失败";
         public const string NOT_ALLOWED_NAME_NULL = "资源名称不能为空";
         public const string NOT_ALLOWED_DELETE_DUETO_HASTASKS = "无法删除，该资源是任务执行者或是审核人";
         public const string PM_HAS_EXISTS = "项目经理已经指定！";
         public const string HEADER_HAS_EXISTS = "项目负责人已经指定！";
         public const string LEADER_NOT_EXISTS = "必须指定项目负责人或者项目经理！";
         public const string ISEXIST = "资源已添加";
      }


      public class Review
      {
         public const string HAS_IN_REVIEW = "该任务已提交审核申请，无法重复提交申请";
      }


      public class Role
      {

         public const string EDIT_FAIL = "角色编辑失败！";
         public const string DELETE_FAIL = "角色删除失败！";
         public const string USER_ROLE_EDIT_FAIL = "用户角色设置失败！";
         public const string ROLE_APP_EDIT_FAIL = "角色应用设置失败！";

      }


      public class App
      {

         public const string EDIT_FAIL = "应用编辑失败！";
         public const string DELETE_FAIL = "应用删除失败！";

      }


      public class User
      {

         public const string NOT_ALLOWED_NAME_NULL = "用户名不能为空！";
         public const string NOT_BINDING_ROLES = "用户还未绑定角色";
         public const string NOT_EXISTS = "用户不存在";
      }


      public class Permission
      {
         public const string PERMISSION_DENY = "没有权限访问";
      }

      public class Advice
      {
         public const string NOT_ALLOWED_NULL = "建议实体不能为空！";
         public const string NOT_ALLOWED_TITLE_NULL = "建议标题不能为空！";
         public const string NOT_ALLOWED_CONTENT_NULL = "建议内容不能为空！";
      }

      public class Payments
      {
         public const string EDIT_FAIL = "编辑失败！";
      }

   }


   public static partial class Success
   {

      public class Project
      {

         public const string EDIT_SUCCESS = "项目编辑成功！";
         public const string ADD_MILESTONE_SUCCESS = "里程碑绑定成功";
         public const string Edit_MILESTONE_SUCCESS = "项目里程碑编辑成功";
      }


      public class Task
      {
         public const string EDIT_SUCCESS = "任务编辑成功！";
      }


      public class WorkJournal
      {
         public const string EDIT_SUCCESS = "日志编辑成功！";
      }


      public class Resource
      {
         public const string EDIT_SUCCESS = "资源编辑成功！";
      }


      public class Review { }


      public class Role
      {

         public const string EDIT_SUCCESS = "角色编辑成功！";
         public const string DELETE_SUCCESS = "角色删除成功！";
         public const string USER_ROLE_EDIT_SUCCESS = "用户角色设置成功！";
         public const string ROLE_APP_EDIT_SUCCESS = "角色应用设置成功！";

      }


      public class App
      {
         public const string EDIT_SUCCESS = "应用编辑成功！";
         public const string DELETE_SUCCESS = "应用删除成功！";
      }


      public class User { }


      public class File
      {
         public const string CONFIRM_DELETE = "确定要删除该文件？";
         public const string DELETE_SUCCESS = "文件删除成功！";
      }


      public class Folder
      {
         public const string EDIT_SUCCESS = "文件夹编辑成功！";
         public const string CONFIRM_DELETE = "确定要删除文件夹及其内部所有文件？";
      }


      public class Advice
      {
         public const string EDITSUCCESS = "编辑成功";
      }

      public class MileStone
      {
         public const string EDITSUCCESS = "编辑成功";
      }

      public class Payments
      {
         public const string EDITSUCCESS = "编辑成功";
      }

   }


   public static partial class Confirm
   {

      public class Project
      {

         public const string PROJECT_CONFIRM_START = "确定要项目启动？";
         public const string File_CONFIRM_DELETE = "确定要删除项目文件？";
         public const string PROJECT_CONFIRM_COMPLETE = "确定要标记项目完成？";
         public const string PROJECT_FORCE_CLOSE= "确定要强制关闭本项目？";
      }


      public class Task
      {
         public const string TASK_CONFIRM_START = "确定要任务启动？";
         public const string TASK_CONFIRM_DELETE = "确定要删除项目文件？";
         public const string TASK_SUBMIT_REQUEST = "确定要提交该任务？";
         public const string TASK_SUBMIT_EDIT = "确定要发出修改任务申请？";
      }


      public class Resource
      {
         public const string RESOURCE_CONFIRM_DELETE = "确定要删除资源？";
      }

   }

}
