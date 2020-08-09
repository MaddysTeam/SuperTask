using Business;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Business.Helper
{

	public static class TaskHelper
	{
		static APDBDef.WorkTaskTableDef t = APDBDef.WorkTask;

		public static List<WorkTask> GetProjectTasks(Guid projectId, APDBDef db = null)
		{
			db = db ?? new APDBDef();
			return db.WorkTaskDal.ConditionQuery(t.Projectid == projectId
														 & t.TaskStatus != TaskKeys.DeleteStatus, t.SortId.Asc, null, null);
		}

		public static List<WorkTask> GetProjectUserTasks(Guid projectId, Guid userId, APDBDef db)
		{
			return db.WorkTaskDal.ConditionQuery(t.Projectid == projectId
														 & t.ManagerId == userId
														 & t.TaskStatus != TaskKeys.DeleteStatus, t.SortId.Asc, null, null);
		}

		public static List<WorkTask> GetProjectLeafTasks(Guid projectId, Guid userId, APDBDef db)
		{
			return GetProjectUserTasks(projectId, userId, db)
				  .Where(t => t.TaskStatus != TaskKeys.DeleteStatus & t.ParentId.IsEmpty())
				  .ToList();
		}

      public static List<WorkTask> GetUserTasks( Guid userId, APDBDef db)
      {
         return db.WorkTaskDal.ConditionQuery(t.ManagerId == userId & t.TaskStatus != TaskKeys.DeleteStatus, null, null, null);
      }



      /// <summary>
      /// write by huachao 2020.07.17.
      /// Get project task and its sub tasks
      /// </summary>
      /// <param name="projectId"></param>
      /// <param name="db"></param>
      /// <returns></returns>
      public static List<WorkTask> GetProjectTaskAndItsSubTask(Guid projectId,APDBDef db)
      {
         if (projectId.IsEmptyGuid())
            return new List<WorkTask>();
         var all = GetProjectTasks(projectId, db);
         var parents = all?.FindAll(x => x.IsParent);
         var results = new List<WorkTask>();
         foreach (var item in parents)
         {
            results.Add(item);
            results.AddRange(parents.FindAll(x => x.ParentId == item.TaskId));
         }

         return results;
      }


		/// <summary>
		/// 获取父任务的所有子任务
		/// </summary>
		/// <param name="parentId">父id</param>
		/// <returns></returns>
		public static List<WorkTask> GetAllChildren(Guid parentId, APDBDef db = null, Symber.Web.Data.APSqlOrderPhrase order=null)
		{
			db = db ?? new APDBDef();
			return db.WorkTaskDal.ConditionQuery(t.ParentId == parentId, order, null, null);
		}



		/// <summary>
		/// 获取父任务的所有子任务
		/// </summary>
		/// <param name="parent">父任务节点</param>
		/// <param name="all">任务列表</param>
		/// <returns></returns>
		public static List<WorkTask> GetAllChildren(WorkTask parent, List<WorkTask> all, bool inlcudeParent = false)
		{
			var list = new List<WorkTask>();
			if (parent == null)
				return list;

			if (inlcudeParent)
				list.Add(parent);

			if (all.Count <= 1)
				return list;

			var parentSortId = parent.ParentId.IsEmpty() ? 0 : all.FindIndex(tk => tk.TaskId == parent.TaskId);

			for (int i = parentSortId + 1; i < all.Count; i++)
			{
				if (all[i].TaskId == parent.TaskId || all[i].IsDelteStatus)
					continue;
				if (all[i].TaskLevel <= parent.TaskLevel)
					break;
				else
					list.Add(all[i]);
			}

			return list;
		}


		public static List<WorkTask> GetAllParents(WorkTask child, List<WorkTask> all)
		{
			var parents = new List<WorkTask>();
			while (!child.ParentId.IsEmpty())
			{
				var parent = all.Find(x => x.TaskId == child.ParentId);
				parents.Add(parent);

				child = parent;
			}

			return parents;
		}


		/// <summary>
		/// 设置任务进度
		/// </summary>
		/// <param name="tasks">所有任务</param>
		/// <param name="root">父任务实体</param>
		/// <returns></returns>
		public static void SetTasksProcessRate(List<WorkTask> tasks, WorkTask root)
		{
			if (root == null) return;
			if (!root.ParentId.IsEmpty())
				root.ParentId = Guid.Empty;

			var nodes = new List<Node<WorkTask>>();

			foreach (var tk in tasks)
			{
				if (tk.IsParent)
				{
					tk.RateOfProgress = 0;
				}

				nodes.Add(new Node<WorkTask> { NodeId = tk.TaskId, ParentId = tk.ParentId, Instance = tk });
			}

			var parentNode = new Node<WorkTask> { NodeId = root.TaskId, Instance = root };
			LoopToAppendChildren(nodes, parentNode);
		}


		/// <summary>
		/// 逻辑删除任务
		/// </summary>
		/// <param name="delTaskIds">需要删除的任务id</param>
		/// <param name="all">任务列表</param>
		/// <param name="reOrderSortId">是否重新排序</param>
		/// <returns>WorkTask list</returns>
		public static List<WorkTask> RemoveDelTasks(List<string> delTaskIds, List<WorkTask> all, bool reOrderSortId)
		{
			var removed = new List<WorkTask>();

			if (delTaskIds == null || delTaskIds.Count < 0)
				return removed;

			foreach (var item in delTaskIds)
			{
				var guidId = item.ToGuid(Guid.Empty);

				var deltk = all.FirstOrDefault(tk => tk.TaskId == guidId);
				var delTks = GetAllChildren(deltk, all, true);

				removed.AddRange(delTks);

				delTks.ForEach(tk =>
				{
					tk.SetStatus(TaskKeys.DeleteStatus);

					//将标记为删除的任务移出任务集合
					var t = all.FirstOrDefault(x => x.TaskId == tk.TaskId);
					if (t != null)
						all.Remove(t);
				});

			}

			if (reOrderSortId)
			{
				//调整sortId
				var i = 1;
				all.ForEach(tk =>
				{
					tk.SortId = i;
					i++;
				});
			}

			return removed;
		}


		/// <summary>
		/// 创建项目根任务
		/// </summary>
		/// <param name="user">用户</param>
		/// <param name="project">项目</param>
		/// <param name="db">APDBDef</param>
		/// <returns>WorkTask</returns>
		public static WorkTask CreateAndSaveRootTaskInDB(UserInfo user, Project project, APDBDef db)
		{
			var task = new WorkTask
			{
				TaskId = Guid.NewGuid(),
				TaskName = project.ProjectName,
				Projectid = project.ProjectId,
				ManagerId = user.UserId,
				CreatorId = user.UserId,
				ReviewerID = user.UserId,
				TaskType = TaskKeys.ProjectTaskType,
				TaskStatus = TaskKeys.PlanStatus,
				//Creator = userName,
				//Manager = userName,
				CreateDate = DateTime.Now,
				EndDate = project.EndDate,
				SortId = 1 //表示根任务
			};
			db.WorkTaskDal.Insert(task);

			return task;
		}


		private static void LoopToAppendChildren(List<Node<WorkTask>> all, Node<WorkTask> parent)
		{
			var subItems = all.Where(ee => ee.ParentId == parent.NodeId).ToList();
			parent.children = new List<Node<WorkTask>>();
			parent.children.AddRange(subItems);

			foreach (var subItem in subItems)
			{
				//递归
				LoopToAppendChildren(all, subItem);
			}

			//递归后：从叶子任务开始逐层往上遍历,如果有子任务且全部完成则该父任务完成，否则循环遍历子任务，将完成的预估时间加到分子上

			var parentTk = parent.Instance;
			parentTk.IsParent = parentTk != null && parent.children.Count > 0;

			if (parentTk.IsParent)
			{
				parentTk.EstimateWorkHours = TaskKeys.DefaultEstimateHours;
				parentTk.Numerator = 0;

				if (parent.children.All(x => x.Instance.IsCompleteStatus))
				{
					parentTk.SetStatus(TaskKeys.CompleteStatus);
					parentTk.RateOfProgress = 100;
					parentTk.RealEndDate = DateTime.Now;
				}
				else if (parent.children.Any(x => x.Instance.IsProcessStatus))
				{
					parentTk.SetStatus(TaskKeys.ProcessStatus);
					parentTk.RateOfProgress = 0;
				}
				else if (parentTk.IsCompleteStatus)
				{
					parentTk.SetStatus(TaskKeys.ProcessStatus);
					parentTk.RateOfProgress = 0;
				}
				else
				{
					parentTk.RateOfProgress = 0;
				}

				//便利每个父任务的子任务

				foreach (var item in parent.children)
				{
					var tk = item.Instance;
					parentTk.EstimateWorkHours += (tk.EstimateWorkHours == 0 ? TaskKeys.DefaultEstimateHours : tk.EstimateWorkHours);

					//子任务能改变父任务的开始和结束时间
					if (tk.StartDate < parentTk.StartDate) parentTk.StartDate = tk.StartDate;
					if (tk.EndDate > parentTk.EndDate) parentTk.EndDate = tk.EndDate;

					if (tk.IsProcessStatus && tk.RateOfProgress == 100)
						tk.RateOfProgress = 0;

					if (tk.IsCompleteStatus)
					{
						tk.RateOfProgress = 100;
						parentTk.Numerator += tk.EstimateWorkHours;
					}
				}
			}
		}


		class Node<T>
		{
			public Guid NodeId { get; set; }

			public Guid ParentId { get; set; }

			public string Name { get; set; }

			public List<Node<T>> children { get; set; }

			public T Instance { get; set; }
		}
	}


	public class TaskLogHelper
	{

		static APDBDef.WorkTaskLogTableDef wtl = APDBDef.WorkTaskLog;

		public static void CreateLog(WorkTask task, Guid operatorId, APDBDef db)
		{
			db.WorkTaskLogDal.Insert(new WorkTaskLog
			{
				LogID = Guid.NewGuid(),
				CreateDate = DateTime.Now,
				EndDate = task.EndDate,
				EstimateWorkHours = task.EstimateWorkHours,
				TaskManagerId = task.ManagerId,
				ParentId = task.ParentId,
				ProjectId = task.Projectid,
				RateOfProgress = task.RateOfProgress,
				StartDate = task.StartDate,
				ReviewerID = task.ReviewerID,
				TaskStatus = task.TaskStatus,
				TaskType = task.TaskType,
				TaskId = task.TaskId,
				WorkHours = task.WorkHours,
				OperatorId = operatorId,
				TaskName = task.TaskName,
				ServiceCount = task.ServiceCount
			});
		}

		public static void CreateLogs(List<WorkTask> tasks, Guid operatorId, APDBDef db)
		{
			if (tasks == null || tasks.Count <= 0 || db == null)
				return;

			foreach (var tk in tasks)
			{
				CreateLog(tk, operatorId, db);
			}
		}

		public static void CreateLogs(IEnumerable<WorkTask> current, IEnumerable<WorkTask> orignals, Guid operatorId, APDBDef db)
		{
			if (current == null || orignals == null) return;

			var enumrator = current.GetEnumerator();

			while (enumrator.MoveNext())
			{
				if (!orignals.Any(t => t.Equals(enumrator.Current)))
				{
					CreateLog(enumrator.Current, operatorId, db);
				}
			}
		}

	}

}
