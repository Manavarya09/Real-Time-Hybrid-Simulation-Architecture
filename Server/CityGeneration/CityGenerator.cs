using NeuroCity.Server.Core;
using NeuroCity.Server.Entities;

namespace NeuroCity.Server.CityGeneration;

public class CityGenerator
{
    private readonly Random _random = new();
    private readonly string[] _buildingTypes = { "residential", "commercial", "industrial", "skyscraper" };
    private readonly string[] _buildingColors = { 
        "#4A90A4", "#7B8FA1", "#A67C52", "#8B7355", "#6B8E8E", 
        "#5D6D7E", "#7D8C8E", "#8B6969", "#6B5B4F", "#5C6B73" 
    };

    public void GenerateCity(WorldState worldState, int gridWidth, int gridHeight)
    {
        var spacing = 15f;
        var offsetX = -(gridWidth * spacing) / 2f;
        var offsetZ = -(gridHeight * spacing) / 2f;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                var building = GenerateBuilding(
                    offsetX + x * spacing,
                    offsetZ + z * spacing
                );
                
                worldState.Buildings.Add(building);
            }
        }
    }

    private Building GenerateBuilding(float x, float z)
    {
        var type = _buildingTypes[_random.Next(_buildingTypes.Length)];
        var (height, width, depth) = GetBuildingDimensions(type);
        
        height *= GetHeightVariation();

        return new Building
        {
            Id = Guid.NewGuid().ToString(),
            Position = new Vector3(x, height / 2f, z),
            Type = type,
            Height = height,
            Width = width,
            Depth = depth,
            Color = _buildingColors[_random.Next(_buildingColors.Length)]
        };
    }

    private (float height, float width, float depth) GetBuildingDimensions(string type)
    {
        return type switch
        {
            "residential" => (_random.Next(8, 20), _random.Next(4, 8), _random.Next(4, 8)),
            "commercial" => (_random.Next(15, 35), _random.Next(6, 12), _random.Next(6, 12)),
            "industrial" => (_random.Next(6, 15), _random.Next(10, 20), _random.Next(10, 20)),
            "skyscraper" => (_random.Next(40, 80), _random.Next(6, 10), _random.Next(6, 10)),
            _ => (10f, 5f, 5f)
        };
    }

    private float GetHeightVariation()
    {
        return (float)(0.8 + _random.NextDouble() * 0.4);
    }
}
