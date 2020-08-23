using Business.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TheSite.Models;

namespace Business
{

   public partial class Resource
   {

      [Display(Name = "所属项目")]
      public string ProjectName { get; set; }

      [Display(Name = "所属任务")]
      public string TaskName { get; set; }

      [Display(Name = "资源类型")]
      public string TypeNames { get; set; }

      public string UserName { get; set; }

      public bool IsReadonlyStatus => this.Status == ResourceKeys.ReadonlyStatus;

      public bool IsEditableStatus => this.Status == ResourceKeys.EditableStatus;


      public virtual bool IsLeader()
        => ResourceTypes.InlcudeAny(new Guid[] { ResourceKeys.PMType, ResourceKeys.HeaderType });

      public virtual bool IsPM()
       => ResourceTypes.InlcudeAny(new Guid[] { ResourceKeys.PMType });

      public virtual bool IsHeader()
      => ResourceTypes.InlcudeAny(new Guid[] { ResourceKeys.HeaderType });

      //public static string DefaultLeaderTypes=> string.Join(",", new string[] { ResourceKeys.HeaderType.ToString()});
      //public static string DefaultPMTypes => ResourceKeys.PMType.ToString()

      public void SetStatus(Guid status)
      {
         Status = status;
      }


      public Result Validate()
      {
         var message = string.Empty;
         var result = true;

         if (string.IsNullOrEmpty(ResourceName))
         {
            message = Errors.Resource.NOT_ALLOWED_NAME_NULL;
            result = false;
         }

         return new Result { IsSuccess = result, Msg = message };
      }


      public static Resource Create(string resourceName,Guid userId,Guid projectId,string resourceTypes)
      {
         return new Resource
         {
            ResourceId = Guid.NewGuid(),
            UserId = userId,
            ResourceName = resourceName,
            CreateDate = DateTime.Now,
            Projectid = projectId,
            Status = ResourceKeys.EditableStatus,
            ResourceTypes = resourceTypes
         };
      }

   }

}