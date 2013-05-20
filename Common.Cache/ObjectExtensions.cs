using Newtonsoft.Json;

namespace InMemoryCache
{
    public static class ObjectExtensions
    {
        public static T GetDeepCopy<T>(this T @this)
            where T : class
        {
            if (@this == null)
            {
                return @this;
            }

            var serialized = @this.GetJSON();
            return serialized.GetObject<T>();
        }

        public static string GetJSON<T>(this T @this)
            where T : class
        {
            return JsonConvert.SerializeObject(@this, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
        }

        public static T GetObject<T>(this string @this)
        {
            if (@this == null)
            {
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(@this, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
        }
    }
}