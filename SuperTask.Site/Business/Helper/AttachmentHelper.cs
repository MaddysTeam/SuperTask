using Business.Config;
using Symber.Web.Report;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;

namespace Business.Helper
{

	public static class AttachmentHelper
	{

		public static void UploadTaskAttachment(WorkTask task, APDBDef db)
		{
			var attachment = task.CurrentAttachment;
			if (attachment != null && !string.IsNullOrEmpty(attachment.Url))
			{
				attachment.TaskId = task.TaskId;
				attachment.Projectid = task.Projectid;
				attachment.PublishUserId = task.ManagerId;
				attachment.UploadDate = DateTime.Now;
				attachment.AttachmentId = Guid.NewGuid();

				db.AttachmentDal.Insert(attachment);
			}
		}


		public static IList<Attachment> GetAttachments(Guid projectId, Guid taskId, APDBDef db)
		{
			var a = APDBDef.Attachment;
			return db.AttachmentDal.ConditionQuery(a.Projectid == projectId & a.TaskId == taskId, null, null, null);
		}


	}

}

