using Business;
using Business.Config;
using Business.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TheSite.EvalAnalysis;

namespace TheSite.Controllers
{

   public class DesignerController : BaseController
   {

      static APDBDef.DictionaryTableDef d = APDBDef.Dictionary;

      public ActionResult InitialDictionary()
      {
         InitialTaskType();

         return View();
      }

      public void InitialDocumentType()
      {
         var rootId = Guid.Parse(DictionaryKeys.RootId);
         var taskFileTypeId = TaskKeys.FileTypeGuid;

         var taskFileTypeDic = new Dictionary(Guid.NewGuid(), rootId, "任务文档类型", null, null, null, null, 2);
         var requireTypeId = new Dictionary(Guid.NewGuid(), taskFileTypeId, "需求类", null, TaskKeys.RequreFileType.ToString(), null, null, 3);
         var designTypeId = new Dictionary(Guid.NewGuid(), taskFileTypeId, "设计类", null, TaskKeys.DesignFileType.ToString(), null, null, 3);
         var checkAndAcceptTypeId = new Dictionary(Guid.NewGuid(), taskFileTypeId, "验收类", null, TaskKeys.CheckAndAcceptFileType.ToString(), null, null, 3);
         var deliverTypeId = new Dictionary(Guid.NewGuid(), taskFileTypeId, "交付类", null, TaskKeys.DeliverFileType.ToString(), null, null, 3);

         var dicArray = new Dictionary[] { taskFileTypeDic, requireTypeId, designTypeId, checkAndAcceptTypeId, deliverTypeId }.ToList();

         db.DictionaryDal.ConditionDelete(d.ParentID == taskFileTypeId);
         db.DictionaryDal.PrimaryDelete(taskFileTypeId);

         foreach (var item in dicArray)
            db.DictionaryDal.Insert(item);
      }

      public void InitialEval()
      {
         //增加考核管理员
         var evalManagerId = RoleKeys.EvalManagerId.ToGuid(Guid.Empty);
         Role.PrimaryDelete(evalManagerId);
         db.RoleDal.Insert(new Role { RoleId = evalManagerId, RoleName = "考核管理员", RoleType = RoleKeys.SystemType });

         //增加同事角色
         var workMateId = RoleKeys.WorkMateRoleId.ToGuid(Guid.Empty);
         Role.PrimaryDelete(workMateId);
         db.RoleDal.Insert(new Role { RoleId = workMateId, RoleName = "同事", RoleType = RoleKeys.SystemType });

         db.EvalGroupDal.ConditionDelete(null);

         //设置默认考核组
         db.EvalGroupDal.Insert(new EvalGroup
         {
            GroupId = EvalGroupConfig.DefaultGroupId.ToGuid(Guid.Empty),
            GroupName = EvalGroupConfig.DefaultName,
            CreaterId = GetUserInfo().UserId,
            CreateDate = DateTime.Now
         });

         db.TaskCompelxtiyRoleDal.ConditionDelete(null);

         var crs = new List<TaskCompelxtiyRole>
         {
            new TaskCompelxtiyRole
            {
               CompelxtiyRoleId = Guid.NewGuid(),
               RoleId = RoleKeys.ProjectManagerId.ToGuid(Guid.Empty), //项目经理默认占比
               Propertion = 50,
               IsStandard = false,
            },
          new TaskCompelxtiyRole
            {
               CompelxtiyRoleId = Guid.NewGuid(),
               RoleId =Guid.Empty, //技术经理默认占比
               Propertion = 30,
               IsStandard = true,
            },
            new TaskCompelxtiyRole
            {
               CompelxtiyRoleId = Guid.NewGuid(),
               RoleId =Guid.Empty, //标准复杂度默认占比
               Propertion = 20,
               IsStandard = true,
            },
         };

         //设置默认复杂度操作角色
         foreach (var item in crs)
            db.TaskCompelxtiyRoleDal.Insert(item);


         // 删除原先的自动考核指标
         var i = APDBDef.Indication;
         db.IndicationDal.ConditionDelete(i.IndicationType == IndicationKeys.AutoType);

         //将自动计算的指标放入数据库
         var indicationIds = new Dictionary<string, Guid> {
            { "工作量", DefaultAlgorithms.WorkQuantityId },
            { "工作效率", DefaultAlgorithms.WorkEfficiencyId },
            { "工作复杂度", DefaultAlgorithms.WorkComplexityId },
            { "BUG量", DefaultAlgorithms.BUGQuantityId },
            { "交付物确认数量", DefaultAlgorithms.TaskUploadFileQuantityId },
            { "执行力", DefaultAlgorithms.ExcutiveCapabilityId },
            { "成本控制", DefaultAlgorithms.CostControlId },
            { "预算偏差", DefaultAlgorithms.BugetDiviationId },
            { "项目质量", DefaultAlgorithms.ProjectQualtiyId },

         };

         foreach (var item in indicationIds)
         {
            db.IndicationDal.Insert(new Indication
            {
               IndicationId = item.Value,
               AlgorithmnId = item.Value,
               IndicationName = item.Key,
               CreateDate = DateTime.Now,
               CreaterId = GetUserInfo().UserId,
               IndicationType = IndicationKeys.AutoType,
               IndicationStatus = IndicationKeys.EnabelStatus,
            });
         }


         //删除原先的标准工时和标准复杂度
         var tsi = APDBDef.TaskStandardItem;
         db.TaskStandardItemDal.ConditionDelete(null);

         //任务表新增标准工时和标准复杂度字段
         List<TaskStandardItem> items = new List<TaskStandardItem> {
            new TaskStandardItem { ItemId=Guid.NewGuid(),  ItemName="默认", StandardComplextiy=3, StandardWorkhours=3,SortId=1 },
            new TaskStandardItem { ItemId=Guid.NewGuid(),  ItemName="M1", StandardComplextiy=2, StandardWorkhours=16,SortId=2 },
            new TaskStandardItem { ItemId=Guid.NewGuid(),  ItemName="M2", StandardComplextiy=2, StandardWorkhours=16,SortId=3 },
            new TaskStandardItem { ItemId=Guid.NewGuid(),  ItemName="M3", StandardComplextiy=2, StandardWorkhours=8 ,SortId=4},
            new TaskStandardItem { ItemId=Guid.NewGuid(),  ItemName="M4", StandardComplextiy=2, StandardWorkhours=4 ,SortId=5},
            new TaskStandardItem { ItemId=Guid.NewGuid(),  ItemName="M5", StandardComplextiy=2, StandardWorkhours=8,SortId=6 },

            new TaskStandardItem { ItemId=Guid.NewGuid(),  ItemName="Q1", StandardComplextiy=2, StandardWorkhours=4,SortId=7 },
            new TaskStandardItem { ItemId=Guid.NewGuid(),  ItemName="Q2", StandardComplextiy=2, StandardWorkhours=3,SortId=8 },
            new TaskStandardItem { ItemId=Guid.NewGuid(),  ItemName="Q3", StandardComplextiy=2, StandardWorkhours=0.1 ,SortId=9},
            new TaskStandardItem { ItemId=Guid.NewGuid(),  ItemName="Q4", StandardComplextiy=2, StandardWorkhours=0.1,SortId=10 },
         };

         items.ForEach((item) =>
         {
            db.TaskStandardItemDal.Insert(item);
         });

         //设置所有任务的默认标准复杂度
         var allTasks = WorkTask.GetAll();
         var standardId = items.FirstOrDefault(item => item.ItemName == "默认").ItemId;
         foreach (var item in allTasks)
            WorkTask.UpdatePartial(item.TaskId, new { StandardItemId = standardId });



         //指标类型

         db.DictionaryDal.ConditionDelete(d.ParentID == IndicationKeys.IndicaitonTypeKeyGuid);

         var subjectType = new Dictionary(IndicationKeys.SubjectTypeGuid, IndicationKeys.IndicaitonTypeKeyGuid, "主动", null, IndicationKeys.SubjectType.ToString(), null, null, 4);
         var autoType = new Dictionary(IndicationKeys.AutoTypeGuid, IndicationKeys.IndicaitonTypeKeyGuid, "自动", null, IndicationKeys.AutoType.ToString(), null, null, 4);

         var dicArray = new Dictionary[] { subjectType, autoType }.ToList();

         foreach (var item in dicArray)
            db.DictionaryDal.Insert(item);


         //考核表类型

         db.DictionaryDal.ConditionDelete(d.ParentID == EvalTableKeys.TableTypeKeyGuid);

         subjectType = new Dictionary(EvalTableKeys.SubjectTypeGuid, EvalTableKeys.TableTypeKeyGuid, "主动", null, EvalTableKeys.SubjectType.ToString(), null, null, 4);
         autoType = new Dictionary(EvalTableKeys.AutoTypeGuid, EvalTableKeys.TableTypeKeyGuid, "自动", null, EvalTableKeys.AutoType.ToString(), null, null, 4);

         dicArray = new Dictionary[] { subjectType, autoType }.ToList();

         foreach (var item in dicArray)
            db.DictionaryDal.Insert(item);


         //考核表状态

         db.DictionaryDal.ConditionDelete(d.ParentID == EvalTableKeys.TableStatusKeyGuid);

         var disableStatus = new Dictionary(EvalTableKeys.DisableStatusGuid, EvalTableKeys.TableStatusKeyGuid, "禁用", null, EvalTableKeys.DisableStatus.ToString(), null, null, 4);
         var readyStatus = new Dictionary(EvalTableKeys.ReadyStatusGuid, EvalTableKeys.TableStatusKeyGuid, "准备中", null, EvalTableKeys.ReadyStatus.ToString(), null, null, 4);
         var processStatus = new Dictionary(EvalTableKeys.ProcessStatusGuid, EvalTableKeys.TableStatusKeyGuid, "执行中", null, EvalTableKeys.ProcessStatus.ToString(), null, null, 4);
         var doneStatus = new Dictionary(EvalTableKeys.DoneStatusGuid, EvalTableKeys.TableStatusKeyGuid, "制作完毕", null, EvalTableKeys.DoneStatus.ToString(), null, null, 4);

         dicArray = new Dictionary[] { disableStatus, readyStatus, processStatus, doneStatus }.ToList();

         foreach (var item in dicArray)
            db.DictionaryDal.Insert(item);


         //需要还原applibrary， workflowtask, review, dictionary 表从生产环境导出(删除现有表),review 重新设置主键，dbConnection 表中设置本地连接字符串

         //设置某人为考核管理员角色

         //设置一些表的Ntext属性为nvarchar(max) 包含 EvalGroupAccessor  EvalGroupTarget  Indication

         //所有人员添加“同事”角色

      }

      public void InitialTaskType()
      {
         var taskTypeId = TaskKeys.TypeGuid;
         var projectTaskTypeId = TaskKeys.ProjectTaskType;
         var tempTaskTypeId = TaskKeys.TempTaskType;
         var documentTaskTypeId = TaskKeys.DocumentTaskType;
         var maintanceTaskTypeId = TaskKeys.MaintainedTaskType;
         var manageTaskTypeId = TaskKeys.ManageTaskType;
         var planTaskTypeId = TaskKeys.PlanTaskTaskType;
         var designTaskTypeId = TaskKeys.DesignFileType;
         var developTaskTypeId = TaskKeys.DevelopTaskType;
         var testTaskTypeId = TaskKeys.TestTaskType;
         var deployTaskTypeId = TaskKeys.DeployTaskType;
         var officeTaskTypeId = TaskKeys.OfficeTaskType;

         //任务大类
         var projectTaskType = new Dictionary(projectTaskTypeId, taskTypeId, "项目任务", null, "1", null, null, 3);
         var planTaskType = new Dictionary(planTaskTypeId, taskTypeId, "计划任务", null, "1", null, null, 3);
         var tempTaskType = new Dictionary(tempTaskTypeId, taskTypeId, "临时任务", null, "2", null, null, 3);
         var maintanceTaskType = new Dictionary(maintanceTaskTypeId, taskTypeId, "运维类任务", null, "3", null, null, 3);
         var documentTaskType = new Dictionary(documentTaskTypeId, taskTypeId, "文档类任务", null, "4", null, null, 3);
         var manageTaskType = new Dictionary(manageTaskTypeId, taskTypeId, "管理类任务", null, "5", null, null, 3);
         var designTaskType = new Dictionary(designTaskTypeId, taskTypeId, "设计类任务", null, "6", null, null, 3);
         var developTaskType = new Dictionary(developTaskTypeId, taskTypeId, "开发类任务", null, "7", null, null, 3);
         var testTaskType = new Dictionary(testTaskTypeId, taskTypeId, "测试类任务", null, "8", null, null, 3);
         var deployTaskType = new Dictionary(deployTaskTypeId, taskTypeId, "部署类任务", null, "9", null, null, 3);
         var officeTaskType = new Dictionary(officeTaskTypeId, taskTypeId, "内勤类任务", null, "10", null, null, 3);

         //任务子类型默认
         var defaultSubTypeId = Guid.NewGuid();
         var dst = new Dictionary(defaultSubTypeId, defaultSubTypeId, "默认", null, null, null, null, 1);

         //文档任务子类型
         var st = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "PPT制作（创新）", "0.3", "W01", "0.3'/页", "页", 4);
         var st2 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "PPT制作（借鉴）", "0.05", "W02", "0.05'/页", "页", 4);
         var st3 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "PPT审阅", "0.063", "W03", "0.063 '/页", "页", 4);
         var st4 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "资料编制（创新）", "5", "W04", "5 '/篇", "篇", 4);
         var st5 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "资料整理（收集）", "0.25", "W05", "0.25'/页", "页", 4);
         var st6 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "资料整理（规范）", "0.119", "W06", "0.119/页", "页", 4);



         ////运维任务子类型
         //var st4 = new Dictionary(Guid.NewGuid(), maintanceTaskTypeId, "日常巡检", "1", "6", "1 '/次", "次", 4);
         //var st5 = new Dictionary(Guid.NewGuid(), maintanceTaskTypeId, "技术支持（远程）", "2", "7", "2 '/次", "次", 4);
         //var st6 = new Dictionary(Guid.NewGuid(), maintanceTaskTypeId, "技术支持（上门）", "10", "8", "10 '/次", "次", 4);
         //var st10 = new Dictionary(Guid.NewGuid(), maintanceTaskTypeId, "原运维任务", "0", "0", "0 '/个", "个", 4);

         ////管理类任务子类型
         //var st7 = new Dictionary(Guid.NewGuid(), manageTaskTypeId, "试用期员工管理", "1.375", "9", "1.375 '/人周", "人周", 4);
         //var st8 = new Dictionary(Guid.NewGuid(), manageTaskTypeId, "预决算管理", "3", "10", "3 '/个", "个", 4);
         //var st9 = new Dictionary(Guid.NewGuid(), manageTaskTypeId, "需求管理", "0.5", "11", "0.5 '/次", "次", 4);

         var dicArray = new Dictionary[] {
            projectTaskType,
            planTaskType,
            tempTaskType,
            maintanceTaskType,
            documentTaskType,
            manageTaskType,
            dst,
            st,
            st2,
            st3,
            st4,
            st5,
            st6,
            st7,
            st8,
            st9
         }.ToList();

         db.DictionaryDal.ConditionDelete(
            d.ParentID == taskTypeId |
            d.ParentID == projectTaskTypeId |
            d.ParentID == tempTaskTypeId |
            d.ParentID == documentTaskTypeId |
            d.ParentID == maintanceTaskTypeId |
            d.ParentID == manageTaskTypeId
            );
         foreach (var item in dicArray)
            db.DictionaryDal.Insert(item);


         // 填了value,不让其成为父任务，不让其修改子类型
      }

      // private List<Guid> ProjectRoles => RoleMapping.Select(x => x.Value.ToGuid(Guid.Empty)).ToList();


      //private Dictionary<int, string> RoleMapping = new Dictionary<int, string>
      //{
      //  {1,"D832C25C-B4A2-40B6-8D25-28B1A54D2855" },
      //  {2,"E4CE70F5-E2FB-4E26-9FE2-5712DC6C0F19" },
      //  {3,"75FDECAD-434E-40F7-943E-1B46AC9B8DF8" },
      //  {4,"B72E4531-DDAD-4ADE-A9FC-F6D61275952C" },
      //  {5,"7C83D402-4D02-4975-B894-5AED20F04847" },
      //  {6,"F2810545-D8DA-4B0C-A9AC-062AC3542791" },
      //  {7,"4FDB1AEC-0B46-403E-B434-D471E508F18F" },
      //  {8,"895CBEF0-B7C5-48C4-A0B7-58D63547088B" },
      //  {9,"BA7D6C28-1D3E-459E-96A0-3D34BC2BF710" },
      //  {10,"40952B32-0521-4072-9AC4-14C69A7A482F" },
      //};

      //private Dictionary<string, Project> _projects = new Dictionary<string, Project>
      //{
      //   {"LX201410821",new Project { ProjectId=Guid.NewGuid(),Code="LX201410821", ProjectName="电教馆学生学籍注册系统",ProjectOwner="电教馆", ProjectExecutor="电达" } },
      //   {"LX201510301",new Project { ProjectId=Guid.NewGuid(),Code="LX201510301", ProjectName="上海市长宁区教育信息中心服务器设备租赁协议",ProjectOwner="长宁教育学院", ProjectExecutor="电达"  } },
      //   {"LX201510305",new Project { ProjectId=Guid.NewGuid(),Code="LX201510305", ProjectName="不间断电源设备租赁",ProjectOwner="教研室" , ProjectExecutor="电达" } },
      //   {"LX201510313",new Project { ProjectId=Guid.NewGuid(),Code="LX201510313", ProjectName="上海市教委教研室平板电脑租赁",ProjectOwner="教研室", ProjectExecutor="电达"  } },
      //   {"LX201510323",new Project { ProjectId=Guid.NewGuid(),Code="LX201510323", ProjectName="上海教育丛书编委会平板电脑租赁",ProjectOwner="上海市中小学幼儿教师奖励基金会" , ProjectExecutor="电达" } },
      //   {"LX201510326",new Project { ProjectId=Guid.NewGuid(),Code="LX201510326", ProjectName="上海市普通高中学生综合素质评价信息管理平台（一期）建设",ProjectOwner="电教馆" , ProjectExecutor="电达" } },
      //   {"LX201510329",new Project { ProjectId=Guid.NewGuid(),Code="LX201510329", ProjectName="上海市教委教研室iPad Air2平板电脑租赁服务",ProjectOwner="教研室"  , ProjectExecutor="电达"} },
      //   {"LX201510608",new Project { ProjectId=Guid.NewGuid(),Code="LX201510608", ProjectName="上海市电化教育馆基础教育统一身份认证服务平台建设",ProjectOwner="电教馆", ProjectExecutor="电达"  } },
      //   {"LXJR201510601",new Project { ProjectId=Guid.NewGuid(),Code="LXJR201510601", ProjectName="上海市教育学会 台式一体机打印机笔记本电脑设备租赁",ProjectOwner="上海市教育学会" ,ProjectExecutor="教软"  } },
      //   {"LXJR201510602",new Project { ProjectId=Guid.NewGuid(),Code="LXJR201510602", ProjectName="上海市教育委员会教学研究室网络设备租赁（2015-2020）",ProjectOwner="教研室" ,ProjectExecutor="教软"  } },
      //   {"LX201510609",new Project { ProjectId=Guid.NewGuid(),Code="LX201510609", ProjectName="上海市中小学专题教育网络平台升级改造及支持服务",ProjectOwner="电教馆" ,ProjectExecutor="电达" } },
      //   {"LXJR201510604",new Project { ProjectId=Guid.NewGuid(),Code="LXJR201510604", ProjectName="乒乓球训练自适应学习和评分系统",ProjectOwner="上海市电化教育馆"  ,ProjectExecutor="教软"} },
      //   {"LXJR201510606",new Project { ProjectId=Guid.NewGuid(),Code="LXJR201510606", ProjectName="上海市长宁区教育学院日志服务器租赁服务（2016年-2020年）",ProjectOwner="长宁区教育学院"  ,ProjectExecutor="教软"} },
      //   {"LXJR201510611",new Project { ProjectId=Guid.NewGuid(),Code="LXJR201510611", ProjectName="上海市教育委员会教学研究室惠普扫描仪工作站租赁服务",ProjectOwner="教研室" ,ProjectExecutor="教软" } },
      //   {"LXJR201610602",new Project { ProjectId=Guid.NewGuid(),Code="LXJR201610602", ProjectName="上海市教委教研室信息化建设笔记本电脑租赁服务（2016年-2019年）",ProjectOwner="教研室" ,ProjectExecutor="教软" } },
      //   {"LX201610601",new Project { ProjectId=Guid.NewGuid(),Code="LX201610601", ProjectName="上海市普通高中学生综合素质评价信息管理系统运营支持服务",ProjectOwner="电教馆"  ,ProjectExecutor="电达"} },
      //   {"LX201610602",new Project { ProjectId=Guid.NewGuid(),Code="LX201610602", ProjectName="上海市特教信息化公共服务平台功能升级及运维服务",ProjectOwner="电教馆"  ,ProjectExecutor="电达"} },
      //   {"LXJR201610604",new Project { ProjectId=Guid.NewGuid(),Code="LXJR201610604", ProjectName="上海教委教研室信息化建设网络运维服务（2016年-2019年）",ProjectOwner="教研室"  ,ProjectExecutor="教软"} },
      //   {"LXJR201610607",new Project { ProjectId=Guid.NewGuid(),Code="LXJR201610607", ProjectName="上海市教育委员会教学研究室打印机设备租赁（2016年-2019年）",ProjectOwner="教研室" ,ProjectExecutor="教软" } },
      //   {"LX201610606",new Project { ProjectId=Guid.NewGuid(),Code="LX201610606", ProjectName="虹口区中小学生信息管理平台 ",ProjectOwner="虹口区教育局"  ,ProjectExecutor="电达"} },
      //   {"LX201610607",new Project { ProjectId=Guid.NewGuid(),Code="LX201610607", ProjectName="虹口区教师专业人才梯队建设管理平台二期",ProjectOwner="上海市虹口区教育校产基建管理站"  ,ProjectExecutor="电达"} },
      //   {"LXJR201610611",new Project { ProjectId=Guid.NewGuid(),Code="LXJR201610611", ProjectName="长三角优质教育资源网升级及运维服务",ProjectOwner="电教馆"  ,ProjectExecutor="教软"} },
      //   {"LX201610610",new Project { ProjectId=Guid.NewGuid(),Code="LX201610610", ProjectName="上海市义务教育入学报名系统一站式服务",ProjectOwner="电教馆"  ,ProjectExecutor="电达"} },
      //   {"LX201610611",new Project { ProjectId=Guid.NewGuid(),Code="LX201610611", ProjectName="学科德育数字化资源再加工服务",ProjectOwner="电教馆"  ,ProjectExecutor="电达"} },
      //   {"LXJR201610618",new Project { ProjectId=Guid.NewGuid(),Code="LXJR201610618", ProjectName="学生成长数据汇聚平台",ProjectOwner="电教馆"  ,ProjectExecutor="教软"} },
      //   {"LXJR201610612",new Project { ProjectId=Guid.NewGuid(),Code="LXJR201610612", ProjectName="教研室专项会议支持（数学教育改革经验总结交流会及教学评价推进)",ProjectOwner="教研室" ,ProjectExecutor="教软" } },
      //   {"LXJR201610619",new Project { ProjectId=Guid.NewGuid(),Code="LXJR201610619", ProjectName="上海市普通高中学生综合素质评价信息管理系统综合门户",ProjectOwner="电教馆" ,ProjectExecutor="教软" } },
      //   {"LX201610614",new Project { ProjectId=Guid.NewGuid(),Code="LX201610614", ProjectName="上海市普通高中学生综合素质评价信息管理系统呼叫中心客户服务支持",ProjectOwner="电教馆"  ,ProjectExecutor="电达"} },
      //   {"LXJR201610620",new Project { ProjectId=Guid.NewGuid(),Code="LXJR201610620", ProjectName="上海市中小幼教师信息技术应用能力提升工程全员培训支持服务采购需求",ProjectOwner="电教馆"  ,ProjectExecutor="教软"} },
      //   {"LXJR201610613",new Project { ProjectId=Guid.NewGuid(),Code="LXJR201610613", ProjectName="上海市宝山区行知实验幼儿园网站系统建设",ProjectOwner="上海市宝山区行知实验幼儿园" ,ProjectExecutor="教软" } },
      //   {"LXJR201610614",new Project { ProjectId=Guid.NewGuid(),Code="LXJR201610614", ProjectName="教研室DHCP服务器及File存储设备租赁服务",ProjectOwner="教研室"  ,ProjectExecutor="教软"} },
      //   {"LXJR201610615",new Project { ProjectId=Guid.NewGuid(),Code="LXJR201610615", ProjectName="教研室网络环境及服务器设备运行情况展示服务",ProjectOwner="教研室"  ,ProjectExecutor="教软"} },
      //   {"LX201610615",new Project { ProjectId=Guid.NewGuid(),Code="LX201610615", ProjectName="教研室网络环境及服务器设备运行情况展示服务",ProjectOwner="教研室"  ,ProjectExecutor="电达"} },
      //   {"LX201610616",new Project { ProjectId=Guid.NewGuid(),Code="LX201610616", ProjectName="2017年中小学生学籍业务运营综合支持服务",ProjectOwner="电教馆"  ,ProjectExecutor="电达"} },
      //   {"LX201610617",new Project { ProjectId=Guid.NewGuid(),Code="LX201610617", ProjectName="综评系统高校管理平台及高校信息化平台对接服务",ProjectOwner="电教馆"  ,ProjectExecutor="电达"} },
      //   {"LX201610618",new Project { ProjectId=Guid.NewGuid(),Code="LX201610618", ProjectName="上海市普通高中学生综合素质评价信息管理系统（二期）",ProjectOwner="电教馆" ,ProjectExecutor="电达" } },
      //   {"LX201610619",new Project { ProjectId=Guid.NewGuid(),Code="LX201610619", ProjectName="义务教育招生业务数据监控及运营综合支持服务",ProjectOwner="电教馆"  ,ProjectExecutor="电达"} },
      //   {"LX201610620",new Project { ProjectId=Guid.NewGuid(),Code="LX201610620", ProjectName="上海市义务教育入学报名业务监控服务和移动应用开发",ProjectOwner="电教馆"  ,ProjectExecutor="电达"} },
      //   {"LX201610621",new Project { ProjectId=Guid.NewGuid(),Code="LX201610621", ProjectName="上海市市区两级学籍系统的运维与安全管理服务",ProjectOwner="电教馆"  ,ProjectExecutor="电达"} },
      //   {"LX201610622",new Project { ProjectId=Guid.NewGuid(),Code="LX201610622", ProjectName="上海市教师信息技术应用能力提升工程技术素养类课程资",ProjectOwner="电教馆" ,ProjectExecutor="电达" } },
      //   {"LXJR201610624",new Project { ProjectId=Guid.NewGuid(),Code="LXJR201610624", ProjectName="教研室办公设备续租续租服务",ProjectOwner="教研室"  ,ProjectExecutor="教软"} },
      //   {"LXJR201610626",new Project { ProjectId=Guid.NewGuid(),Code="LXJR201610626", ProjectName="上海市教育督导事务中心笔记本电脑租赁项目",ProjectOwner="上海市教育督导事务中心" ,ProjectExecutor="教软" } },
      //   {"LXJR201610628",new Project { ProjectId=Guid.NewGuid(),Code="LXJR201610628", ProjectName="教研室群晖存储设备租赁",ProjectOwner="教研室"  ,ProjectExecutor="教软"} },
      //   {"LXJR201710601",new Project { ProjectId=Guid.NewGuid(),Code="LXJR201710601", ProjectName="宝山区信息技术应用能力提升工程培训支持服务",ProjectOwner="宝山区教育学院"  ,ProjectExecutor="教软"} },

      //};

   }

}