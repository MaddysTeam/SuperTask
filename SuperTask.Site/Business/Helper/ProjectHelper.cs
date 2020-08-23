using Business;
using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Business.Helper
{

	public static class ProjectHelper
	{

		public static List<Project> All(APDBDef db)
		{
			var p = APDBDef.Project;
			return db.ProjectDal.ConditionQuery(p.ProjectStatus != ProjectKeys.DeleteStatus, null, null, null);
		}

		/// <summary>
		/// 某用户参与的所有项目
		/// </summary>
		/// <param name="userId">用户id</param>
		/// <param name="db"></param>
		/// <returns></returns>
		public static List<Project> UserJoinedProjects(Guid userId, APDBDef db=null)
		{
			var p = APDBDef.Project;
			var re = APDBDef.Resource;

         db = db ?? new APDBDef();
			var projects = APQuery.select(p.ProjectId, p.ProjectName, p.CreateDate,p.ProjectStatus)
				   .from(p,
						 re.JoinInner(re.Projectid == p.ProjectId & re.UserId == userId))
						 .order_by(p.CreateDate.Desc)
						 .distinct()
				   .query(db, r => new Project
				   {
					   ProjectId = p.ProjectId.GetValue(r),
					   ProjectName = p.ProjectName.GetValue(r),
					   ProjectStatus=p.ProjectStatus.GetValue(r)
				   }).ToList();

			return projects;
		}


		/// <summary>
		/// 某用户参与的所有可用项目，不包含被关闭和被删除的项目
		/// </summary>
		/// <param name="userId">用户id</param>
		/// <param name="db"></param>
		/// <returns></returns>
		public static List<Project> UserJoinedAvailableProject(Guid userId, APDBDef db) =>
			UserJoinedProjects(userId, db).FindAll(p => p.ProjectStatus != ProjectKeys.CompleteStatus & p.ProjectStatus != ProjectKeys.DeleteStatus);


		/// <summary>
		/// 通过项目id获取当前的项目数据
		/// </summary>
		/// <param name="projectid">项目id</param>
		/// <param name="db"></param>
		/// <param name="isforceClear"></param>
		/// <returns></returns>
		public static Project GetCurrentProject(Guid projectid, APDBDef db = null, bool isforceClear = false)
		{
			db = db ?? new APDBDef();
			return db.ProjectDal.PrimaryGet(projectid);
		}


		/// <summary>
		/// 通过项目节点任务的完成率来得到项目进度
		/// </summary>
		/// <param name="projectId">项目id</param>
		/// <param name="db"></param>
		/// <returns></returns>
		public static double GetProcessByNodeTasks(Guid projectId, APDBDef db)
		{
			var pst = APDBDef.ProjectStoneTask;
			var all = db.ProjectStoneTaskDal.ConditionQueryCount(pst.ProjectId == projectId);
			var completed = db.ProjectStoneTaskDal.ConditionQueryCount(pst.ProjectId == projectId & pst.TaskStatus == TaskKeys.CompleteStatus);

			return all <= 0 ? 0 : ((double)(completed * 100 / all)).Round(2);
		}


		public static List<Folder> CreateProjectFolders(Project project, Guid operatorId, APDBDef db)
		{
			List<Folder> folders = new List<Folder>();
			if (project == null || operatorId.IsEmpty())
				throw new ArgumentException("project cannot be null");

			var projectFolder = Folder.PrimaryGet(project.FolderId);
			if(projectFolder == null)
				folders.Add(
				projectFolder=ShareFolderHelper.CreateFolder(project.FolderId, project.ProjectName, ShareFolderKeys.RootProjectFolderId, ShareFolderKeys.ProjectType, operatorId, db)
				);
		
			var projectFolderTypes = DictionaryHelper.GetSubTypeDics(ShareFolderKeys.ProjectType);
			foreach (var item in projectFolderTypes)
			{
				folders.Add(
				ShareFolderHelper.CreateFolder(Guid.NewGuid(), item.Title, projectFolder.FolderId, item.ID, operatorId, db)
				);
			}

			return folders;
		}

		/// <summary>
		///  upload file to folder for specially type . for example: bug files shold be upload to test type folder
		/// </summary>
		/// <param name="folderTypeId"></param>
		/// <param name="fileId"></param>
		/// <param name="db"></param>
		public static void UploadFileToProjectFolder(Guid projectId, Guid folderTypeId, Guid fileId, Guid operatorId, APDBDef db)
		{
			if (projectId.IsEmpty() || folderTypeId.IsEmpty() || fileId.IsEmpty())
				return;

			var project = GetCurrentProject(projectId,db);
			var subFolders = ShareFolderHelper.GetSubFolders(project.FolderId, db);
			if (subFolders == null || subFolders.Count <= 0)
				subFolders = CreateProjectFolders(project, operatorId, db);

			var folder = subFolders.Find(x => x.FolderType == folderTypeId);
			ShareFolderHelper.UploadFolderFile(folder.FolderId, fileId, db);
		}

	}

}
