using Business.Helper;
using FluentScheduler;
using System;
using System.Web.Hosting;

namespace Business
{

   /// <summary>
   /// 日志生成器
   /// </summary>
   public class WorkJournalGenerator : Registry
   {
      public WorkJournalGenerator()
      {
         Schedule<GenerateJournalJob>().ToRunNow().AndEvery(1).Days().At(0, 1);
      }
   }


   /// <summary>
   /// 日志生成job
   /// </summary>
   public class GenerateJournalJob : IJob, IRegisteredObject
   {

      APDBDef _db;
      APDBDef.WorkJournalTableDef wj = APDBDef.WorkJournal;
      APDBDef.WorkTaskTableDef t = APDBDef.WorkTask;


      public GenerateJournalJob()
      {
         _db = new APDBDef();
      }


      public void Execute()
      {
         var exists = _db.WorkJournalDal.ConditionQueryCount(wj.CreateDate > DateTime.Now.TodayStart()) > 0;
         if (exists)
            return;

         var processTasks = _db.WorkTaskDal.ConditionQuery(t.TaskStatus == TaskKeys.ProcessStatus, null, null, null);


         _db.BeginTrans();

         try
         {
            processTasks.ForEach(tk => WorkJournalHelper.CreateByTask(tk,_db));


            _db.Commit();
         }
         catch
         {
            _db.Rollback();
         }
      }


      public void Stop(bool immediate)
      {
         throw new NotImplementedException();
      }

   }

}