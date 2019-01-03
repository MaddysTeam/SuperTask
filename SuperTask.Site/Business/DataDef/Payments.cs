using Business.Helper;
using System;

namespace Business
{

   public partial class Payments
   {

      public bool IsProjectType => this.PayType == PaymentsKeys.ProjectPaymentsType;
      public bool IsInternalVenderType => this.PayType == PaymentsKeys.InternalVenderPaymentsType;
   }

}