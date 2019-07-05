//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using System;
//using System.IO;
//using System.Net;
//using System.Security.Cryptography;
//using System.Text;
//using System.Web;
//using Wangsu.WcsLib.Core;
//using Wangsu.WcsLib.HTTP;
//using Wangsu.WcsLib.Utility;

//namespace Util.ThirdParty.WangsuCloud
//{

//   public class FileUploader
//   {

//      public const string ACCESS_KEY = "e35c1b6c230532f52c2d2dbaaf96322829a2afbc"; // 必须
//      public const string SECURITY_KEY = "7253219e0206883520aa81840bac6e4972156ef0";// 必须
//      public const string UPLOAD_DOMAIN = "tejiao.up28.v1.wcsapi.com";// 必须
//      public const string MANAGER_DOMAIN = "tejiao.mgr28.v1.wcsapi.com"; // 必须
//      public const string SCOPE = "tejiao-test001"; //项目文件夹，如新项目需要登录创建
//      public const string DEADLINE = "1546300800000";
//      public const string DOMAIN = "cdn.sser.shdjg.net"; 

//      public static SimpleUpload SimpleUploader
//      {
//         get
//         {
//            if (_simpleUploader == null)
//            {
//               _simpleUploader = new SimpleUpload(
//               new Mac(ACCESS_KEY, SECURITY_KEY),
//               new Config(UPLOAD_DOMAIN, MANAGER_DOMAIN)
//               );
//            }

//            return _simpleUploader;
//         }
//      }

//      public static SliceUpload SliceUploader
//      {
//         get
//         {
//            if (_sliceUploader == null)
//            {
//               Environment.SetEnvironmentVariable("WcsLibAkSk", $"{ACCESS_KEY},{SECURITY_KEY}");

//               _sliceUploader = new SliceUpload(
//               new Mac(ACCESS_KEY, SECURITY_KEY),
//               new Config(UPLOAD_DOMAIN, MANAGER_DOMAIN)
//               );
//            }

//            return _sliceUploader;
//         }
//      }


//      /// <summary>
//      /// 上传文件，通过流的方式上传
//      /// </summary>
//      /// <param name="file"></param>
//      /// <param name="isOverwirte"></param>
//      /// <returns></returns>
//      public static UploadResult Upload(UploadFile file, bool isOverwirte = true)
//      {
//         if (null == file && null == file.Stream)
//            throw new ArgumentNullException();

//         var fileName = file.FileName;
//         var result = new UploadResult();
//         var policy = new PutPolicy { scope = SCOPE, deadline = DEADLINE, overwrite = isOverwirte ? "1" : "0" };
//         var policyJson = JsonConvert.SerializeObject(policy);

//         try
//         {
//            file.Stream.Seek(0, SeekOrigin.Begin); //需要明确的长度

//            SimpleUploader.UploadStream(file.Stream, policyJson, fileName);

//            result.IsSuccess = true;
//            result.FileUrl = $"http://{FileUploader.DOMAIN}/{fileName}";
//         }
//         catch (Exception e)
//         {
//            result.IsSuccess = false;
//            result.Message = $"exception:{e.Message}";
//         }

//         return result;
//      }


//      /// <summary>
//      /// 分片上传文件
//      /// </summary>
//      /// <param name="file"></param>
//      /// <param name="isOverwirte"></param>
//      /// <returns></returns>
//      public static UploadResult SliceUpload(UploadFile file, bool isOverwirte = true)
//      {
//         if (null == file && null == file.Stream)
//            throw new ArgumentNullException();

//         var fileName = file.FileName;
//         var result = new UploadResult();
//         var policy = new PutPolicy { scope = SCOPE, deadline = DEADLINE, overwrite = isOverwirte ? "1" : "0" };
//         var policyJson = JsonConvert.SerializeObject(policy);

//         try
//         {
//            file.Stream.Seek(0, SeekOrigin.Begin); //需要明确的长度

//            SliceUploadHelper.UploadStream(file.Stream, SliceUploader, policyJson, fileName);

//            result.IsSuccess = true;
//            result.FileUrl = $"http://{FileUploader.DOMAIN}/{fileName}";
//         }
//         catch (Exception e)
//         {
//            result.IsSuccess = false;
//            result.Message = $"exception:{e.Message}";
//         }

//         return result;
//      }

//      private static SimpleUpload _simpleUploader;
//      private static SliceUpload _sliceUploader;
//   }

//   public static class Certificate
//   {

//      public static string ToBase64hmac(this string strText, string strKey, Encoding encode = null)
//      {
//         encode = encode ?? Encoding.UTF8;
//         HMACSHA1 myHMACSHA1 = new HMACSHA1(encode.GetBytes(strKey));
//         byte[] byteText = myHMACSHA1.ComputeHash(encode.GetBytes(strText));

//         return Convert.ToBase64String(byteText);
//      }

//      public static string ToBase64(this string source, Encoding encode = null)
//      {
//         encode = encode ?? Encoding.UTF8;
//         byte[] bytes = encode.GetBytes(source);
//         var str = string.Empty; ;

//         try
//         {
//            str = Convert.ToBase64String(bytes);
//         }
//         catch
//         {
//            str = source;
//         }

//         return str;
//      }

//   }

//   public class UploadFile
//   {
//      public string FileName { get; set; }
//      public Stream Stream { get; set; }
//   }

//   public class UploadResult
//   {
//      public bool IsSuccess { get; set; }
//      public string Message { get; set; }
//      public string FileUrl { get; set; }
//   }

//   public class PutPolicy
//   {
//      public string scope { get; set; }
//      public string deadline { get; set; }
//      public string saveKey { get; set; }
//      public string overwrite { get; set; }
//   }

//   public static class SliceUploadHelper
//   {

//      public static HttpResult UploadStream(Stream stream, SliceUpload uploader, string policy, string file)
//      {

//         Auth auth = EnvUtility.EnvAuth();
//         Config config = EnvUtility.EnvConfig();

//         // 上传到这个 bucket
//         string bucket = FileUploader.SCOPE;
//         string key = file;

//         // 上传前先删除
//         BucketManager bm = new BucketManager(auth, config);
//         HttpResult result = bm.Delete(bucket, key);

//         long dataSize = GetDataSize(stream.Length);
//         byte[] data = new byte[dataSize];
//         stream.Read(data, 0, data.Length);

//         // 最后合成文件时的 hash
//         Console.WriteLine("ETag of uploading data: {0}", ETag.ComputeEtag(data));

//         // 一个小时的超时，转为 UnixTime 毫秒数
//         long deadline = UnixTimestamp.GetUnixTimestamp(3600) * 1000;
//         string putPolicy = "{\"scope\": \"" + bucket + ":" + key + "\",\"deadline\": \"" + deadline + "\"}";
//         string uploadToken = auth.CreateUploadToken(putPolicy);

//         // 第一个分片不宜太大，因为可能遇到错误，上传太大是白费流量和时间！
//         const long blockSize = 4 * 1024 * 1024;
//         const int firstChunkSize = 1024;

//         SliceUpload su = uploader; // new SliceUpload(auth, config);
//         result = su.MakeBlock(blockSize, 0, data, 0, firstChunkSize, uploadToken);

//         if ((int)HttpStatusCode.OK == result.Code)
//         {
//            long blockCount = (dataSize + blockSize - 1) / blockSize;
//            string[] contexts = new string[blockCount];


//            JObject jo = JObject.Parse(result.Text);

//            contexts[0] = jo["ctx"].ToString();

//            // 上传第 1 个 block 剩下的数据
//            result = su.Bput(contexts[0], firstChunkSize, data, firstChunkSize, (int)(blockSize - firstChunkSize), uploadToken);
//            if ((int)HttpStatusCode.OK == result.Code)
//            {
//               jo = JObject.Parse(result.Text);
//               contexts[0] = jo["ctx"].ToString();

//               // 上传后续 block，每次都是一整块上传
//               for (int blockIndex = 1; blockIndex < blockCount; ++blockIndex)
//               {
//                  long leftSize = dataSize - blockSize * blockIndex;
//                  int chunkSize = (int)(leftSize > blockSize ? blockSize : leftSize);
//                  result = su.MakeBlock(chunkSize, blockIndex, data, (int)(blockSize * blockIndex), chunkSize, uploadToken);
//                  if ((int)HttpStatusCode.OK == result.Code)
//                  {
//                     jo = JObject.Parse(result.Text);
//                     contexts[blockIndex] = jo["ctx"].ToString();
//                  }
//                  else
//                  {
//                     return result;
//                  }
//               }

//               // 合成文件，注意与前面打印的 ETag 对比

//               result = su.MakeFile(dataSize, key, contexts, uploadToken);
//            }
//         }


//         return result;
//      }


//      public static class EnvUtility
//      {
//         public static Auth EnvAuth()
//         {
//            string[] aksk = GetEnvValues("WcsLibAkSk");
//            if (null != aksk && 2 <= aksk.Length)
//            {
//               Mac mac = new Mac(aksk[0], aksk[1]);
//               return new Auth(mac);
//            }

//            throw new Exception("Please set EnvironmentVariable \"WcsLibAkSk\" to \"AccessKey,SecretKey\".");
//         }

//         public static Config EnvConfig()
//         {
//            string[] configs = GetEnvValues("WcsLibConfig");
//            if (null != configs && 2 <= configs.Length)
//            {
//               bool useHttps = false;
//               if (3 == configs.Length && 0 == configs[2].CompareTo("true"))
//               {
//                  useHttps = true;
//               }
//               return new Config(configs[0], configs[1], useHttps);
//            }

//            Console.WriteLine("EnvironmentVariable \"WcsLibConfig\" not found, or invalid, use default Config().");
//            return new Config();
//         }

//         private static string[] GetEnvValues(string variable)
//         {
//            string envVar = Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.Process);
//            if (string.IsNullOrEmpty(envVar))
//            {
//               envVar = Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.User);
//               if (string.IsNullOrEmpty(envVar))
//               {
//                  envVar = Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.Machine);
//               }
//            }
//            if (!string.IsNullOrEmpty(envVar))
//            {
//               return envVar.Split(',');
//            }
//            return null;
//         }
//      }


//      private static long GetDataSize(long actualSize)
//      {
//         long chunckSize = 5 * 1024 * 1024;
//         long size = chunckSize;
//         var i = 1;
//         while (actualSize >= i * chunckSize)
//         {
//            size = i * chunckSize;s'x
//            i++;
//         }

//         size = i * chunckSize;

//         return size;

//      }
//   }

//}
