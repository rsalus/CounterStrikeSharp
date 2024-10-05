using System.Text.Json.Serialization;

namespace CounterStrikeSharp.API.Core.Model
{
    public class LoadedGameData
    {
        [JsonPropertyName("signatures")] public Signatures? Signatures { get; set; }
        [JsonPropertyName("offsets")] public Offsets? Offsets { get; set; }
    }
}
