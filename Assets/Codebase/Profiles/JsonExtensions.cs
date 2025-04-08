using Newtonsoft.Json;

namespace Extensions
{
    public static class JsonExtensions
    {
        public static string ToJson(this object obj)
        {
            var settings = new JsonSerializerSettings();
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            
            return JsonConvert.SerializeObject(obj, settings);
        }

        public static T ToDeserialized<T>(this string json) =>
            JsonConvert.DeserializeObject<T>(json);
    }
}
