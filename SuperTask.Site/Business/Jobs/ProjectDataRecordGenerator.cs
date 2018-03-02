using FluentScheduler;
using System;
using System.Web.Hosting;

namespace Business
{

   /// <summary>
   /// 项目属性数据记录生成器
   /// </summary>
   public class ProjectDataRecordGenerator : Registry
   {
      public ProjectDataRecordGenerator()
      {
         Schedule<GenerateProjectDataJob>().ToRunNow().AndEvery(1).Days().At(0, 1);
      }
   }


   /// <summary>
   /// 项目属性数据生成job
   /// </summary>
   public class GenerateProjectDataJob : IJob, IRegisteredObject
   {

      APDBDef _db;
      //APDBDef.WorkJournalTableDef wj = APDBDef.WorkJournal;
      //APDBDef.WorkTaskTableDef t = APDBDef.WorkTask;


      public GenerateProjectDataJob()
      {
         _db = new APDBDef();
      }


      public void Execute()
      {
         //var exists = _db.WorkJournalDal.ConditionQueryCount(wj.CreateDate > DateTime.Now.TodayStart()) > 0;
         //if (exists)
         //   return;

         //var processTasks = _db.WorkTaskDal.ConditionQuery(t.TaskStatus == TaskKeys.ProcessStatus, null, null, null);


         //_db.BeginTrans();

         //try
         //{
         //   processTasks.ForEach(tk => _db.WorkJournalDal.Insert(new WorkJournal
         //   {
         //      JournalId = Guid.NewGuid(),
         //      Projectid = tk.Projectid,
         //      TaskId = tk.TaskId,
         //      UserId = tk.ManagerId,
         //      CreateDate = DateTime.Now,
         //      Status = JournalKeys.SaveStatus,
         //      RecordDate = DateTime.Now,
         //      WorkHours = 0,
         //      Progress = tk.RateOfProgress,
         //      RecordType = tk.IsParent ? JournalKeys.AutoRecordType : JournalKeys.ManuRecordType
         //   }));


         //   _db.Commit();
         //}
         //catch
         //{
         //   _db.Rollback();
         //}
      }


      public void Stop(bool immediate)
      {
         throw new NotImplementedException();
      }

   }

}