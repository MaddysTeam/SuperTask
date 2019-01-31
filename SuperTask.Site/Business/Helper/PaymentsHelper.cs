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

      /// <summary>
      ///  新增默认款项
      /// </summary>
      /// <param name="project"></param>
      /// <param name="db"></param>
      public static void AddDefaultPayments(Project project, APDBDef db)
      {
         var projectId = project.ProjectId;
         var start = project.StartDate;
         var end = project.EndDate;
         var payments = new List<Payments> {
            //项目款项
            new Payments(Guid.NewGuid(),PaymentsKeys.FirstPayment,projectId,0,0,start,end,PaymentsKeys.ProjectPaymentsType,Guid.Empty,1,false,Guid.Empty),
            new Payments(Guid.NewGuid(),PaymentsKeys.MiddlePayment,projectId,0,0,start,end,PaymentsKeys.ProjectPaymentsType,Guid.Empty,2,false,Guid.Empty),
            new Payments(Guid.NewGuid(),PaymentsKeys.TailPayment,projectId,0,0,start,end,PaymentsKeys.ProjectPaymentsType,Guid.Empty,3,false,Guid.Empty),

            // 外包款项
            new Payments(Guid.NewGuid(),string.Empty,projectId,0,0,start,end,PaymentsKeys.InternalVenderPaymentsType,Guid.Empty,1,false,Guid.Empty),

            // 验收款项
            new Payments(Guid.NewGuid(),"是否有履约保证金",projectId,0,0,start,end,PaymentsKeys.CheckBeforeDeliveryType,PaymentsKeys.AppointGuaranteeResourceId,1,false,Guid.Empty),
            new Payments(Guid.NewGuid(),"是否有质量保证金",projectId,0,0,start,end,PaymentsKeys.CheckBeforeDeliveryType,PaymentsKeys.QualityGuaranteeResourceId,2,false,Guid.Empty),
         };

         payments.ForEach(p =>
         {
            db.PaymentsDal.Insert(p);
         });
      }


      /// <summary>
      /// 得到项目下所有的款项，同时计算百分比
      /// </summary>
      /// <param name="projectId"></param>
      /// <param name="db"></param>
      /// <returns></returns>
      public static List<Payments> GetProjectPayments(Guid projectId, APDBDef db)
      {
         var project = db.ProjectDal.PrimaryGet(projectId);
         var money = project.Money;

         var result = db.PaymentsDal.ConditionQuery(p.ProjectId == projectId, null, null, null);
         foreach (var item in result)
         {
            double r = money <= 0 ? 0 : (double)(item.Money / money).Round(2);
            item.Ratio = r.ToString("P");
         }

         return result.Count > 0 ? result.OrderBy(x => x.Sort).ToList() : result;
      }

   }

}
