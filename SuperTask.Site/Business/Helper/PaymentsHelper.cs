using Business;
using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Business.Helper
{

   public static class PaymentsHelper
   {

      static APDBDef.PaymentsTableDef p = APDBDef.Payments;

      public static void AddDefaultPayments(Project project, APDBDef db)
      {
         var projectId = project.ProjectId;
         var start = project.StartDate;
         var end = project.EndDate;
         var payments = new List<Payments> {
            //项目款项
            new Payments(Guid.NewGuid(),PaymentsKeys.FirstPayment,projectId,0,0,start,end,PaymentsKeys.ProjectPaymentsType,Guid.Empty),
            new Payments(Guid.NewGuid(),PaymentsKeys.MiddlePayment,projectId,0,0,start,end,PaymentsKeys.ProjectPaymentsType,Guid.Empty),
            new Payments(Guid.NewGuid(),PaymentsKeys.TailPayment,projectId,0,0,start,end,PaymentsKeys.ProjectPaymentsType,Guid.Empty),

            // 外包款项
            new Payments(Guid.NewGuid(),string.Empty,projectId,0,0,start,end,PaymentsKeys.InternalVenderPaymentsType,Guid.Empty),

            // 验收款项
            new Payments(Guid.NewGuid(),"是否有履约保证金",projectId,0,0,start,end,PaymentsKeys.CheckBeforeDeliveryType,PaymentsKeys.AppointGuaranteeResourceId),
            new Payments(Guid.NewGuid(),"是否有质量保证金",projectId,0,0,start,end,PaymentsKeys.CheckBeforeDeliveryType,PaymentsKeys.QualityGuaranteeResourceId),
         };

         payments.ForEach(p =>
         {
            db.PaymentsDal.Insert(p);
         });
      }


      public static List<Payments> GetProjectPayments(Guid projectId, APDBDef db)
      {
         return db.PaymentsDal.ConditionQuery(p.ProjectId == projectId, null, null, null);
      }

   }

}
