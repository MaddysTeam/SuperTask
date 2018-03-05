using Business;
using Business.Config;
using Business.Helper;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web.Mvc;
using TheSite.Models;

namespace TheSite.Controllers
{

   [AllowAnonymous]
   public class AccountController : BaseController
   {

      ST_AccountManager _manager;

      public AccountController()
      {
         _manager = new ST_AccountManager(db);
      }

      // GET: Account/Login
      // POST: Account/Login

      [AllowAnonymous]
      public ActionResult Login()
      {
         return View();
      }

      [HttpPost]
      [AllowAnonymous]
      [ValidateAntiForgeryToken]
      public ActionResult Login(LoginViewModel model, string returnUrl)
      {
         if (!ModelState.IsValid)
         {
            return View(model);
         }

         if (string.IsNullOrEmpty(returnUrl))
         {
            returnUrl = ThisApp.DefaultUrl;
         }

         var encryptPassword = MD5Encryptor.Encrypt(model.Password);
         var result = _manager.SingIn(model.Username, encryptPassword, model.RememberMe);

         switch (result)
         {
            case SignInStatus.Success:
               return RedirectToLocal(returnUrl);
            case SignInStatus.LockedOut:
               return View("Lockout");
            case SignInStatus.RequiresVerification:
               return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
            case SignInStatus.Failure:
            default:
               ModelState.AddModelError("", "用户名或密码不正确。");
               return View(model);
         }
      }


      // POST:				/Account/LogOff

      [HttpPost]
      [ValidateAntiForgeryToken]
      public ActionResult LogOff()
      {
         Session.Clear();
         System.Web.Security.FormsAuthentication.SignOut();
         return RedirectToAction("Login", "Account");
      }


      // GET: Account/Register
      // POST: Account/Register

      [AllowAnonymous]
      public ActionResult Register()
      {
         return View();
      }

      [HttpPost]
      [ValidateAntiForgeryToken]
      public ActionResult Register(RegisterViewModel model)
      {
         return RedirectToAction("Login");
      }


      //	修改密码
      //	GET:						/Account/ChgPwd			
      //	POST-AJAX:				/Account/ChgPwd	

      public ActionResult ChgPwd()
      {
         return View();
      }

      [HttpPost]
      [ValidateAntiForgeryToken]
      public ActionResult ChgPwd(ChgPwdViewModel model)
      {
         ThrowNotAjax();


         var result = _manager.ChangePassword(GetUserInfo().UserId, model.OldPassword, model.NewPassword);

         return Json(new
         {
            result = result ? AjaxResults.Success : AjaxResults.Error,
            msg = result ? "密码修改成功" : "请输入正确的密码"
         });
      }


      [HttpPost]
      public ActionResult Reset(Guid id)
      {
         ThrowNotAjax();

         _manager.Reset(id, MD5Encryptor.Encrypt(ThisApp.DefaultPassword));


         return Json(new
         {
            result = AjaxResults.Success,
            msg = "初始密码为123456"
         });
      }

      [HttpPost]
      public ActionResult Forbidden(Guid id)
      {
         ThrowNotAjax();

         var result = _manager.Forbidden(id);

         return Json(new
         {
            result = result ? AjaxResults.Success : AjaxResults.Error,
            msg = "操作成功"
         });
      }


      #region [ Private ]


      private ActionResult RedirectToLocal(string returnUrl)
      {
         if (Url.IsLocalUrl(returnUrl))
         {
            return Redirect(returnUrl);
         }

         return RedirectToAction("Index", "Home");
      }


      #endregion

   }

}