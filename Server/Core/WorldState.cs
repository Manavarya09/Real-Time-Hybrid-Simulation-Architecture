using System.Text.Json.Serialization;
using NeuroCity.Server.Entities;
using NeuroCity.Server.Traffic;

namespace NeuroCity.Server.Core;

public class WorldState
{
    [JsonPropertyName("buildings")]
    public List<Building> Buildings { get; set; } = new();

    [JsonPropertyName("cars")]
    public List<CarAgent> Cars { get; set; } = new();

    [JsonPropertyName("roads")]
    public RoadGraph? Roads { get; set; }

    [JsonPropertyName("tick")]
    public long Tick { get; set; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }
}
