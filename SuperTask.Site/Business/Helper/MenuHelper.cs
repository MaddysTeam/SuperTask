using RoadFlow.Data.MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheSite.Models;

namespace Business.Helper
{

   //TODO change to dynamic 
   public class MenuHelper
   {
      public static Guid ProjectMainPageCode = "de2acf4d-a32d-4b33-9daf-c3e4d5b9cd4d".ToGuid(Guid.Empty);
      public static Guid ProjectResourcePageCode = "de2acf4d-a32d-4b33-9daf-c3e4d5b9cd5d".ToGuid(Guid.Empty);
      public static Guid ProjectTaskPageCode = "de2acf4d-a32d-4b33-9daf-c3e4d5b9cd6d".ToGuid(Guid.Empty);
      public static Guid ProjectProcessPageCode = "de2acf4d-a32d-4b33-9daf-c3e4d5b9cd7d".ToGuid(Guid.Empty);
      public static Guid ProjectFilePageCode = "de2acf4d-a32d-4b33-9daf-c3e4d5b9cd8d".ToGuid(Guid.Empty);

      public static List<MenuItem> GetProjectMenuItems(Guid projectId,Guid userId,Guid checkedId)
      {
         var currentProject = ProjectrHelper.GetCurrentProject(projectId, new APDBDef());

         if (currentProject == null) return null;

         var subMenu= new List<MenuItem>()
         {
            new MenuItem { Id = ProjectMainPageCode, FullTitle=$"首页({currentProject.ProjectName})", Title = $"首页({currentProject.ProjectName})".Ellipsis(10), IsVisible = true, Url = string.Format("/project/{0}?id={1}", "details", projectId) },
            new MenuItem { Id = ProjectFilePageCode,FullTitle = "项目文件夹", Title = "项目文件夹", IsVisible = true, Url = string.Format("/ShareFolder/{0}?projectId={1}", "Index", projectId)},
            new MenuItem { Id = ProjectResourcePageCode,FullTitle = "项目资源", Title = "项目资源", IsVisible = true , Url = string.Format("/ProjectManage/{0}?projectId={1}", "Resource", projectId) },
            new MenuItem { Id = ProjectTaskPageCode, FullTitle = "任务分配",Title = "任务分配", IsVisible = true, Url = string.Format("/ProjectManage/{0}?projectId={1}", "TaskArrangement", projectId)},
            new MenuItem { Id = ProjectProcessPageCode, FullTitle = "任务进度", Title = "任务进度", IsVisible = ResourceHelper.HasPermission(userId, projectId, "P_10005", new APDBDef()), Url = string.Format("/ProjectManage/{0}?projectId={1}", "ProcessInfo", projectId)} ,
         };

         foreach (var item in subMenu)
         {
            item.IsChecked = checkedId == item.Id;
         }

         return subMenu;
      }

   }

}