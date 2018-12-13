using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Helper
{

   public class ReviewKeys
   {

      public static Guid FlowId => Guid.Parse("C70BC5D3-3153-456E-A56F-368035A76D4D");

      public static Guid TypeGuid=> Guid.Parse("DD3ADC7F-A55C-3C58-9CAF-D3A2B7A9DA1B");

      public static Guid StatusGuid => Guid.Parse("dd3adc7f-a55c-3c58-9caf-d3a2b7a9da7b");

      public static Guid ReviewTypeForPjChanged => Guid.Parse("E98894E6-1DA1-3BCC-2E03-AF0BAAA4C62F");

      public static Guid ReviewTypeForTkChanged => Guid.Parse("26999EA8-3BDD-A4FB-495B-E440B6FF7408");

      public static Guid ReviewTypeForTkSubmit => Guid.Parse("88B7B73E-630B-B338-C4B9-6F4870DB1A7F");

      public static Guid ProjectRequestFailed => Guid.Parse("30472940-C13B-0413-CC4E-E6656BC07C67");

      public static Guid TaskRequestFailed => Guid.Parse("2B34A362-DA94-6A50-7C01-D0FC10A17ACA");


      public static Guid ResultWait => Guid.Parse("D39C12D1-D7B1-31FF-4960-949C994F3574");

      public static Guid ResultSuccess => Guid.Parse("4861D3E5-FEAE-0127-17EB-DF6D69C30304");

      public static Guid ResultFailed => Guid.Parse("CAF25AF4-4A15-D844-C9C5-E278FAEC7CD9");

      public static Guid ResultUnknow => Guid.Parse("1F363B4A-F430-B7DB-F835-27C1818B3E4F");


      public static string GetTypeKeyByValue<V>(V val) => DictionaryHelper.GetDicByValue(TypeGuid, val).Title;

      public static string GetStatusKeyByValue<V>(V val) => DictionaryHelper.GetDicByValue(StatusGuid, val).Title;

   }

}
