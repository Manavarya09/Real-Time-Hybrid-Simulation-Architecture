using System.Text.Json.Serialization;
using NeuroCity.Server.Entities;

namespace NeuroCity.Server.Economy;

public enum ConstructionState
{
    None,
    Planning,
    Constructing,
    Completed,
    Upgrading,
    Demolishing
}

public class ConstructionProject
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("buildingType")]
    public string BuildingType { get; set; } = string.Empty;

    [JsonPropertyName("x")]
    public float X { get; set; }

    [JsonPropertyName("z")]
    public float Z { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; } = "Planning";

    [JsonPropertyName("progress")]
    public float Progress { get; set; }

    [JsonPropertyName("cost")]
    public float Cost { get; set; }

    [JsonPropertyName("constructionTime")]
    public float ConstructionTime { get; set; }

    [JsonPropertyName("elapsedTime")]
    public float ElapsedTime { get; set; }

    [JsonPropertyName("workers")]
    public int Workers { get; set; }
}

public class ConstructionSystem
{
    private readonly EconomySystem _economySystem;
    private readonly List<ConstructionProject> _projects = new();
    private readonly Dictionary<string, BuildingTemplate> _buildingTemplates = new();
    private readonly Random _random = new();

    private const float BaseConstructionTime = 10f;

    public IReadOnlyList<ConstructionProject> Projects => _projects;

    public ConstructionSystem(EconomySystem economySystem)
    {
        _economySystem = economySystem;
        InitializeTemplates();
    }

    private void InitializeTemplates()
    {
        _buildingTemplates["residential"] = new BuildingTemplate
        {
            Type = "residential",
            DisplayName = "Residential Building",
            Cost = 5000,
            ConstructionTime = 15f,
            Width = 8,
            Depth = 8,
            BaseHeight = 15f,
            Color = "#4A90A4"
        };

        _buildingTemplates["commercial"] = new BuildingTemplate
        {
            Type = "commercial",
            DisplayName = "Commercial Building",
            Cost = 15000,
            ConstructionTime = 25f,
            Width = 12,
            Depth = 12,
            BaseHeight = 25f,
            Color = "#7B8FA1"
        };

        _buildingTemplates["industrial"] = new BuildingTemplate
        {
            Type = "industrial",
            DisplayName = "Industrial Facility",
            Cost = 25000,
            ConstructionTime = 35f,
            Width = 20,
            Depth = 20,
            BaseHeight = 12f,
            Color = "#A67C52"
        };

        _buildingTemplates["skyscraper"] = new BuildingTemplate
        {
            Type = "skyscraper",
            DisplayName = "Skyscraper",
            Cost = 100000,
            ConstructionTime = 60f,
            Width = 10,
            Depth = 10,
            BaseHeight = 60f,
            Color = "#5D6D7E"
        };

        _buildingTemplates["powerplant"] = new BuildingTemplate
        {
            Type = "powerplant",
            DisplayName = "Power Plant",
            Cost = 50000,
            ConstructionTime = 45f,
            Width = 25,
            Depth = 25,
            BaseHeight = 20f,
            Color = "#FF6B6B"
        };

        _buildingTemplates["waterplant"] = new BuildingTemplate
        {
            Type = "waterplant",
            DisplayName = "Water Treatment Plant",
            Cost = 30000,
            ConstructionTime = 35f,
            Width = 20,
            Depth = 20,
            BaseHeight = 15f,
            Color = "#4ECDC4"
        };

        _buildingTemplates["farm"] = new BuildingTemplate
        {
            Type = "farm",
            DisplayName = "Farm",
            Cost = 20000,
            ConstructionTime = 30f,
            Width = 30,
            Depth = 30,
            BaseHeight = 5f,
            Color = "#A8E6CF"
        };

        _buildingTemplates["hospital"] = new BuildingTemplate
        {
            Type = "hospital",
            DisplayName = "Hospital",
            Cost = 80000,
            ConstructionTime = 50f,
            Width = 20,
            Depth = 25,
            BaseHeight = 30f,
            Color = "#FF8A80"
        };

        _buildingTemplates["school"] = new BuildingTemplate
        {
            Type = "school",
            DisplayName = "School",
            Cost = 40000,
            ConstructionTime = 40f,
            Width = 18,
            Depth = 20,
            BaseHeight = 12f,
            Color = "#FFD54F"
        };

        _buildingTemplates["park"] = new BuildingTemplate
        {
            Type = "park",
            DisplayName = "Park",
            Cost = 10000,
            ConstructionTime = 20f,
            Width = 15,
            Depth = 15,
            BaseHeight = 2f,
            Color = "#69F0AE"
        };

        _buildingTemplates["stadium"] = new BuildingTemplate
        {
            Type = "stadium",
            DisplayName = "Stadium",
            Cost = 150000,
            ConstructionTime = 80f,
            Width = 50,
            Depth = 40,
            BaseHeight = 25f,
            Color = "#7C4DFF"
        };
    }

    public void Initialize()
    {
        Console.WriteLine($"[ConstructionSystem] Initialized with {_buildingTemplates.Count} building types");
    }

    public List<BuildingTemplate> GetAvailableBuildings()
    {
        return _buildingTemplates.Values.ToList();
    }

    public BuildingTemplate? GetTemplate(string type)
    {
        return _buildingTemplates.TryGetValue(type.ToLower(), out var template) ? template : null;
    }

    public bool CanBuild(string type, float money)
    {
        var template = GetTemplate(type);
        if (template == null) return false;
        return money >= template.Cost;
    }

    public ConstructionProject? StartConstruction(string type, float x, float z)
    {
        var template = GetTemplate(type);
        if (template == null)
        {
            Console.WriteLine($"[ConstructionSystem] Unknown building type: {type}");
            return null;
        }

        if (!_economySystem.Resources.Money.CanAfford(template.Cost))
        {
            Console.WriteLine($"[ConstructionSystem] Insufficient funds for {type}");
            return null;
        }

        _economySystem.Resources.Money.Spend(template.Cost);

        var project = new ConstructionProject
        {
            Id = Guid.NewGuid().ToString(),
            BuildingType = type,
            X = x,
            Z = z,
            State = "Constructing",
            Cost = template.Cost,
            ConstructionTime = template.ConstructionTime,
            Progress = 0,
            Workers = _random.Next(5, 20)
        };

        _projects.Add(project);
        Console.WriteLine($"[ConstructionSystem] Started construction: {template.DisplayName} at ({x}, {z})");

        return project;
    }

    public void Update(float deltaTime)
    {
        var completedProjects = new List<ConstructionProject>();

        foreach (var project in _projects)
        {
            if (project.State != "Constructing") continue;

            var progressRate = project.Workers / 10f;
            project.ElapsedTime += deltaTime * progressRate;
            project.Progress = Math.Min(1f, project.ElapsedTime / project.ConstructionTime);

            if (project.Progress >= 1f)
            {
                project.State = "Completed";
                completedProjects.Add(project);
            }
        }

        foreach (var project in completedProjects)
        {
            CompleteConstruction(project);
        }
    }

    private void CompleteConstruction(ConstructionProject project)
    {
        var template = GetTemplate(project.BuildingType);
        if (template == null) return;

        var building = new Building
        {
            Id = Guid.NewGuid().ToString(),
            Position = new Vector3(project.X, template.BaseHeight / 2f, project.Z),
            Type = project.BuildingType,
            Height = template.BaseHeight,
            Width = template.Width,
            Depth = template.Depth,
            Color = template.Color
        };

        _economySystem.BuildBuilding(project.BuildingType);
        
        _projects.Remove(project);
        
        Console.WriteLine($"[ConstructionSystem] Completed: {template.DisplayName}");
    }

    public bool DemolishBuilding(string buildingId, List<Building> buildings, out float refund)
    {
        refund = 0;
        var building = buildings.FirstOrDefault(b => b.Id == buildingId);
        if (building == null) return false;

        var template = GetTemplate(building.Type);
        refund = template != null ? template.Cost * 0.7f : 1000f;

        _economySystem.Resources.Money.Add(refund);
        buildings.Remove(building);
        
        Console.WriteLine($"[ConstructionSystem] Demolished {building.Type}, refund: {refund:C}");
        return true;
    }

    public void CancelProject(string projectId)
    {
        var project = _projects.FirstOrDefault(p => p.Id == projectId);
        if (project == null) return;

        var refund = project.Cost * 0.5f;
        _economySystem.Resources.Money.Add(refund);
        _projects.Remove(project);
        
        Console.WriteLine($"[ConstructionSystem] Cancelled project, refund: {refund:C}");
    }

    public void Shutdown()
    {
        _projects.Clear();
        Console.WriteLine("[ConstructionSystem] Shutdown");
    }
}

public class BuildingTemplate
{
    public string Type { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public float Cost { get; set; }
    public float ConstructionTime { get; set; }
    public float Width { get; set; }
    public float Depth { get; set; }
    public float BaseHeight { get; set; }
    public string Color { get; set; } = "#888888";
}
