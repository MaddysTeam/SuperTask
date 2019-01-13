using Business.Helper;
using System;
using TheSite.Models;

namespace Business
{

   public partial class Payments
   {
      public Result Valiedate()
      {
         if (ProjectId.IsEmpty())
         {
            return new Result { IsSuccess = false, Msg = Errors.Payments.EDIT_FAIL };
         }

         return new Result { IsSuccess = true};
      }

      public bool IsProjectType => this.PayType == PaymentsKeys.ProjectPaymentsType; 
      public bool IsInternalVenderType => this.PayType == PaymentsKeys.InternalVenderPaymentsType;
   }

}