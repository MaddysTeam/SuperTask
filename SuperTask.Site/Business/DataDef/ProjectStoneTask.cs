using Business.Helper;
using System;
using TheSite.Models;

namespace Business
{

   public partial class ProjectStoneTask
   {

      public Attachment CurrentAttachment { get; set; }

      public Result Valiedate()
      {
         if (ProjectId.IsEmpty())
         {
            return new Result { IsSuccess = false, Msg = Errors.Payments.EDIT_FAIL };
         }

         return new Result { IsSuccess = true};
      }

      public void SetStatus(Guid status)
      {
         TaskStatus = status;
      }

      public virtual void Complete(DateTime realEndDate)
      {
         RealEndDate = realEndDate;
         SetStatus(TaskKeys.CompleteStatus);
      }

      public bool IsTempEditStatus => TaskStatus == TaskKeys.TaskTempEditStatus;

      //public bool IsProjectType => this.PayType == PaymentsKeys.ProjectPaymentsType; 
      //public bool IsInternalVenderType => this.PayType == PaymentsKeys.InternalVenderPaymentsType;
   }

}