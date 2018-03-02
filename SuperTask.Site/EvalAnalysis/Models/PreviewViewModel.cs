using Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheSite.Models
{

   public class PreviewViewModel
   {

      public EvalTable EvalTable { get; set; }

      public List<Role> AccessorRoles { get; set; }

      public Role CurrentAccessorRole { get; set; }

      public List<EvalIndication> EvalIndications { get; set; }

   }

}
