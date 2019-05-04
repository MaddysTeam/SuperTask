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

      public GenerateProjectDataJob()
      {
         _db = new APDBDef();
      }


      public void Execute(){}


      public void Stop(bool immediate)
      {
         throw new NotImplementedException();
      }

   }

}