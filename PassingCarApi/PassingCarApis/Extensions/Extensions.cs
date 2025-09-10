using Newtonsoft.Json;
using PassingCarApis.Configuration;
using System.Diagnostics;

namespace PassingCarApis.Extensions
{
    public static class Extensions
    {
        public static bool HasValues(this IEnumerable<object> enumerable)
        {
            return enumerable != null && enumerable.Count() > 0;
        }
        public static void CatchIt(this Exception ex)
        {
            JsonConvert.SerializeObject(ex).AddToLog();
            //if (ex.Message.Contains("UserProfile") || ex.Message.Contains("u.Photo"))
            //{
            //    JsonConvert.SerializeObject(ex).AddToLog();
            //}
            //else
            //{
            //    ex.Message.AddToLog();
            //}
            //StackFrame frame = new(1);
            //System.Reflection.MethodBase? method = frame.GetMethod();
            //Type? type = method?.DeclaringType;
            //string? name = method?.Name;
            //if (AppConfiguration.CatchFullException)
            //{
            //    JsonConvert.SerializeObject(new
            //    {
            //        method,
            //        type,
            //        name,
            //        fullError = JsonConvert.SerializeObject(ex)
            //    }).AddToLog();
            //}
            //else
            //{
            //    JsonConvert.SerializeObject(new
            //    {
            //        method,
            //        type,
            //        name,
            //        error = ex.Message
            //    }).AddToLog();
            //}
        }
    }
}
