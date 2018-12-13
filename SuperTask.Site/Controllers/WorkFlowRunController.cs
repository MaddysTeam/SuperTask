using Business;
using Business.Roadflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TheSite.Controllers
{

   public class WorkFlowRunController : BaseController
   {

      RoadflowService service = new RoadflowService();


      public ActionResult FlowIndex(RunParams parms)
      {
         var result = service.FlowIndex(parms);

         if (!string.IsNullOrEmpty(parms.ObjJson))
            ViewBag.objStr = parms.ObjJson.ToString();

         var query = string.Format("flowid={0}&instanceid={1}&taskid={2}&stepid={3}&groupid={4}&userId={5}&detaultMember={6}&display={7}&title={8}",
      parms.FlowId, parms.Instanceid, parms.Taskid, parms.Stepid, parms.Groupid, parms.UserId, parms.DetaultMember, parms.Display, parms.Title);


         ViewBag.query = query;
         ViewBag.src = "";

         return View(result);
      }


      public ActionResult Execute(RunParams parms)
      {
         var jsonParams = new object();
         var steps = new List<object>();
         var result = new FlowRunningResult();
         var flowType = parms.FlowType;

         if (string.IsNullOrEmpty(flowType))
            throw new ApplicationException("流程类型不存在！");

         try
         {

            if (parms.FlowType == "submit")
            {
               result = service.FlowSend(parms);

               foreach (var step in result.NextSteps)
               {
                  var defaultMember = result.StepDefaultUsers[step.ID];//默认处理人员

                  steps.Add(new { id = step.ID, member = defaultMember });
               }
            }
            else if (parms.FlowType == "back")
            {
               result = service.FlowBack(parms);

               foreach (var step in result.PrevSteps)
               {
                  steps.Add(new { id = step.Key, member = string.Empty });
               }
            }

            jsonParams = new { type = parms.FlowType, steps = steps };

            parms.JsonParams = Newtonsoft.Json.JsonConvert.SerializeObject(jsonParams);

            service.FlowExcute(parms);

         }
         catch
         {
            parms.RetryCount++;
            if (parms.RetryCount <= Business.Config.ThisApp.reviewRetryCount)
            {
               // 利用重试机制试下
               System.Threading.Thread.Sleep(3000);
               return Execute(parms);
            }
         }

         //流程完成或者发起后，通过url触发自定义逻辑

         if (Review.ReviewStrategy.ContainsKey(parms.FlowType))
         {
            var strategy = Review.ReviewStrategy[parms.FlowType];
            var review = db.ReviewDal.PrimaryGet(Guid.Parse(parms.Instanceid));

            if (strategy.ContainsKey(review.ReviewType))
            {
               var url = strategy[review.ReviewType];
               return Redirect(url + "?instanceId=" + parms.Instanceid);
            }
         }

         return RedirectToAction("Index", "Home");
      }


      public ActionResult ShowComment()
      {
         return View();
      }
   }

}