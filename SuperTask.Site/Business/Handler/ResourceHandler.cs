using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Business
{

   public class ResourceSearchHandler : IHandler<APSqlSelectCommand, ResourceSearchOption>
   {

      public virtual void Handle(APSqlSelectCommand command, ResourceSearchOption option)
      {
         if (command == null || option == null) return;
      }

   }


   public class DefaultResourceSearchHandler : ResourceSearchHandler
   {

      protected APDBDef.UserInfoTableDef u = APDBDef.UserInfo;
      protected APDBDef.ResourceTableDef re = APDBDef.Resource;
      protected APDBDef.WorkTaskTableDef t = APDBDef.WorkTask;
      protected APDBDef.ProjectTableDef p = APDBDef.Project;

      public override void Handle(APSqlSelectCommand command, ResourceSearchOption option)
      {  
         base.Handle(command, option);

         command.where(re.Projectid == option.ProjectId)
                .order_by(re.CreateDate.Desc);
      }
   }


   public class ResourceSearchOption : SearchOption
   {

      public Guid SourceId { get; set; }
      public Guid ProjectId { get; set; }
      public Guid TaskId { get; set; }
      public Guid SearchType { get; set; }

   }


   public class ResourceEditHandler : IHandler<APSqlSelectCommand, ResourceEditOption>
   {

      public void Handle(APSqlSelectCommand t, ResourceEditOption v)
      {
         throw new NotImplementedException();
      }

   }


   public class ResourceEditOption : EditOption
   {

      public Guid SourceId { get; set; }
      public Guid ProjectId { get; set; }
      public Guid TaskId { get; set; }

   }

}