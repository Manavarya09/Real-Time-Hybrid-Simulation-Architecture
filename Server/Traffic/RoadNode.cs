using System.Text.Json.Serialization;
using NeuroCity.Server.Entities;

namespace NeuroCity.Server.Traffic;

public class RoadNode
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("x")]
    public float X { get; set; }

    [JsonPropertyName("y")]
    public float Y { get; set; }

    [JsonPropertyName("z")]
    public float Z { get; set; }

    [JsonIgnore]
    public List<RoadNode> Neighbors { get; set; } = new();

    [JsonPropertyName("neighbors")]
    public List<string> NeighborIds { get; set; } = new();

    public RoadNode() { }

    public RoadNode(float x, float z)
    {
        Id = Guid.NewGuid().ToString();
        X = x;
        Y = 0.1f;
        Z = z;
    }

    public Vector3 ToVector3() => new(X, Y, Z);

    public float DistanceTo(RoadNode other)
    {
        var dx = X - other.X;
        var dz = Z - other.Z;
        return MathF.Sqrt(dx * dx + dz * dz);
    }

    public void AddNeighbor(RoadNode node)
    {
        if (!Neighbors.Contains(node))
        {
            Neighbors.Add(node);
            NeighborIds.Add(node.Id);
        }
    }
}
