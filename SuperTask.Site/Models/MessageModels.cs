namespace TheSite.Models
{

   public class Result
   {

      public static Result Initial()=> new Result { IsSuccess=true, Msg= "操作成功" };

      public bool IsSuccess { get; set; }
      public string Msg { get; set; }
   }

}
 