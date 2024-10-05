using System.Text.Json.Serialization;

namespace CounterStrikeSharp.API.Core.Model
{
    public class Offsets
    {
        [JsonPropertyName("windows")] public int Windows { get; set; }

        [JsonPropertyName("linux")] public int Linux { get; set; }
    }
}
