using System.Text.Json.Serialization;
using NeuroCity.Server.Entities;

namespace NeuroCity.Server.Traffic;

public class CarAgent
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("x")]
    public float X { get; set; }

    [JsonPropertyName("y")]
    public float Y { get; set; }

    [JsonPropertyName("z")]
    public float Z { get; set; }

    [JsonPropertyName("rotation")]
    public float Rotation { get; set; }

    [JsonPropertyName("color")]
    public string Color { get; set; } = "#ff4444";

    [JsonIgnore]
    public RoadNode? CurrentNode { get; set; }

    [JsonIgnore]
    public RoadNode? TargetNode { get; set; }

    [JsonIgnore]
    public List<RoadNode> Path { get; set; } = new();

    [JsonIgnore]
    public float Speed { get; set; }

    [JsonIgnore]
    private float _progress;

    [JsonIgnore]
    public bool IsMoving => TargetNode != null;

    private static readonly string[] CarColors = 
    { 
        "#ff6b6b", "#4ecdc4", "#45b7d1", "#f9ca24", 
        "#6c5ce7", "#fd79a8", "#00b894", "#e17055",
        "#74b9ff", "#a29bfe"
    };

    private static readonly Random _random = new();

    public CarAgent()
    {
        Id = Guid.NewGuid().ToString();
        Color = CarColors[_random.Next(CarColors.Length)];
        Speed = 8f + (float)_random.NextDouble() * 4f;
    }

    public void SpawnAt(RoadNode node)
    {
        CurrentNode = node;
        X = node.X;
        Y = 0.3f;
        Z = node.Z;
        TargetNode = null;
        Path.Clear();
        _progress = 0;
    }

    public void SetPath(List<RoadNode> path)
    {
        if (path == null || path.Count == 0) return;

        Path = path;
        CurrentNode = path[0];
        
        if (path.Count > 1)
        {
            TargetNode = path[1];
            X = CurrentNode.X;
            Z = CurrentNode.Z;
            _progress = 0;
            UpdateRotation();
        }
        else
        {
            TargetNode = null;
        }
    }

    public void Update(float deltaTime)
    {
        if (TargetNode == null || CurrentNode == null) return;

        var distance = CurrentNode.DistanceTo(TargetNode);
        if (distance < 0.01f)
        {
            ArrivedAtNode();
            return;
        }

        var moveAmount = Speed * deltaTime;
        _progress += moveAmount;

        var t = Math.Clamp(_progress / distance, 0f, 1f);
        
        X = MathF.Lerp(CurrentNode.X, TargetNode.X, t);
        Z = MathF.Lerp(CurrentNode.Z, TargetNode.Z, t);

        if (_progress >= distance)
        {
            ArrivedAtNode();
        }
    }

    private void ArrivedAtNode()
    {
        if (Path.Count == 0)
        {
            TargetNode = null;
            CurrentNode = TargetNode;
            return;
        }

        Path.RemoveAt(0);

        if (Path.Count == 0)
        {
            CurrentNode = TargetNode;
            TargetNode = null;
            return;
        }

        CurrentNode = TargetNode;
        TargetNode = Path[0];
        _progress = 0;
        UpdateRotation();
    }

    private void UpdateRotation()
    {
        if (CurrentNode == null || TargetNode == null) return;

        var dx = TargetNode.X - CurrentNode.X;
        var dz = TargetNode.Z - CurrentNode.Z;
        Rotation = MathF.Atan2(dx, dz);
    }

    public bool HasReachedDestination()
    {
        return TargetNode == null && Path.Count == 0;
    }
}
