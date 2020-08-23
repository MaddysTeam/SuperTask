using Business.Config;
using Business.Helper;
using Microsoft.AspNet.Identity.Owin;
using Symber.Web.Data;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace Business
{

   public interface IAccountManager<User, UId>
   {

      User GetAccountById(UId id);

      User GetAccountByNamePassword(string name, string password);

      SignInStatus SingIn(string userName, string password, bool rememberMe);

      bool Register(User user);

      bool ChangePassword(UId id, string oldPassword, string newPassword);

      bool Forbidden(UId id);
   }


   public enum AccountStatus
   {
      Enable = 0,
      Disable = 1,
      Frozen = 2
   }

   public class ST_AccountManager : IAccountManager<Account, Guid>
   {

      APDBDef _db;

      public ST_AccountManager() { }

      public ST_AccountManager(APDBDef db)
      {
         this._db = db;
      }


      public bool ChangePassword(Guid id, string oldPassword, string newPassword)
      {
         var account = GetAccountById(id);

         if (account == null)
            return false;


         var encryptedPassword = MD5Encryptor.Encrypt(oldPassword);

         if (encryptedPassword != account.Password)
            return false;

         account.Password = MD5Encryptor.Encrypt(newPassword);
         _db.AccountDal.Update(account);

         return true;
      }


      public void Reset(Guid id, string defaultPassword)
      {
         _db.AccountDal.UpdatePartial(id, new
         {
            Password = defaultPassword
         });
      }


      public Account GetAccountById(Guid id)
      {
         return _db.AccountDal.PrimaryGet(id);
      }


      public Account GetAccountByNamePassword(string name, string password)
      {
         var a = APDBDef.Account;

         var existAccounts = _db.AccountDal.ConditionQuery(a.UserName == name & a.Password == password, null, null, null);
         if (existAccounts != null)
            return existAccounts.FirstOrDefault();

         return null;
      }


      public bool Register(Account account)
      {
         if (account == null || string.IsNullOrEmpty(account.Password) || string.IsNullOrEmpty(account.UserName))
            return false;

         var a = APDBDef.Account;

         var exists = _db.AccountDal.ConditionQueryCount(a.UserName == account.UserName) > 0;

         if (!exists)
         {
            _db.AccountDal.Insert(account);
            return true;
         }

         return false;
      }


      public SignInStatus SingIn(string userName, string password, bool rememberMe)
      {
         var a = APDBDef.Account;
         var ur = APDBDef.UserRole;
         var r = APDBDef.Role;
         var ap = APDBDef.App;

         var existAccount = GetAccountByNamePassword(userName, password);

         var status = existAccount != null ? SignInStatus.Success : SignInStatus.Failure;

         if (status == SignInStatus.Success)
         {
            var userInfo = _db.UserInfoDal.PrimaryGet(existAccount.UserId);

            var roles = APQuery.select(ur.RoleId.As("roleId"), r.RoleName)
                              .from(r, ur.JoinInner(ur.RoleId == r.RoleId))
                              .where(ur.UserId == existAccount.UserId & r.RoleType == RoleKeys.SystemType)
                              .query(_db, re =>
                              new Role
                              {
                                 RoleId = ur.RoleId.GetValue(re, "roleId"),
                                 RoleName = r.RoleName.GetValue(re)
                              }).ToList();


            userInfo.SetRoles(roles);

            //TODO: will delete later,for temp; 这里的逻辑会在不久之后删除,因为现在改起来会很麻烦！
            if (roles.Any(role => role.RoleName == "admin"))
            {
               userInfo = _db.UserInfoDal.PrimaryGet(ResourceKeys.TempBossId);
               userInfo.UserName = "admin(不要做任何保存动作！切记)";
            }

            HttpContext.Current.SetValueToCookie(userInfo, userName);
         }

         return status;
      }


      public bool Forbidden(Guid id)
      {
         if (id.IsEmpty())
            return false;

         _db.AccountDal.UpdatePartial(id, new { Status = 1 });
         _db.UserInfoDal.UpdatePartial(id, new { IsDelete = 1 });

         return true;
      }

   }

}
