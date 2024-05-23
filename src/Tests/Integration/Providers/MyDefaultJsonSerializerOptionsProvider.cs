using Questrade.Library.PubSubClientHelper.Primitives;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Integration.Providers;

internal class MyDefaultJsonSerializerOptionsProvider : IJsonSerializerOptionsProvider
{
    public virtual JsonSerializerOptions GetJsonSerializerOptions()
    {
        var settings = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        return settings;
    }
}
