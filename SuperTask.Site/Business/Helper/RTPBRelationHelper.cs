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
      static APDBDef.WorkTaskTableDef t = APDBDef.WorkTask;
      static APDBDef.BugTableDef b = APDBDef.Bug;
      static APDBDef.RequireTableDef r = APDBDef.Require;
      static APDBDef.PublishTableDef p = APDBDef.Publish;

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


      public static bool BindRelationBetweenPublishAndTasks(Guid[] publishIds, Guid taskId, APDBDef db)
      {
         if (publishIds == null || taskId.IsEmpty())
            return false;

         db.RTPBRelationDal.ConditionDelete(rtpb.TaskId == taskId & rtpb.TypeId == RTPBRelationKeys.TaskWithPublish);

         foreach (var publishId in publishIds)
         {
            if (!publishId.IsEmpty())
               db.RTPBRelationDal.Insert(new RTPBRelation() { PublishId = publishId, TaskId = taskId, TypeId = RTPBRelationKeys.TaskWithPublish });
         }

         return true;
      }


      public static bool BindRelationBetweenRequiresAndTask(Guid[] requiredIds, Guid taskId, APDBDef db)
		{
			if (requiredIds == null || taskId.IsEmpty())
				return false;

			db.RTPBRelationDal.ConditionDelete(rtpb.TaskId == taskId & rtpb.TypeId == RTPBRelationKeys.TaskWithRequire);

			foreach (var requireId in requiredIds)
			{
				if (!requireId.IsEmpty())
					db.RTPBRelationDal.Insert(new RTPBRelation() { TaskId = taskId, RequireId=requireId, TypeId = RTPBRelationKeys.TaskWithRequire });
			}

			return true;
		}


		public static bool BindRelationBetweenBugsAndTask(Guid[] bugIds, Guid taskId, APDBDef db)
		{
			if (bugIds == null || taskId.IsEmpty())
				return false;

			db.RTPBRelationDal.ConditionDelete(rtpb.TaskId == taskId & rtpb.TypeId == RTPBRelationKeys.TaskWithBug);

			foreach (var bugId in bugIds)
			{
				if (!bugId.IsEmpty())
					db.RTPBRelationDal.Insert(new RTPBRelation() { TaskId = taskId, BugId = bugId, TypeId = RTPBRelationKeys.TaskWithBug });
			}

			return true;
		}

      public static bool BindRelationBetweenTasksAndBug(Guid[] taskIds, Guid bugId, APDBDef db)
      {
         if (taskIds == null || bugId.IsEmpty())
            return false;

         db.RTPBRelationDal.ConditionDelete(rtpb.BugId == bugId & rtpb.TypeId == RTPBRelationKeys.TaskWithBug);

         foreach (var taskId in taskIds)
         {
            if (!taskId.IsEmpty())
               db.RTPBRelationDal.Insert(new RTPBRelation() { TaskId = taskId, BugId = bugId, TypeId = RTPBRelationKeys.TaskWithBug });
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

      public static List<Bug> GetTaskRelativeBugs(Guid taskId, APDBDef db)
      {
         if (taskId.IsEmpty())
            return new List<Bug>();

         var subQuery = APQuery.select(rtpb.BugId).from(rtpb).where(rtpb.TaskId == taskId & rtpb.TypeId == RTPBRelationKeys.TaskWithBug);
         return db.BugDal.ConditionQuery(b.BugId.In(subQuery), null, null, null);
      }


      public static List<WorkTask> GetPublishRelativeTasks(Guid publishId, APDBDef db)
		{
			if (publishId.IsEmpty())
				return new List<WorkTask>();

			var subQuery = APQuery.select(rtpb.TaskId).from(rtpb).where(rtpb.PublishId == publishId & rtpb.TypeId == RTPBRelationKeys.TaskWithPublish);
			return db.WorkTaskDal.ConditionQuery(t.TaskId.In(subQuery), null, null, null);
		}

      public static List<Publish> GetTaskRelativePublishs(Guid taskId, APDBDef db)
      {
         if (taskId.IsEmpty())
            return new List<Publish>();

         var subQuery = APQuery.select(rtpb.PublishId).from(rtpb).where(rtpb.TaskId == taskId & rtpb.TypeId == RTPBRelationKeys.TaskWithPublish);
         return db.PublishDal.ConditionQuery(p.PublishId.In(subQuery), null, null, null);
      }


      public static List<WorkTask> GetRequireRelativeTasks(Guid required, APDBDef db)
		{
			if (required.IsEmpty())
				return new List<WorkTask>();

			APDBDef.WorkTaskTableDef t = APDBDef.WorkTask;
			var subQuery = APQuery.select(rtpb.TaskId).from(rtpb).where(rtpb.RequireId == required & rtpb.TypeId == RTPBRelationKeys.TaskWithRequire);
			return db.WorkTaskDal.ConditionQuery(t.TaskId.In(subQuery), null, null, null);
		}


		public static List<Require> GetTaskRelativeRequires(Guid taskId, APDBDef db)
		{
			if (taskId.IsEmpty())
				return new List<Require>();

			APDBDef.RequireTableDef r = APDBDef.Require;
			var subQuery = APQuery.select(rtpb.RequireId).from(rtpb).where(rtpb.TaskId == taskId & rtpb.TypeId == RTPBRelationKeys.TaskWithRequire);
			return db.RequireDal.ConditionQuery(r.RequireId.In(subQuery), null, null, null);
		}


      public static string GetTaskIds(List<WorkTask> tasks)
      {
         if (tasks.Count > 0)
            return string.Join(",", tasks.Select(x => x.TaskId.ToString()));

         return string.Empty;
      }

      public static string GetRequireIds(List<Require> requires)
		{
			if (requires.Count > 0)
				return string.Join(",", requires.Select(x => x.RequireId.ToString()));

			return string.Empty;
		}

      public static string GetBugIds(List<Bug> bugs)
      {
         if (bugs.Count > 0)
            return string.Join(",", bugs.Select(x => x.BugId.ToString()));

         return string.Empty;
      }

      public static string GetPublishIds(List<Publish> publishs)
      {
         if (publishs.Count > 0)
            return string.Join(",", publishs.Select(x => x.PublishId.ToString()));

         return string.Empty;
      }

   }

}