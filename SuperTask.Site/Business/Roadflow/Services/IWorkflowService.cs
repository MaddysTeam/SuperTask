using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Roadflow
{

   public interface IWorkflowService
   {

   }


   public interface IRoadFlow_Service: IWorkflowService
   {

      FlowRunningResult FlowIndex(RunParams paras);
      FlowRunningResult FlowSend(RunParams paras);
      FlowRunningResult FlowExcute(RunParams paras);
      FlowRunningResult FlowBack(RunParams paras);

   }

}
