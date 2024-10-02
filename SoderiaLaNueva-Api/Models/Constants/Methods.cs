using Newtonsoft.Json;

namespace SoderiaLaNueva_Api.Models.Constants
{
    public static class Methods
    {
        public static T Clone<T>(this T source)
        {
            if (source is null)
                return default;

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                MaxDepth = 128
            };

            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source, Formatting.None, settings), settings);
        }

        public static string Serialize<T>(this T source)
        {
            if (source is null)
            {
                return string.Empty;
            }

            JsonSerializerSettings settings = new()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                MaxDepth = 128
            };
            return JsonConvert.SerializeObject(source, Formatting.None, settings);
        }

        public static T Deserialize<T>(this string source)
        {
            if (source is null)
                return default;

            JsonSerializerSettings settings = new()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                MaxDepth = 128
            };
            return JsonConvert.DeserializeObject<T>(source.ToString(), settings);
        }
    }
}
