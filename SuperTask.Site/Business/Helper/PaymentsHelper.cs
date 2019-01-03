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

      public static void AddDefaultPayments(Guid projectId, APDBDef db)
      {
         var payments = new List<Payments> {
            //项目款项
            new Payments(Guid.NewGuid(),PaymentsKeys.FirstPayment,projectId,0,0,DateTime.MinValue,DateTime.MinValue,PaymentsKeys.ProjectPaymentsType,Guid.Empty),
            new Payments(Guid.NewGuid(),PaymentsKeys.MiddlePayment,projectId,0,0,DateTime.MinValue,DateTime.MinValue,PaymentsKeys.ProjectPaymentsType,Guid.Empty),
            new Payments(Guid.NewGuid(),PaymentsKeys.TailPayment,projectId,0,0,DateTime.MinValue,DateTime.MinValue,PaymentsKeys.ProjectPaymentsType,Guid.Empty),

            // 外包款项
            new Payments(Guid.NewGuid(),string.Empty,projectId,0,0,DateTime.MinValue,DateTime.MinValue,PaymentsKeys.InternalVenderPaymentsType,Guid.Empty),
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
