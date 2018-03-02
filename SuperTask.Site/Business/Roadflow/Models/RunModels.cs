using RoadFlow.Data.Model;
using RoadFlow.Data.Model.WorkFlowInstalledSub;
using RoadFlow.Data.Model.WorkFlowInstalledSub.StepSet;
using Platform = RoadFlow.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoadFlow.Data.Model.WorkFlowExecute;

namespace Business.Roadflow
{

   public class RunParams
   {
      public string FlowId { get; set; }
      public string Instanceid { get; set; }
      public string Taskid { get; set; }
      public string Stepid { get; set; }
      public string Groupid { get; set; }
      public string Display { get; set; }
      public string UserId { get; set; }
      public string Comment { get; set; }
      public string Issign { get; set; }
      public string Title { get; set; }
      public string JsonParams { get; set; }

      //尝试用JSON传递 不同的实体
      public string ObjJson { get; set; }
      public string DetaultMember { get; set; }
      public string FlowType { get; set; }
   }


   internal class RunWorkflow: Platform.WorkFlow
   {

      public WorkFlowInstalled GetRunModel(Guid flowId)
      {
         return this.GetWorkFlowRunModel(flowId);
      }


      public Step GetStep(WorkFlowInstalled workflow, Guid stepId)
      {
         if (workflow == null)
            return null;

         if (stepId == Guid.Empty)
            stepId = workflow.FirstStepID;

         var currentStep = workflow.Steps.ToList().Find(p => p.ID == stepId);

         return currentStep;
      }


      public bool CheckFlow(WorkFlowInstalled workflow,out string error)
      {
         if(workflow == null)
         {
            error = "未找到流程运行时!";
            return false;
         }
         else if (workflow.Status == 3)
         {
            error = "未找到流程运行时!";
            return false;
         }
         else if (workflow.Status == 4)
         {
            error = "该流程已被删除,不能发起新的实例!";
            return false;
         }

         error = string.Empty;

         return true;

      }


      public virtual string SaveFormData(string instanceid, Execute execute, WorkFlowCustomEventParams eventParams)
      {
         //保存业务数据
         if (execute.ExecuteType == EnumType.ExecuteType.Save ||
             execute.ExecuteType == EnumType.ExecuteType.Submit ||
             execute.ExecuteType == EnumType.ExecuteType.Completed)
         {
            instanceid = SaveFromData(instanceid, eventParams);
            if (execute.InstanceID.IsNullOrEmpty())
            {
               execute.InstanceID = instanceid;
               eventParams.InstanceID = instanceid;
            }
         }

         return execute.InstanceID;
      }
   }


   internal class RunTask : Platform.WorkFlowTask
   {

      public virtual void UpdateOpenTime(Guid taskId)
      {
         var task = new Platform.WorkFlowTask();
         if (taskId != Guid.Empty)
         {
            task.UpdateOpenTime(taskId, RoadFlow.Utility.DateTimeNew.Now, true);
         }
      }


      public virtual void InvokeStepEvent(IEnumerable<Step> steps, Execute execute, WorkFlowCustomEventParams eventParams)
      {
         foreach (var step in steps)
         {
            //步骤提交前事件
            if (!step.Event.SubmitBefore.IsNullOrEmpty() && execute.ExecuteType == RoadFlow.Data.Model.WorkFlowExecute.EnumType.ExecuteType.Submit)
            {
               object obj = ExecuteFlowCustomEvent(step.Event.SubmitBefore.Trim(), eventParams);
            }
            //步骤退回前事件
            if (!step.Event.BackBefore.IsNullOrEmpty() && execute.ExecuteType == RoadFlow.Data.Model.WorkFlowExecute.EnumType.ExecuteType.Back)
            {
               object obj = ExecuteFlowCustomEvent(step.Event.BackBefore.Trim(), eventParams);
            }
         }
      }

   }


   internal class RunAppLibrary : Platform.AppLibrary
   {

      public AppLibrary GetByFormId(Guid formId)
      {
         return Get(formId, true);
      }

   }


   internal class RunOrganize:Platform.Organize
   {

   }


   internal class RunUser :Platform.Users
   {
   }


   internal class RunWorkflowComment: Platform.WorkFlowComment
   {

   }


   internal class RunDelegation:Platform.WorkFlowDelegation
   {
   }


   internal class RunExcute
   {

      internal virtual void SetExcuteType(Execute execute,string operation)
      {
         switch (operation)
         {
            case "submit":
               execute.ExecuteType = RoadFlow.Data.Model.WorkFlowExecute.EnumType.ExecuteType.Submit;
               break;
            case "save":
               execute.ExecuteType = RoadFlow.Data.Model.WorkFlowExecute.EnumType.ExecuteType.Save;
               break;
            case "back":
               execute.ExecuteType = RoadFlow.Data.Model.WorkFlowExecute.EnumType.ExecuteType.Back;
               break;
            case "completed":
               execute.ExecuteType = RoadFlow.Data.Model.WorkFlowExecute.EnumType.ExecuteType.Completed;
               break;
            case "redirect":
               execute.ExecuteType = RoadFlow.Data.Model.WorkFlowExecute.EnumType.ExecuteType.Redirect;
               break;
         }

      }


      internal virtual void SetSteps(Execute execute,Dictionary<string,string> stepNames)
      {
         var runOrganize= new RunOrganize();

         foreach (var key in stepNames.Keys)
         {
            Guid gid;
            var member = stepNames[key];

            if (key.IsGuid(out gid))
            {
               switch (execute.ExecuteType)
               {
                  case EnumType.ExecuteType.Submit:
                     execute.Steps.Add(gid, runOrganize.GetAllUsers(RunUser.PREFIX+member));
                     break;
                  case EnumType.ExecuteType.Back:
                     execute.Steps.Add(gid, new List<RoadFlow.Data.Model.Users>());
                     break;
                  case EnumType.ExecuteType.Save:
                     break;
                  case EnumType.ExecuteType.Completed:
                     break;
                  case EnumType.ExecuteType.Redirect:
                     break;
               }
            }

            if (execute.ExecuteType == EnumType.ExecuteType.Redirect)
            {
               execute.Steps.Add(Guid.Empty, new RunOrganize().GetAllUsers(member));
            }
         }

      }


      internal virtual void ExcuteDelegation(Execute execute)
      {
         var delegation = new RunDelegation();
         var busers = new RunUser();

         //处理委托
         foreach (var executeStep in execute.Steps)
         {
            for (int i = 0; i < executeStep.Value.Count; i++)
            {
               Guid newUserID = delegation.GetFlowDelegationByUserID(execute.FlowID, executeStep.Value[i].ID);
               if (newUserID != Guid.Empty && newUserID != executeStep.Value[i].ID)
               {
                  executeStep.Value[i] = busers.Get(newUserID);
               }
            }
         }
      }

   }


   internal class RunDBConnection :Platform.DBConnection
   {

   }


   internal class RunDictionary : Platform.Dictionary
   {

   }


   public class FlowRunningResult
   {

      public WorkFlowInstalled RunModel { get; set; }

      public Step CurrentStep { get; set; }

      public List<Step> NextSteps { get; set; }

      public AppLibrary CurrentLibrary { get; set; }

      public WorkFlowTask CurrentTask { get; set; }

      public bool IsSign { get; set; }

      public int SignatureType { get; set; }

      public string ErrorMessage { get; set; }

      public IDictionary<Guid, string> StepDefaultUsers { get; set; }

      public string Comment { get; set; }

      public Result TaskResult { get; set; }

      public Dictionary<Guid,string> PrevSteps { get; set; }

      public int BackType { get; set; }

   }


}
