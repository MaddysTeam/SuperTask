using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Helper
{

   public static class PaymentsKeys
   {
      // 款项类型
      public static Guid PaymensTypeGuid=> Guid.Parse("95bcb7e2-b4ab-456b-bde2-22b40c6a2d4d");
      public static Guid ProjectPaymentsType => Guid.Parse("e4d5b55d-c1bb-4954-92bf-58f0f35ff109"); // 项目款项
      public static Guid InternalVenderPaymentsType => Guid.Parse("c14eb95b-0f39-4b45-9399-759b783172a6"); // 内部外包款项
      public static Guid CheckBeforeDeliveryType => Guid.Parse("7a8568af-feef-41e2-9365-17ad9cc1a947"); // 交付前的验收款项
      public static Guid BondType => Guid.Parse("ad1fd7f1-a6f7-455e-be88-b853f7da82d2"); // 保证金
      public static Guid GuaranteeType => Guid.Parse("664b555f-277c-4808-888c-1109ce41a6b7"); // 保函
      public static Guid NothingType => Guid.Parse("9083e19a-7901-43e4-95e9-cc4dbfc95482"); // 无

      public static string FirstPayment="首付款";
      public static string MiddlePayment = "中期款";
      public static string TailPayment = "尾款";
      public static string DefaultVenderName = "默认供应商";

      public static Guid AppointGuaranteeResourceId = Guid.Parse("9ea2e0f3-3806-4cc0-807d-1943ab63200b");
      public static Guid QualityGuaranteeResourceId = Guid.Parse("86e4cd97-f19d-4637-a50f-5ca72d49e3f8");

      public static string GetTypeKeyByValue<V>(V val) => DictionaryHelper.GetDicByValue(PaymensTypeGuid, val).Title;
   }

}
