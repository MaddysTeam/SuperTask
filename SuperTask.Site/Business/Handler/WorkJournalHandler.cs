using Business.Helper;
using Symber.Web.Data;
using System;
using System.Collections.Generic;
using TheSite.Models;

namespace Business
{

   public class WorkJournalEditHandler : IHandler<WorkJournal, WorkJournalEditOption>
   {

      public virtual void Handle(WorkJournal journal, WorkJournalEditOption option)
      {
         ValidateJournal(journal, option);
         if (!option.Result.IsSuccess)
            return;

         var db = option.db;

         journal.SetStatus(JournalKeys.RecordedStatus);

         if (journal.JournalId.IsEmpty())
         {
            journal.CreateDate = DateTime.Now;

            db.WorkJournalDal.Insert(journal);

            option.Result.IsSuccess = true;
            option.Result.Msg = Success.WorkJournal.EDIT_SUCCESS;
         }
         else
         {
            db.BeginTrans();


            try
            {
               journal.ModifyDate = DateTime.Now;

               //插入附件数据
               if (journal.HasUploadFile())
               {
                  UpLoadFileByJournal(journal, option);
               }

			   //TODO: 子父节点工时不在关联
              // DoSomtingBeforeEditing(journal, option);

               db.WorkJournalDal.Update(journal);

               TaskLogHelper.CreateLogs(option.Tasks, option.OriginalTasks, journal.UserId, db);

               option.Result.IsSuccess = true;
               option.Result.Msg = Success.WorkJournal.EDIT_SUCCESS;


               db.Commit();
            }
            catch
            {
               db.Rollback();

               option.Result.IsSuccess = false;
               option.Result.Msg = Errors.WorkJournal.EDIT_FAILED;
            }
         }
      }


      protected virtual void ValidateJournal(WorkJournal journal, WorkJournalEditOption option)
      {
         option.Result = journal.Validate();
      }

      protected virtual void UpLoadFileByJournal(WorkJournal journal, WorkJournalEditOption option)
      {
         var att = journal.Attachment;
         var db = option.db;
         var existAtt = db.AttachmentDal.PrimaryGet(att.AttachmentId);

         if (existAtt == null)
         {
            att.AttachmentId = Guid.NewGuid();
            att.Projectid = journal.Projectid;
            att.ItemId = journal.TaskId;
            att.PublishUserId = journal.UserId;
            att.UploadDate = DateTime.Now;

            db.AttachmentDal.Insert(att);
         }
         else
         {
            existAtt.RealName = att.RealName;
            existAtt.Url = att.Url;
            db.AttachmentDal.Update(existAtt);
         }

         journal.AttachmentId = att.AttachmentId;
      }

      /// <summary>
      /// TODO:这个名字有点怪，还没想好，主要是 Task 和 Journal 层级间的一些逻辑，例如任务层级工时的改变，和日志层级工时的改变
      /// </summary>
      protected virtual void DoSomtingBeforeEditing(WorkJournal journal, WorkJournalEditOption option)
      {
         var db = option.db;
         var original = option.Journals.Find(j => j.JournalId == journal.JournalId);
         var tks = option.Tasks;
         var parentJournals = new List<WorkJournal>();

         var tk = tks.Find(task => task.TaskId == journal.TaskId);
         if (tk != null)
         {
            //当前任务实际进度（覆盖）
            tk.RateOfProgress = journal.Progress;
            //累计当前任务实际工时（累加）
            tk.WorkHours = GetCurrentValue(tk.WorkHours, original.WorkHours, journal.WorkHours);
            //累计任务子类型的工作数量 （累加）
            tk.SubTypeValue = GetCurrentValue(tk.SubTypeValue, original.TaskSubTypeValue, journal.TaskSubTypeValue);

            //修改其所有影响父任务的实际工时(累加)
            while (!tk.ParentId.IsEmpty())
            {
               tk = tks.Find(task => task.TaskId == tk.ParentId);
               tk.WorkHours = GetCurrentValue(tk.WorkHours, original.WorkHours, journal.WorkHours);

               //修改父任务日志
               var parentJournal = option.Journals.Find(j => j.TaskId == tk.TaskId
                                                   && j.RecordDate.ToString("yyyy-MM-dd") == journal.RecordDate.ToString("yyyy-MM-dd"));
               if (parentJournal != null)
               {
                  parentJournal.WorkHours = GetCurrentValue(parentJournal.WorkHours, original.WorkHours, journal.WorkHours);
               }
               else //如果父任务工时不存在，则新建补齐，注意有实际工时的任务，不能在不同的父节点移动，所以不存在该问题
               {
                  parentJournal = WorkJournalHelper.CreateByTask(tk, db);
                  parentJournal.WorkHours = journal.WorkHours;
                  parentJournal.RecordDate = journal.RecordDate;
               }

               parentJournal.ModifyDate = DateTime.Now;
               parentJournals.Add(parentJournal);
            }
         }

         tks.ForEach(task => db.WorkTaskDal.UpdatePartial(task.TaskId, new {
            RateOfProgress = task.RateOfProgress,
            WorkHours = task.WorkHours,
            SubTypeValue =task.SubTypeValue,
            AttachmentId = journal.AttachmentId,
         }));

         parentJournals.ForEach(j => db.WorkJournalDal.Update(j));
      }

      protected double GetCurrentValue(double val, double orignal, double addVal)
      {
         if (val <= 0) return addVal;

         return val - orignal + addVal;
      }

      protected int GetCurrentValue(int val, int orignal, int addVal)
      {
         if (val <= 0) return addVal;

         return val - orignal + addVal;
      }

   }

   public class WorkJournalEditOption : EditOption
   {
      public APDBDef db { get; set; }

      public Result Result { get; set; }

      public List<WorkTask> Tasks { get; set; }

      public List<WorkTask> OriginalTasks { get; set; }

      public List<WorkJournal> Journals { get; set; }
   }

}
