using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Infra.Utils;

public static class JsonUtils
{
    public static string Serialize<T>(T message)
    {
        return JsonConvert.SerializeObject(message);
    }

    public static T? Deserialize<T>(string message)
    {
        return JsonConvert.DeserializeObject<T>(message);
    }

    public static string? MergeJson(string jsonValue1, string jsonValue2)
    {
        if (!IsValidJson(jsonValue1) || !IsValidJson(jsonValue2))
        {
            return null;
        }

        var jsonObject1 = JObject.Parse(jsonValue1);
        var jsonObject2 = JObject.Parse(jsonValue2);

        jsonObject1.Merge(jsonObject2, new JsonMergeSettings()
        {
            MergeArrayHandling = MergeArrayHandling.Union
        });

        return jsonObject1.ToString();
    }

    private static bool IsValidJson(string jsonValue)
    {
        if (string.IsNullOrEmpty(jsonValue))
        {
            return false;
        }

        try
        {
            JToken.Parse(jsonValue);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
