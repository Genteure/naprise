using System.Text.Json;
using System.Text.Json.Serialization;

namespace Naprise
{
    internal static class SharedJsonOptions
    {
        internal static readonly JsonSerializerOptions SnakeCaseNamingOptions = new()
        {
            PropertyNamingPolicy = Json.JsonSnakeCaseNamingPolicy.Instance,
        };

        internal static readonly JsonSerializerOptions SnakeCaseNamingIngoreNullOptions = new()
        {
            PropertyNamingPolicy = Json.JsonSnakeCaseNamingPolicy.Instance,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
    }
}
