using System.Text.Json.Serialization;

namespace CounterStrikeSharp.API.Core.Model
{
    public class Signatures
    {
        [JsonPropertyName("library")] public required string Library { get; set; }

        [JsonPropertyName("windows")] public required string Windows { get; set; }

        [JsonPropertyName("linux")] public required string Linux { get; set; }
    }
}
