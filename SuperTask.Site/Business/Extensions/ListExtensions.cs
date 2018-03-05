using Business;
using Business.Config;
using Business.Helper;
using System.Collections.Generic;

namespace Business
{

   public static class ListExtensions
   {

      public static List<T> DeepClone<T>(this List<T> source)
      {
         using (System.IO.Stream objectStream = new System.IO.MemoryStream())
         {
            System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            formatter.Serialize(objectStream, source);
            objectStream.Seek(0, System.IO.SeekOrigin.Begin);
            return formatter.Deserialize(objectStream) as List<T>;
         }
      }

   }

}