using System.Text.Json;
using System.Text.Json.Serialization;
using Questrade.Library.PubSubClientHelper.Primitives;

namespace Integration.Providers;

public class PubSubJsonSerializerOptions<TMessage> : IJsonSerializerOptionsProvider<TMessage>
    where TMessage : class, new()
{
    public JsonSerializerOptions GetJsonSerializerOptions()
    {
        return new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };
    }
}
