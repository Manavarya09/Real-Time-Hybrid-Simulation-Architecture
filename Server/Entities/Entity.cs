using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeuroCity.Server.Entities;

public class Vector3
{
    [JsonPropertyName("x")]
    public float X { get; set; }
    
    [JsonPropertyName("y")]
    public float Y { get; set; }
    
    [JsonPropertyName("z")]
    public float Z { get; set; }

    public Vector3() { }

    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}

public class Entity
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("position")]
    public Vector3 Position { get; set; } = new();
}

public class Building : Entity
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "residential";

    [JsonPropertyName("height")]
    public float Height { get; set; } = 10f;

    [JsonPropertyName("color")]
    public string Color { get; set; } = "#888888";

    [JsonPropertyName("width")]
    public float Width { get; set; } = 5f;

    [JsonPropertyName("depth")]
    public float Depth { get; set; } = 5f;

    public Building()
    {
        Position = new Vector3(0, 0, 0);
    }
}
