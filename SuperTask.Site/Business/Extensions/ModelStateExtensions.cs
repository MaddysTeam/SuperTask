using System.Text;
using System.Web.Mvc;

namespace Business
{

   public static class ModelStateExtensions
   {

      public static string ShowErrorMessages(this ModelStateDictionary state)
      {
         StringBuilder errinfo = new StringBuilder();
         foreach (var s in state.Values)
         {
            foreach (var p in s.Errors)
            {
               errinfo.AppendFormat(@"{0} <br>", p.ErrorMessage);
            }
         }

         return errinfo.ToString();
      }

   }

}