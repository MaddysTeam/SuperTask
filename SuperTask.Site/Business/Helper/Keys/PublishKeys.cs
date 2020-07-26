using Business.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Helper
{

   public static class PublishKeys
   {

      public static Guid SelectAll => AppKeys.SelectAll;


      /// <summary>
      /// 发布类型
      /// </summary>
      public static Guid TypeGuid = Guid.Parse("dee2b239-9cbf-47c4-bb7e-6ebdd402e947");

      /// <summary>
      /// 发布状态
      /// </summary>
      public static Guid StatusGuid = Guid.Parse("d0608da5-2e29-4240-89de-aa342b9b655c");
      public static Guid Ready = Guid.Parse("fabb206d-d46a-4121-82ad-ba67c4810694");
      public static Guid Success = Guid.Parse("e1547596-6ccd-47ad-a2aa-0baaf62d6af2");
      public static Guid Fail = Guid.Parse("c5649764-1ccb-4e90-89c9-5c6537e53643");
      public static Guid Close = Guid.Parse("d503b017-93a0-4c13-aae9-4e191b33f226");

      /// <summary>  
      /// 发布处理
      /// </summary>
      public static Guid HandleGuid = Guid.Parse("13715897-a68a-4f32-a8cc-e0cd60fab689");
      public static Guid HandleReady = Guid.Parse("6ea0dac1-056d-47c0-9bf4-57c3b4c4e08a");
      public static Guid HandleSuccess = Guid.Parse("6d510e62-dc4a-4ab7-be73-2ef72123de64");
      public static Guid HandleFail = Guid.Parse("5f6e38bc-047a-48d5-916b-3462b657ccee");

      /// <summary>  
		/// 关联
		/// </summary>
      public static Guid RelativeGuid = Guid.Parse("920d91d9-a88b-4837-a5be-3632e74da2ea");


      public static string GetTypeKeyByValue(Guid val) => DictionaryHelper.GetDicById(TypeGuid, val).Title;

      public static string GetStatusKeyByValue(Guid val) => DictionaryHelper.GetDicById(StatusGuid, val).Title;



      public static Dictionary<Guid, Guid> HandleMapping => new Dictionary<Guid, Guid>
      {
         {HandleReady,Ready }, {HandleSuccess,Success }, {HandleFail,Fail}
      };

      public static Dictionary<Guid, string> OperationResultDic = new Dictionary<Guid, string>
      {
       { HandleReady,"待发布" },
       { HandleSuccess,"发布成功" },
       { HandleFail, "发布失败" },
       { Close, "关闭发布" },
       {RelativeGuid,"变更了发布关联" }
		};

   }

}
