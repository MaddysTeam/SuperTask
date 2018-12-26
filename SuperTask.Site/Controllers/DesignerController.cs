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
         // InitialTaskType();
         //InitialEval();

         InitialProjectMileStone();

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
         //var evalManagerId = RoleKeys.EvalManagerId.ToGuid(Guid.Empty);
         //Role.PrimaryDelete(evalManagerId);
         //db.RoleDal.Insert(new Role { RoleId = evalManagerId, RoleName = "考核管理员", RoleType = RoleKeys.SystemType });

         ////增加同事角色
         //var workMateId = RoleKeys.WorkMateRoleId.ToGuid(Guid.Empty);
         //Role.PrimaryDelete(workMateId);
         //db.RoleDal.Insert(new Role { RoleId = workMateId, RoleName = "同事", RoleType = RoleKeys.SystemType });

         //db.EvalGroupDal.ConditionDelete(null);

         ////设置默认考核组
         //db.EvalGroupDal.Insert(new EvalGroup
         //{
         //   GroupId = EvalGroupConfig.DefaultGroupId.ToGuid(Guid.Empty),
         //   GroupName = EvalGroupConfig.DefaultName,
         //   CreaterId = GetUserInfo().UserId,
         //   CreateDate = DateTime.Now
         //});

         //db.TaskCompelxtiyRoleDal.ConditionDelete(null);

         //var crs = new List<TaskCompelxtiyRole>
         //{
         //   new TaskCompelxtiyRole
         //   {
         //      CompelxtiyRoleId = Guid.NewGuid(),
         //      RoleId = RoleKeys.ProjectManagerId.ToGuid(Guid.Empty), //项目经理默认占比
         //      Propertion = 50,
         //      IsStandard = false,
         //   },
         // new TaskCompelxtiyRole
         //   {
         //      CompelxtiyRoleId = Guid.NewGuid(),
         //      RoleId =Guid.Empty, //技术经理默认占比
         //      Propertion = 30,
         //      IsStandard = true,
         //   },
         //   new TaskCompelxtiyRole
         //   {
         //      CompelxtiyRoleId = Guid.NewGuid(),
         //      RoleId =Guid.Empty, //标准复杂度默认占比
         //      Propertion = 20,
         //      IsStandard = true,
         //   },
         //};

         ////设置默认复杂度操作角色
         //foreach (var item in crs)
         //   db.TaskCompelxtiyRoleDal.Insert(item);


         //// 删除原先的自动考核指标
         //var i = APDBDef.Indication;
         //db.IndicationDal.ConditionDelete(i.IndicationType == IndicationKeys.AutoType);

         //将自动计算的指标放入数据库
         var indicationIds = new Dictionary<string, Guid> {
            //{ "工作量", DefaultAlgorithms.WorkQuantityId },
            //{ "工作效率", DefaultAlgorithms.WorkEfficiencyId },
            //{ "工作复杂度", DefaultAlgorithms.WorkComplexityId },
            //{ "BUG量", DefaultAlgorithms.BUGQuantityId },
            //{ "交付物确认数量", DefaultAlgorithms.TaskUploadFileQuantityId },
            //{ "执行力", DefaultAlgorithms.ExcutiveCapabilityId },
            //{ "成本控制", DefaultAlgorithms.CostControlId },
            //{ "预算偏差", DefaultAlgorithms.BugetDiviationId },
            //{ "项目质量", DefaultAlgorithms.ProjectQualtiyId },

            //{"工作量", DefaultAlgorithms.TaskQuantityId },
            //{"计划完成度", DefaultAlgorithms.PlanTaskComplentionId },
            //{"计划时效性", DefaultAlgorithms.PlanTaskTimelinessId }
              {"日志更新时效性", DefaultAlgorithms.WorkJournalFillingRateId }
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
         //var tsi = APDBDef.TaskStandardItem;
         //db.TaskStandardItemDal.ConditionDelete(null);

         ////任务表新增标准工时和标准复杂度字段
         //List<TaskStandardItem> items = new List<TaskStandardItem> {
         //   new TaskStandardItem { ItemId=Guid.NewGuid(),  ItemName="默认", StandardComplextiy=3, StandardWorkhours=3,SortId=1 },
         //   new TaskStandardItem { ItemId=Guid.NewGuid(),  ItemName="M1", StandardComplextiy=2, StandardWorkhours=16,SortId=2 },
         //   new TaskStandardItem { ItemId=Guid.NewGuid(),  ItemName="M2", StandardComplextiy=2, StandardWorkhours=16,SortId=3 },
         //   new TaskStandardItem { ItemId=Guid.NewGuid(),  ItemName="M3", StandardComplextiy=2, StandardWorkhours=8 ,SortId=4},
         //   new TaskStandardItem { ItemId=Guid.NewGuid(),  ItemName="M4", StandardComplextiy=2, StandardWorkhours=4 ,SortId=5},
         //   new TaskStandardItem { ItemId=Guid.NewGuid(),  ItemName="M5", StandardComplextiy=2, StandardWorkhours=8,SortId=6 },

         //   new TaskStandardItem { ItemId=Guid.NewGuid(),  ItemName="Q1", StandardComplextiy=2, StandardWorkhours=4,SortId=7 },
         //   new TaskStandardItem { ItemId=Guid.NewGuid(),  ItemName="Q2", StandardComplextiy=2, StandardWorkhours=3,SortId=8 },
         //   new TaskStandardItem { ItemId=Guid.NewGuid(),  ItemName="Q3", StandardComplextiy=2, StandardWorkhours=0.1 ,SortId=9},
         //   new TaskStandardItem { ItemId=Guid.NewGuid(),  ItemName="Q4", StandardComplextiy=2, StandardWorkhours=0.1,SortId=10 },
         //};

         //items.ForEach((item) =>
         //{
         //   db.TaskStandardItemDal.Insert(item);
         //});

         ////设置所有任务的默认标准复杂度
         //var allTasks = WorkTask.GetAll();
         //var standardId = items.FirstOrDefault(item => item.ItemName == "默认").ItemId;
         //foreach (var item in allTasks)
         //   WorkTask.UpdatePartial(item.TaskId, new { StandardItemId = standardId });



         ////指标类型

         //db.DictionaryDal.ConditionDelete(d.ParentID == IndicationKeys.IndicaitonTypeKeyGuid);

         //var subjectType = new Dictionary(IndicationKeys.SubjectTypeGuid, IndicationKeys.IndicaitonTypeKeyGuid, "主动", null, IndicationKeys.SubjectType.ToString(), null, null, 4);
         //var autoType = new Dictionary(IndicationKeys.AutoTypeGuid, IndicationKeys.IndicaitonTypeKeyGuid, "自动", null, IndicationKeys.AutoType.ToString(), null, null, 4);

         //var dicArray = new Dictionary[] { subjectType, autoType }.ToList();

         //foreach (var item in dicArray)
         //   db.DictionaryDal.Insert(item);


         ////考核表类型

         //db.DictionaryDal.ConditionDelete(d.ParentID == EvalTableKeys.TableTypeKeyGuid);

         //subjectType = new Dictionary(EvalTableKeys.SubjectTypeGuid, EvalTableKeys.TableTypeKeyGuid, "主动", null, EvalTableKeys.SubjectType.ToString(), null, null, 4);
         //autoType = new Dictionary(EvalTableKeys.AutoTypeGuid, EvalTableKeys.TableTypeKeyGuid, "自动", null, EvalTableKeys.AutoType.ToString(), null, null, 4);

         //dicArray = new Dictionary[] { subjectType, autoType }.ToList();

         //foreach (var item in dicArray)
         //   db.DictionaryDal.Insert(item);


         ////考核表状态

         //db.DictionaryDal.ConditionDelete(d.ParentID == EvalTableKeys.TableStatusKeyGuid);

         //var disableStatus = new Dictionary(EvalTableKeys.DisableStatusGuid, EvalTableKeys.TableStatusKeyGuid, "禁用", null, EvalTableKeys.DisableStatus.ToString(), null, null, 4);
         //var readyStatus = new Dictionary(EvalTableKeys.ReadyStatusGuid, EvalTableKeys.TableStatusKeyGuid, "准备中", null, EvalTableKeys.ReadyStatus.ToString(), null, null, 4);
         //var processStatus = new Dictionary(EvalTableKeys.ProcessStatusGuid, EvalTableKeys.TableStatusKeyGuid, "执行中", null, EvalTableKeys.ProcessStatus.ToString(), null, null, 4);
         //var doneStatus = new Dictionary(EvalTableKeys.DoneStatusGuid, EvalTableKeys.TableStatusKeyGuid, "制作完毕", null, EvalTableKeys.DoneStatus.ToString(), null, null, 4);

         //dicArray = new Dictionary[] { disableStatus, readyStatus, processStatus, doneStatus }.ToList();

         //foreach (var item in dicArray)
         //   db.DictionaryDal.Insert(item);


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

         //新增的文档任务
         var st1 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "投标响应技术文档（主）", "20", "W22", "20'/篇", "篇", 4);
         var st2 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "投标响应技术文档（次）", "10", "W23", "10'/篇", "篇", 4);
         var st3 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "投标响应商务文档", "5", "W24", "5'/篇", "篇", 4);
         var st4 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "甲方文档代编写（申报类）", "15", "W25", "15'/篇", "篇", 4);
         var st5 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "甲方文档代编写（管理类）", "4", "W26", "4'/篇", "篇", 4);
         var st6 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "新闻稿", "2", "W27", "2'/篇", "篇", 4);
         var st7 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "课程脚本设计", "4", "W28", "4'/页", "页", 4);
         var st8 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "新闻稿撰写润色完善", "1", "W29", "1'/篇", "篇", 4);
         var st9 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "项目课题研究报告", "8", "W30", "8'/篇", "篇", 4);
         var st10 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "项目推广文案", "5", "W31", "5'/篇", "篇", 4);
         var st11 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "单一来源公示", "2", "W32", "2'/篇", "篇", 4);

         var st12 = new Dictionary(Guid.NewGuid(), designTaskTypeId, "网站更新图片设计", "2", "S09", "2'/张", "张", 4);

         var st13 = new Dictionary(Guid.NewGuid(), maintanceTaskTypeId, "网站内容审核及更新", "0.5", "Y21", "0.5'/条", "条", 4);
         var st14 = new Dictionary(Guid.NewGuid(), maintanceTaskTypeId, "网站业务数据核查", "1", "Y22", "1'/份", "份", 4);
         var st15 = new Dictionary(Guid.NewGuid(), maintanceTaskTypeId, "导入数据整理（简单）", "2", "Y22", "2'/个", "个", 4);
         var st16 = new Dictionary(Guid.NewGuid(), maintanceTaskTypeId, "导入数据整理（复杂）", "5", "Y23", "5'/个", "个", 4);
         var st17 = new Dictionary(Guid.NewGuid(), maintanceTaskTypeId, "课程视频审核", "2", "Y24", "2'/个", "个", 4);

         var st18= new Dictionary(Guid.NewGuid(), manageTaskTypeId, "会务组织及支持（主持）", "1", "G08", "1'/小时", "小时", 4);
         var st19= new Dictionary(Guid.NewGuid(), manageTaskTypeId, "会务组织及支持（参与）", "0.5", "G09", "0.5'/小时", "小时", 4);
         var st20 = new Dictionary(Guid.NewGuid(), manageTaskTypeId, "活动现场采风", "3", "G10", "3'/次", "次", 4);
         var st21 = new Dictionary(Guid.NewGuid(), manageTaskTypeId, "客户拜访洽谈", "3", "G11", "3'/次", "次", 4);
         var st22 = new Dictionary(Guid.NewGuid(), manageTaskTypeId, "应标谈判", "3", "G12", "3'/次", "次", 4);

         var st23 = new Dictionary(Guid.NewGuid(), officeTaskTypeId, "协助友商，申请用章、资质文件外借", "0.5", "N17", "0.5'/次", "次", 4);
         var st24 = new Dictionary(Guid.NewGuid(), officeTaskTypeId, "外出盖章", "3", "N18", "3'/次", "次", 4);

         //文档任务子类型
         //var st1 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "PPT制作（创新）", "0.3", "W01", "0.3'/页", "页", 4);
         //var st2 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "PPT制作（借鉴）", "0.05", "W02", "0.05'/页", "页", 4);
         //var st3 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "PPT审阅", "0.063", "W03", "0.063 '/页", "页", 4);
         //var st4 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "资料编制（创新）", "5", "W04", "5 '/篇", "篇", 4);
         //var st5 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "资料整理（收集）", "0.25", "W05", "0.25'/页", "页", 4);
         //var st6 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "资料整理（规范）", "0.119", "W06", "0.119/页", "页", 4);
         //var st7 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "资料文档审阅", "0.09", "W07", "0.09/页", "页", 4);
         //var st8 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "项目验收材料汇编", "3", "W08", "3/页", "页", 4);
         //var st9 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "会议纪要（外部）", "1", "W09", "1/篇", "篇", 4);
         //var st10 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "会议纪要（内部）", "0.8", "W10", "0.8/篇", "篇", 4);
         //var st11 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "项目合同编制", "2", "W10", "2/篇", "篇", 4);
         //var st12 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "招标需求编制（简单）", "8", "W12", "8/篇", "篇", 4);
         //var st13 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "招标需求编制（复杂）", "10", "W12", "10/篇", "篇", 4);
         //var st14 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "项目方案编制（简单）", "14.286", "W13", "14.286/篇", "篇", 4);
         //var st15 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "项目方案编制（复杂）", "22.222", "W13", "22.222/篇", "篇", 4);
         //var st16 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "项目方案编制（多系统）", "50", "W13", "50/篇", "篇", 4);
         //var st17 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "需求规格说明书", "20", "W14", "20/篇", "篇", 4);
         //var st18 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "需求分析说明书（简单）", "28.571", "W15", "28.571/篇", "篇", 4);
         //var st19 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "需求分析说明书（复杂）", "40", "W15", "40/篇", "篇", 4);
         //var st20= new Dictionary(Guid.NewGuid(), documentTaskTypeId, "概要设计文档", "20", "W16", "20/篇", "篇", 4);
         //var st21 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "详细设计文档（简单）", "25", "W17", "25/篇", "篇", 4);
         //var st22 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "详细设计文档（复杂）", "50", "W17", "50/篇", "篇", 4);
         //var st23 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "用户操作手册", "1", "W18", "1/模块", "模块", 4);
         //var st24 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "系统维护手册", "5", "W19", "5/篇", "篇", 4);
         //var st25 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "运维报告", "2", "W20", "2/篇", "篇", 4);
         //var st26 = new Dictionary(Guid.NewGuid(), documentTaskTypeId, "测试用例编制", "0.15", "W21", "0.15/个", "个", 4);

         //var st27 = new Dictionary(Guid.NewGuid(), designTaskTypeId, "原型制作", "0.8", "S01", "0.8/页", "页", 4);
         //var st28 = new Dictionary(Guid.NewGuid(), designTaskTypeId, "页面设计（首页）", "10", "S02", "10/页", "页", 4);
         //var st29 = new Dictionary(Guid.NewGuid(), designTaskTypeId, "页面设计（内页）", "2.8", "S02", "2.8/页", "页", 4);
         //var st30 = new Dictionary(Guid.NewGuid(), designTaskTypeId, "多媒体制作（片头动画）", "0.7", "S03", "0.7/秒", "秒", 4);
         //var st31 = new Dictionary(Guid.NewGuid(), designTaskTypeId, "多媒体制作（情景交互）", "0.35", "S03", "0.35/秒", "秒", 4);
         //var st32 = new Dictionary(Guid.NewGuid(), designTaskTypeId, "多媒体制作（情景动画）", "0.28", "S03", "0.28/秒", "秒", 4);
         //var st33 = new Dictionary(Guid.NewGuid(), designTaskTypeId, "海报画册设计", "10", "S04", "10/套", "套", 4);
         //var st34 = new Dictionary(Guid.NewGuid(), designTaskTypeId, "系统前端设计", "15", "S05", "15/套", "套", 4);
         //var st35 = new Dictionary(Guid.NewGuid(), designTaskTypeId, "系统架构设计", "15", "S06", "15/套", "套", 4);
         //var st36 = new Dictionary(Guid.NewGuid(), designTaskTypeId, "系统数据库设计", "15", "S07", "15/套", "套", 4);
         //var st37 = new Dictionary(Guid.NewGuid(), designTaskTypeId, "开发框架设计", "15", "S08", "15/套", "套", 4);

         //var st38 = new Dictionary(Guid.NewGuid(), developTaskTypeId, "UI框架搭建", "15", "K01", "15/套", "套", 4);
         //var st39 = new Dictionary(Guid.NewGuid(), developTaskTypeId, "前端页面开发", "2", "K02", "2/页", "页", 4);
         //var st40 = new Dictionary(Guid.NewGuid(), developTaskTypeId, "静态页面制作", "0.5", "K03", "0.5/页", "页", 4);
         //var st41 = new Dictionary(Guid.NewGuid(), developTaskTypeId, "开发框架搭建（全新）", "3.333", "K04", "3.333/套", "套", 4);
         //var st42 = new Dictionary(Guid.NewGuid(), developTaskTypeId, "开发框架搭建（重建）", "1.667", "K04", "1.667/套", "套", 4);
         //var st43 = new Dictionary(Guid.NewGuid(), developTaskTypeId, "业务功能模块开发", "0.333", "K05", "0.333/个", "个", 4);
         //var st44 = new Dictionary(Guid.NewGuid(), developTaskTypeId, "业务功能模块修改", "0.222", "K06", "0.222/个", "个", 4);
         //var st45 = new Dictionary(Guid.NewGuid(), developTaskTypeId, "核心功能模块开发", "1", "K07", "1/个", "个", 4);
         //var st46 = new Dictionary(Guid.NewGuid(), developTaskTypeId, "核心功能模块修改", "0.5", "K08", "0.5/个", "个", 4);
         //var st47 = new Dictionary(Guid.NewGuid(), developTaskTypeId, "代码检查", "0.8", "K09", "0.8/小时", "小时", 4);

         //var st48 = new Dictionary(Guid.NewGuid(), testTaskTypeId, "业务功能测试", "0.333", "C01", "0.333/个", "个", 4);
         //var st49 = new Dictionary(Guid.NewGuid(), testTaskTypeId, "系统性能测试", "10", "C02", "10/次", "次", 4);
         //var st50 = new Dictionary(Guid.NewGuid(), testTaskTypeId, "系统安全测试", "10", "C03", "10/次", "次", 4);

         //var st51 = new Dictionary(Guid.NewGuid(), deployTaskTypeId, "中间件安装", "2", "B01", "2/个", "个", 4);
         //var st52 = new Dictionary(Guid.NewGuid(), deployTaskTypeId, "应用部署（单点）", "0.5", "B02", "0.5/次", "次", 4);
         //var st53 = new Dictionary(Guid.NewGuid(), deployTaskTypeId, "应用部署（集群）", "1", "B03", "1/次", "次", 4);
         //var st54 = new Dictionary(Guid.NewGuid(), deployTaskTypeId, "配置调优", "5", "B04", "5/次", "次", 4);
         //var st55 = new Dictionary(Guid.NewGuid(), deployTaskTypeId, "数据库部署（单点）", "3", "B05", "3/次", "次", 4);
         //var st56 = new Dictionary(Guid.NewGuid(), deployTaskTypeId, "数据库部署（集群）", "15", "B06", "15/次", "次", 4);

         //var st57 = new Dictionary(Guid.NewGuid(), maintanceTaskTypeId, "日常巡检", "1", "Y01", "1'/次", "次", 4);
         //var st58 = new Dictionary(Guid.NewGuid(), maintanceTaskTypeId, "技术支持（远程）", "2", "Y02", "2'/次", "次", 4);
         //var st59 = new Dictionary(Guid.NewGuid(), maintanceTaskTypeId, "技术支持（上门）", "10", "Y03", "10'/次", "次", 4);
         //var st60 = new Dictionary(Guid.NewGuid(), maintanceTaskTypeId, "数据查询", "0.111", "Y04", "0.111'/次", "次", 4);
         //var st61 = new Dictionary(Guid.NewGuid(), maintanceTaskTypeId, "数据更新", "0.15", "Y05", "0.15'/次", "次", 4);
         //var st62 = new Dictionary(Guid.NewGuid(), maintanceTaskTypeId, "数据统计（≦3表）", "0.3", "Y06", "0.3'/次", "次", 4);
         //var st63 = new Dictionary(Guid.NewGuid(), maintanceTaskTypeId, "数据统计（≦5表）", "0.5", "Y06", "0.5'/次", "次", 4);
         //var st64 = new Dictionary(Guid.NewGuid(), maintanceTaskTypeId, "数据统计（﹥5表）", "1", "Y06", "1'/次", "次", 4);
         //var st65 = new Dictionary(Guid.NewGuid(), maintanceTaskTypeId, "数据备份", "0.5", "Y07", "0.5'/个", "个", 4);
         //var st66 = new Dictionary(Guid.NewGuid(), maintanceTaskTypeId, "数据恢复", "1.5", "Y08", "1.5'/个", "个", 4);
         //var st67 = new Dictionary(Guid.NewGuid(), maintanceTaskTypeId, "数据库脚本编制（普通）", "0.15", "Y09", "0.15'/个", "个", 4);
         //var st68 = new Dictionary(Guid.NewGuid(), maintanceTaskTypeId, "数据库脚本编制（复杂）", "0.333", "Y09", "0.333'/个", "个", 4);
         //var st69 = new Dictionary(Guid.NewGuid(), maintanceTaskTypeId, "数据库补丁升级", "3", "Y10", "3'/个", "个", 4);
         //var st70 = new Dictionary(Guid.NewGuid(), maintanceTaskTypeId, "中间件补丁升级", "2", "Y11", "2'/个", "个", 4);
         //var st71 = new Dictionary(Guid.NewGuid(), maintanceTaskTypeId, "等保协调工作", "15", "Y12", "15'/个", "个", 4);
         //var st72 = new Dictionary(Guid.NewGuid(), maintanceTaskTypeId, "等保修复工作", "25", "Y13", "25'/个", "个", 4);
         //var st73 = new Dictionary(Guid.NewGuid(), maintanceTaskTypeId, "客户培训（讲课）", "10", "Y14", "10'/次", "次", 4);
         //var st74 = new Dictionary(Guid.NewGuid(), maintanceTaskTypeId, "客户培训（安排）", "5", "Y15", "5'/次", "次", 4);
         //var st75 = new Dictionary(Guid.NewGuid(), maintanceTaskTypeId, "通知公告", "1", "Y16", "1'/个", "个", 4);
         //var st76 = new Dictionary(Guid.NewGuid(), maintanceTaskTypeId, "客户服务（QQ/EMAIL/TEL）", "0.25", "Y17", "0.25'/个", "个", 4);
         //var st77 = new Dictionary(Guid.NewGuid(), maintanceTaskTypeId, "原运维任务", "0", "0", "0 '/个", "个", 4);

         //var st78 = new Dictionary(Guid.NewGuid(),manageTaskTypeId, "试用期员工管理", "1.375", "G01", "1.375'/人周", "人周", 4);
         //var st79 = new Dictionary(Guid.NewGuid(), manageTaskTypeId, "预决算管理", "3", "G02", "3'/个", "个", 4);
         //var st80 = new Dictionary(Guid.NewGuid(), manageTaskTypeId, "需求管理", "0.5", "G03", "0.5/次", "次", 4);
         //var st81 = new Dictionary(Guid.NewGuid(), manageTaskTypeId, "实习生管理", "1.375", "G04", "1.375'/人周", "人周", 4);
         //var st82 = new Dictionary(Guid.NewGuid(), manageTaskTypeId, "内部培训（讲师）", "3", "G01", "3'/小时", "小时", 4);
         //var st83 = new Dictionary(Guid.NewGuid(), manageTaskTypeId, "内部培训（学习）", "1", "G01", "1'/小时", "小时", 4);

         //var st84 = new Dictionary(Guid.NewGuid(), officeTaskTypeId, "出差", "8", "N01", "8'/天", "天", 4);
         //var st85 = new Dictionary(Guid.NewGuid(), officeTaskTypeId, "SQA报告", "5", "N02", "5'/次", "次", 4);
         //var st86 = new Dictionary(Guid.NewGuid(), officeTaskTypeId, "文件归档", "0.417", "N03", "0.417'/件", "件", 4);
         //var st87 = new Dictionary(Guid.NewGuid(), officeTaskTypeId, "立项审批和申请", "5", "N04", "5'/件", "件", 4);
         //var st88 = new Dictionary(Guid.NewGuid(), officeTaskTypeId, "创建检查文档", "2.5", "N05", "2.5'/套", "件", 4);
         //var st89 = new Dictionary(Guid.NewGuid(), officeTaskTypeId, "部门管理规范建立", "5", "N06", "5'/页", "页", 4);
         //var st90 = new Dictionary(Guid.NewGuid(), officeTaskTypeId, "用品申领", "0.833", "N07", "0.833'/次", "次", 4);
         //var st91 = new Dictionary(Guid.NewGuid(), officeTaskTypeId, "事务通知", "0.417", "N08", "0.417'/次", "次", 4);
         //var st92 = new Dictionary(Guid.NewGuid(), officeTaskTypeId, "会议室预订", "0.417", "N09", "0.417'/次", "次", 4);
         //var st93 = new Dictionary(Guid.NewGuid(), officeTaskTypeId, "项目报销", "2.5", "N10", "2.5'/次", "次", 4);
         //var st94 = new Dictionary(Guid.NewGuid(), officeTaskTypeId, "交通补助申领和发放", "12.5", "N11", "12.5'/次", "次", 4);
         //var st95 = new Dictionary(Guid.NewGuid(), officeTaskTypeId, "福利发放", "0.833", "N12", "0.833'/次", "次", 4);
         //var st96 = new Dictionary(Guid.NewGuid(), officeTaskTypeId, "行政事务", "0.833", "N13", "0.833'/小时", "小时", 4);


         var dicArray = new Dictionary[] {
            //projectTaskType,
            //planTaskType,
            //tempTaskType,
            //maintanceTaskType,
            //documentTaskType,
            //manageTaskType,
            //designTaskType,
            //developTaskType,
            //testTaskType,
            //deployTaskType,
            //officeTaskType,
            //dst,
            st1,st2,st3,st4,st5,st6,st7,st8,st9,st10,
            st11,st12,st13,st14,st15,st16,st17,st18,st19,st20,
            st21,st22,st23,st24
            //st25,st26,st27,st28,st29,st30,
            //st31,st32,st33,st34,st35,st36,st37,st38,st39,st40,
            //st41,st42,st43,st44,st45,st46,st47,st48,st49,st50,
            //st51,st52,st53,st54,st55,st56,st57,st58,st59,st60,
            //st61,st62,st63,st64,st65,st66,st67,st68,st69,st70,
            //st71,st72,st73,st74,st75,st76,st77,st78,st79,st80,
            //st81,st82,st83,st84,st85,st86,st87,st88,st89,st90,
            //st91,st92,st93,st94,st95,st96

         }.ToList();

         //db.DictionaryDal.ConditionDelete(
         //   d.ParentID == taskTypeId |
         //   d.ParentID == planTaskTypeId |
         //   d.ParentID == projectTaskTypeId |
         //   d.ParentID == tempTaskTypeId |
         //   d.ParentID == documentTaskTypeId |
         //   d.ParentID == maintanceTaskTypeId |
         //   d.ParentID == manageTaskTypeId |
         //   d.ParentID == designTaskTypeId |
         //   d.ParentID == developTaskTypeId |
         //   d.ParentID == testTaskTypeId |
         //   d.ParentID == deployTaskTypeId |
         //   d.ParentID == officeTaskTypeId
         //   );

         foreach (var item in dicArray)
            db.DictionaryDal.Insert(item);


         // 填了value,不让其成为父任务，不让其修改子类型
      }

      public void InitialProjectMileStone()
      {
         //MilestoneHelper.AddProjectMileStone(Guid.Parse("D0ADE72B-C831-4619-9A48-CF457F357BC1"), Guid.Parse("F11DB3FC-0954-4C76-A2F7-D04F1BCCD4A9"), Guid.Parse("336BE3BF-4DCF-4DCB-8C4E-2BE48A1BD03D"), db);
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