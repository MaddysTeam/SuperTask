using LitJson;
using RoadFlow.Data.Model;
using RoadFlow.Data.Model.WorkFlowExecute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Roadflow
{

   public class RoadflowService : IRoadFlow_Service
   {

      RunWorkflow _flow;
      RunAppLibrary _appLib;
      RunTask _task;
      RunUser _users;
      RunDBConnection _conn;
      RunExcute _execute;
      RunWorkflowComment _comment;


      public FlowRunningResult FlowIndex(RunParams paras)
      {
         if (paras == null)
            return null;

         this.CreateInstance();

         var workflow = _flow.GetRunModel(paras.FlowId.ToGuid());
         var error = string.Empty;
         if (!_flow.CheckFlow(workflow, out error))
         {
            return new FlowRunningResult { ErrorMessage = error };
         }

         var currentStep = _flow.GetStep(workflow, paras.Stepid.ToGuid());
         if (currentStep == null)
            return null;

         var userId = paras.UserId.ToGuid();
         var form = currentStep.Forms.First();
         var appLib = _appLib.GetByFormId(form.ID);

         var taskId = paras.Taskid.ToGuid();
         _task.UpdateOpenTime(taskId);
         var task = _task.Get(taskId);

         if (task != null)
         {
            if (task.Status.In(2, 3, 4, 5))
               return new FlowRunningResult { ErrorMessage = "该任务已处理,请刷新您的待办列表!" };
            else if (task.ReceiveID != userId)
               return new FlowRunningResult { ErrorMessage = "您不能处理当前任务,请刷新您的待办列表!" };
         }

         var isSign = currentStep.SignatureType == 1 || currentStep.SignatureType == 2;//是否有意见
         var signType = currentStep.SignatureType;
         var comment = _comment.GetOptionsStringByUserID(userId);


         return new FlowRunningResult
         {
            RunModel = workflow,
            CurrentLibrary = appLib,
            CurrentStep = currentStep,
            CurrentTask = task,
            IsSign = isSign,
            SignatureType = signType,
            Comment = comment
         };
      }


      public FlowRunningResult FlowSend(RunParams paras)
      {
         this.CreateInstance();

         var workflow = _flow.GetRunModel(paras.FlowId.ToGuid());
         var currentStep = _flow.GetStep(workflow, paras.Stepid.ToGuid());
         if (currentStep == null)
            return null;

         var groupid = paras.Groupid;
         var userId = paras.UserId.ToGuid();
         var instanceId = paras.Instanceid;


         var stepUsers = new Dictionary<Guid, string>();

         var nextSteps = _flow.GetNextSteps(workflow.ID, currentStep.ID);
         foreach (var step in nextSteps)
         {
            var defaultMember = "";

            switch (step.Behavior.HandlerType)
            {
               case 5://发起者
                  Guid userid = _task.GetFirstSnderID(workflow.ID, groupid.ToGuid());
                  if (userid != Guid.Empty)
                  {
                     defaultMember = RunUser.PREFIX + userid.ToString();
                  }
                  if (defaultMember.IsNullOrEmpty() && currentStep.ID == workflow.FirstStepID)
                  {
                     defaultMember = RunUser.PREFIX + userId.ToString();
                  }
                  break;
               case 6://前一步骤处理者
                  defaultMember = _task.GetStepSnderIDString(workflow.ID, currentStep.ID, groupid.ToGuid());
                  if (defaultMember.IsNullOrEmpty() && currentStep.ID == workflow.FirstStepID)
                  {
                     defaultMember = RunUser.PREFIX + userId.ToString();
                  }
                  break;
               case 7://某一步骤处理者
                  defaultMember = _task.GetStepSnderIDString(workflow.ID, step.Behavior.HandlerStepID, groupid.ToGuid());
                  if (defaultMember.IsNullOrEmpty() && step.Behavior.HandlerStepID == workflow.FirstStepID)
                  {
                     defaultMember = RunUser.PREFIX + userId.ToString();
                  }
                  break;
               case 8://字段值
                  string linkString = step.Behavior.ValueField;
                  if (!linkString.IsNullOrEmpty() && !instanceId.IsNullOrEmpty() && workflow.DataBases.Count() > 0)
                  {
                     defaultMember = _conn.GetFieldValue(linkString, workflow.DataBases.First().PrimaryKey, instanceId);
                  }
                  break;
               case 9://发起者主管
                  Guid firstSenderID = _task.GetFirstSnderID(workflow.ID, groupid.ToGuid());
                  if (firstSenderID.IsEmptyGuid() && currentStep.ID == workflow.FirstStepID)//如果是第一步则发起者为当前人员
                  {
                     firstSenderID = userId;
                  }
                  if (!firstSenderID.IsEmptyGuid())
                  {
                     defaultMember = _users.GetLeader(firstSenderID);
                  }
                  break;
               case 10://发起者分管领导
                  Guid firstSenderID1 = _task.GetFirstSnderID(workflow.ID, groupid.ToGuid());
                  if (firstSenderID1.IsEmptyGuid() && currentStep.ID == workflow.FirstStepID)//如果是第一步则发起者为当前人员
                  {
                     firstSenderID1 = userId;
                  }
                  if (!firstSenderID1.IsEmptyGuid())
                  {
                     defaultMember = _users.GetChargeLeader(firstSenderID1);
                  }
                  break;
               case 11://当前处理者主管
                  defaultMember = _users.GetLeader(userId);
                  break;
               case 12://当前处理者分管领导
                  defaultMember = _users.GetChargeLeader(userId);
                  break;
            }

            if (defaultMember.IsNullOrEmpty())
            {
               defaultMember = step.Behavior.DefaultHandler;
            }

            if (defaultMember.IsNullOrEmpty())
            {
               defaultMember = paras.DetaultMember;
            }

            stepUsers.Add(step.ID, defaultMember);

         }


         return new FlowRunningResult
         {
            RunModel = workflow,
            CurrentStep = currentStep,
            NextSteps = nextSteps,
            StepDefaultUsers = stepUsers
         };
      }


      public FlowRunningResult FlowExcute(RunParams paras)
      {
         if (paras == null)
            return null;

         this.CreateInstance();

         var result = new FlowRunningResult();
         var opationJSON = JsonMapper.ToObject(paras.JsonParams);
         var workflow = _flow.GetRunModel(paras.FlowId.ToGuid());
         var stepId = paras.Stepid;
         var userId = paras.UserId.ToGuid();
         var user = _users.Get(userId);
         var instanceid = paras.Instanceid;

         var execute = new Execute();
         execute.Comment = paras.Comment ?? string.Empty;

         var operation = opationJSON["type"].ToString().ToLower();
         _execute.SetExcuteType(execute, operation);

         execute.FlowID = paras.FlowId.ToGuid();
         execute.GroupID = paras.Groupid.ToGuid();
         execute.InstanceID = paras.Instanceid;
         execute.IsSign = "1" == paras.Issign;
         execute.Note = "";
         execute.Sender = user;
         execute.StepID = stepId.IsGuid() ? stepId.ToGuid() : workflow.FirstStepID;
         execute.TaskID = paras.Taskid.ToGuid();
         execute.Title = paras.Title;


         JsonData stepsjson = opationJSON["steps"];

         var dic = new Dictionary<string, string>();
         foreach (LitJson.JsonData step in stepsjson)
         {
            string id = step["id"].ToString();
            string member = step["member"].ToString();

            dic.Add(id, member);
         }
         _execute.SetSteps(execute, dic);


         var eventParams = new WorkFlowCustomEventParams();
         eventParams.FlowID = execute.FlowID;
         eventParams.GroupID = execute.GroupID;
         eventParams.StepID = execute.StepID;
         eventParams.TaskID = execute.TaskID;
         eventParams.InstanceID = execute.InstanceID;


         //保存表单数据
       paras.Instanceid= _flow.SaveFormData(instanceid,execute,eventParams);
 

         //步骤事件
         var steps = workflow.Steps.Where(p => p.ID == execute.StepID);
         _task.InvokeStepEvent(steps, execute,eventParams);


         //流程步骤中复杂的验证逻辑
         this.CheckComplexSteps(execute, workflow, _task, _users);


         //if (execute.Steps.Count<=0)
         //{
         //   result.ErrorMessage = "";//error message  

         //   return result;
         //}


         //处理委托
         _execute.ExcuteDelegation(execute);


         //处理任务
         result.TaskResult = _task.Execute(execute);

         return result;
      }


      public FlowRunningResult FlowBack(RunParams paras)
      {
         this.CreateInstance();

         var workflow = _flow.GetWorkFlowRunModel(paras.FlowId);

         if (workflow == null) { }

         var taskid = paras.Taskid;
         Guid taskID;
         if (!taskid.IsGuid(out taskID))
         {
            return new FlowRunningResult { ErrorMessage = "未找到当前任务" };
         }

         var currentStep = _flow.GetStep(workflow, paras.Stepid.ToGuid());
         if (currentStep == null)
         {
            return new FlowRunningResult { ErrorMessage = "没有找到当前步骤!" };
         }

         int backModel = currentStep.Behavior.BackModel;//退回策略
         if (backModel == 0)
         {
            return new FlowRunningResult { ErrorMessage = "当前步骤设置为不能退回!" };
         }

         int backType = currentStep.Behavior.BackType;//退回类型
         var prevSteps = _task.GetBackSteps(taskID, backType, currentStep.ID, workflow);


         return new FlowRunningResult
         {
            CurrentStep = currentStep,
            BackType = backType,
            PrevSteps = prevSteps
         };
      }


      private void CheckComplexSteps(Execute execute, WorkFlowInstalled wfInstalled, RunTask btask, RunUser busers)
      {
         var removeIDList = new List<Guid>();
         var steps = wfInstalled.Steps.Where(p => p.ID == execute.StepID);

         if (execute.Steps.Count() > 0 && execute.ExecuteType == EnumType.ExecuteType.Submit)
         {
            var nosubmitMsg = new StringBuilder();

            foreach (var step in execute.Steps)
            {
               var lines = wfInstalled.Lines.Where(p => p.ToID == step.Key && p.FromID == steps.First().ID);
               if (lines.Count() > 0)
               {
                  var line = lines.First();

                  #region  [组织机构关系判断]

                  if ("1" == line.Organize_SenderChargeLeader && !busers.IsChargeLeader(execute.Sender.ID))
                  {
                     removeIDList.Add(step.Key);
                  }
                  if (!line.Organize_SenderIn.IsNullOrEmpty() && !busers.IsContains(execute.Sender.ID, line.Organize_SenderIn))
                  {
                     removeIDList.Add(step.Key);
                  }
                  if ("1" == line.Organize_SenderLeader && !busers.IsLeader(execute.Sender.ID))
                  {
                     removeIDList.Add(step.Key);
                  }
                  if (!line.Organize_SenderNotIn.IsNullOrEmpty() && busers.IsContains(execute.Sender.ID, line.Organize_SenderNotIn))
                  {
                     removeIDList.Add(step.Key);
                  }
                  Guid sponserID = Guid.Empty;//发起者ID
                  if (execute.StepID == wfInstalled.FirstStepID)//如果是第一步则发起者就是发送者
                  {
                     sponserID = execute.Sender.ID;
                  }
                  else
                  {
                     sponserID = btask.GetFirstSnderID(execute.FlowID, execute.GroupID);
                  }
                  if ("1" == line.Organize_SponsorChargeLeader && !busers.IsChargeLeader(sponserID))
                  {
                     removeIDList.Add(step.Key);
                  }
                  if (!line.Organize_SponsorIn.IsNullOrEmpty() && !busers.IsContains(sponserID, line.Organize_SponsorIn))
                  {
                     removeIDList.Add(step.Key);
                  }
                  if ("1" == line.Organize_SponsorLeader && !busers.IsLeader(sponserID))
                  {
                     removeIDList.Add(step.Key);
                  }
                  if (!line.Organize_SponsorNotIn.IsNullOrEmpty() && !busers.IsContains(sponserID, line.Organize_SponsorNotIn))
                  {
                     removeIDList.Add(step.Key);
                  }
                  if ("1" == line.Organize_NotSenderChargeLeader && busers.IsChargeLeader(execute.Sender.ID))
                  {
                     removeIDList.Add(step.Key);
                  }
                  if ("1" == line.Organize_NotSenderLeader && busers.IsLeader(execute.Sender.ID))
                  {
                     removeIDList.Add(step.Key);
                  }
                  if ("1" == line.Organize_NotSponsorChargeLeader && busers.IsChargeLeader(sponserID))
                  {
                     removeIDList.Add(step.Key);
                  }
                  if ("1" == line.Organize_NotSponsorLeader && busers.IsLeader(sponserID))
                  {
                     removeIDList.Add(step.Key);
                  }

                  #endregion

               }
            }

            foreach (Guid rid in removeIDList)
            {
               execute.Steps.Remove(rid);
            }
         }
      }


      private void CreateInstance()
      {
         _flow = new RunWorkflow();
         _appLib = new RunAppLibrary();
         _task = new RunTask();
         _users = new RunUser();
         _conn = new RunDBConnection();
         _execute = new RunExcute();
         _comment = new RunWorkflowComment();
      }

    
   }

}
