using Business.Helper;
using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business
{

   public interface IHandler<T, V>
   {
      void Handle(T t, V v);
   }


   public class SearchOption
   {
      public bool ReturnJson { get; set; }

      public string ViewName { get; set; }

      public DateTime StartDate { get; set; }

      public DateTime EndDate { get; set; }
   }


   public class EditOption
   {

   }


   public static class HandleManager
   {

      public static Dictionary<Guid, ProjectEditHandler> ProjectEditHandlers = new Dictionary<Guid, ProjectEditHandler>
      {
         {ProjectKeys.DefaultProjectType,new ProjectEditHandler() }
      };

      public static Dictionary<Guid, ProjectTemplateEditHandler> ProjectTemplateEditHandlers = new Dictionary<Guid, ProjectTemplateEditHandler>
      {
          {ProjectKeys.DevelopmentProjectType,new DevelopmentProjectTemplateEditHandler() }
      };

      public static Dictionary<Guid, DefaultTaskSearchHandler> TaskSearchHandlers = new Dictionary<Guid, DefaultTaskSearchHandler>
      {
         {TaskKeys.SearchByDetaultType,new TaskSearchHandler() },
         {TaskKeys.SearchByProject, new ProjectTaskSearchHandler()  },
         {TaskKeys.SearchByPersonal, new PersonalTaskSearchHandler()  }
      };

      public static Dictionary<Guid, TaskEditHandler> TaskEditHandlers = new Dictionary<Guid, TaskEditHandler>
      {
         {TaskKeys.DefaultType, new TaskEditHandler()  },
         {TaskKeys.ProjectTaskType, new ProjectTaskEditHandler()  },
         {TaskKeys.PlanTaskTaskType, new LeafTaskEditHandler(new ProjectTaskEditHandler())  },
         {TaskKeys.DocumentTaskType, new LeafTaskEditHandler(new ProjectTaskEditHandler())  },
         {TaskKeys.DesignTaskType,new LeafTaskEditHandler(new ProjectTaskEditHandler()) },
         {TaskKeys.DevelopTaskType,new LeafTaskEditHandler(new ProjectTaskEditHandler()) },
         {TaskKeys.TestTaskType,new LeafTaskEditHandler(new ProjectTaskEditHandler()) },
         {TaskKeys.DeployTaskType,new LeafTaskEditHandler(new ProjectTaskEditHandler()) },
         {TaskKeys.MaintainedTaskType, new LeafTaskEditHandler(new ProjectTaskEditHandler())  },
         {TaskKeys.ManageTaskType,new LeafTaskEditHandler(new ProjectTaskEditHandler())  },
         {TaskKeys.OfficeTaskType,new LeafTaskEditHandler(new ProjectTaskEditHandler())  },

         {TaskKeys.TempTaskType, new TempTaskEditHandler()  },
      };

      public static Dictionary<Guid, ResourceSearchHandler> ResourceSearchHandlers = new Dictionary<Guid, ResourceSearchHandler>
      {
         {ResourceKeys.DefaultSearchType, new DefaultResourceSearchHandler()  },
      };

      public static Dictionary<Guid, TaskReviewRequestHandler> TaskReviewRequestHandlers = new Dictionary<Guid, TaskReviewRequestHandler>
      {
         {ReviewKeys.ReviewTypeForTkChanged, new TaskEditRequestHandler()  },
         {ReviewKeys.ReviewTypeForTkSubmit, new TaskSubmitRequestHandler()  },
      };

      public static Dictionary<Guid, WorkJournalEditHandler> JournalEditHandlers = new Dictionary<Guid, WorkJournalEditHandler>
      {
         {TaskKeys.ProjectTaskType, new WorkJournalEditHandler()  },
         {TaskKeys.PlanTaskTaskType, new WorkJournalEditHandler()  },
         {TaskKeys.TempTaskType, new WorkJournalEditHandler()  },
         {TaskKeys.DocumentTaskType, new WorkJournalEditHandler()  },
         {TaskKeys.DesignTaskType, new WorkJournalEditHandler()  },
         {TaskKeys.DevelopTaskType, new WorkJournalEditHandler()  },
         {TaskKeys.TestTaskType, new WorkJournalEditHandler()  },
         {TaskKeys.DeployTaskType, new WorkJournalEditHandler()  },
         {TaskKeys.MaintainedTaskType, new WorkJournalEditHandler()  },
         {TaskKeys.ManageTaskType, new WorkJournalEditHandler()  },
         {TaskKeys.OfficeTaskType, new WorkJournalEditHandler()  },
      };

      public static Dictionary<string, PDFHandler> PDFHandlers = new Dictionary<string, PDFHandler>
      {
         {".doc", new WordToPDFHandler()  },
         {".docx", new WordToPDFHandler()  },
         {".xls", new ExcelToPDFHandler()  },
         {".xlsx", new ExcelToPDFHandler()  },
         {".pptx",new PPTToPDFHandler() },
         {".ppt",new PPTToPDFHandler() },
         {".pdf", new PDFHandler()  },
      };

   }

}
