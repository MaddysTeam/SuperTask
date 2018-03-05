using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using TheSite.Models;

namespace Business.Helper
{

   public class ReviewHelper
   {

      /// <summary>
      ///  找其任务提交审核的第一条记录
      /// </summary>
      /// <param name="db"></param>
      /// <param name="instanceId"></param>
      /// <returns></returns>
      public static Review GetEarlistReview(APDBDef db,Guid instanceId)
      {
         var rev = APDBDef.Review;

         var subQuery = APQuery.select(rev.TaskId).from(rev).where(rev.ReviewId == instanceId);
         var review = APQuery.select(rev.Asterisk)
            .from(rev)
            .where(rev.TaskId.In(subQuery))
            .order_by(rev.SendDate.Asc)
            .query(db, r =>
            {
               var rv = new Review();
               rev.Fullup(r, rv, false);

               return rv;
            }).FirstOrDefault();

         return review;
      }

   }

}