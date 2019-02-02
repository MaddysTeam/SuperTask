using Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheSite.Models
{
   public class EvalBuilderViewModel
   {

     public EvalTable EvalTable { get; set; } // right side eval table

     public List<Indication> Indications { get; set; } //left side indicatinos

     public List<EvalIndication> EvalIndications { get; set; } //right side eval indicatinos

   }
}
