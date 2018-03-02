using Business.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheSite.Models;

namespace Business
{
   public partial class Account
   {
      public event Forbidden WhenForbidden;
      public delegate void Forbidden(Account account);

      public void ExcuteForbidden(Account account)
      {
         if (WhenForbidden != null)
         {
            WhenForbidden.Invoke(account);
         }
      }

      public IAsyncResult ExcuteForbiddenAsync(Account account, AsyncCallback callback, object para = null)
      {
         if (WhenForbidden != null)
         {
            return WhenForbidden.BeginInvoke(account, callback, para);
         }

         return null;
      }

   }

}
