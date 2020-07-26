using business.helper;
using RoadFlow.Data.MSSQL;
using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheSite.Models;

namespace Business.Helper
{

   public class RTPBRelationHelper
   {

      static APDBDef.RTPBRelationTableDef rtpb = APDBDef.RTPBRelation;


      /// <summary>
      /// bind publish and tasks by one to many
      /// </summary>
      /// <param name="taskIds">task id array</param>
      /// <param name="publishId">publish id</param>
      /// <param name="db">APDBDef</param>
      /// <returns></returns>
      public static bool BindRelationBetweenTasksAndPublish(Guid[] taskIds, Guid publishId, APDBDef db)
      {
         if (taskIds == null || publishId.IsEmpty())
            return false;

         db.RTPBRelationDal.ConditionDelete(rtpb.PublishId == publishId & rtpb.TypeId == RTPBRelationKeys.TaskWithPublish);

         foreach (var taskId in taskIds)
         {
            if (!taskId.IsEmpty())
               db.RTPBRelationDal.Insert(new RTPBRelation() { PublishId = publishId, TaskId = taskId, TypeId = RTPBRelationKeys.TaskWithPublish });
         }

         return true;
      }


      public static List<WorkTask> GetBugRelativeTasks(Guid bugId, APDBDef db)
      {
         if (bugId.IsEmpty())
            return new List<WorkTask>();

         APDBDef.WorkTaskTableDef t = APDBDef.WorkTask;
         var subQuery = APQuery.select(rtpb.TaskId).from(rtpb).where(rtpb.BugId == bugId & rtpb.TypeId == RTPBRelationKeys.TaskWithBug);
         return db.WorkTaskDal.ConditionQuery(t.TaskId.In(subQuery), null, null, null);
      }


      public static List<WorkTask> GetPublishRelativeTasks(Guid publishId, APDBDef db)
      {
         if (publishId.IsEmpty())
            return new List<WorkTask>();

         APDBDef.WorkTaskTableDef t = APDBDef.WorkTask;
         var subQuery = APQuery.select(rtpb.TaskId).from(rtpb).where(rtpb.PublishId == publishId & rtpb.TypeId == RTPBRelationKeys.TaskWithPublish);
         return db.WorkTaskDal.ConditionQuery(t.TaskId.In(subQuery), null, null, null);
      }

   }

}