using System.Text.Json;
using System.Text.Json.Serialization;
using NeuroCity.Server.Entities;
using NeuroCity.Server.Traffic;
using NeuroCity.Server.Player;
using NeuroCity.Server.Environment;
using NeuroCity.Server.Economy;
using NeuroCity.Server.Physics;

namespace NeuroCity.Server.Core;

public class WorldState
{
    [JsonPropertyName("buildings")]
    public List<Building> Buildings { get; set; } = new();

    [JsonPropertyName("cars")]
    public List<CarAgent> Cars { get; set; } = new();

    [JsonPropertyName("roads")]
    public RoadGraph? Roads { get; set; }

    [JsonPropertyName("players")]
    public List<PlayerController> Players { get; set; } = new();

    [JsonPropertyName("environment")]
    public EnvironmentState? Environment { get; set; }

    [JsonPropertyName("resources")]
    public CityResources? Resources { get; set; }

    [JsonPropertyName("tick")]
    public long Tick { get; set; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }
}

public class SaveData
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0";

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("worldName")]
    public string WorldName { get; set; } = "New City";

    [JsonPropertyName("tick")]
    public long Tick { get; set; }

    [JsonPropertyName("buildings")]
    public List<Building> Buildings { get; set; } = new();

    [JsonPropertyName("roads")]
    public RoadGraph? Roads { get; set; }

    [JsonPropertyName("environment")]
    public EnvironmentState? Environment { get; set; }

    [JsonPropertyName("resources")]
    public CityResources? Resources { get; set; }

    [JsonPropertyName("statistics")]
    public WorldStatistics Statistics { get; set; } = new();
}

public class WorldStatistics
{
    [JsonPropertyName("totalBuildings")]
    public int TotalBuildings { get; set; }

    [JsonPropertyName("totalCars")]
    public int TotalCars { get; set; }

    [JsonPropertyName("population")]
    public int Population { get; set; }

    [JsonPropertyName("daysElapsed")]
    public int DaysElapsed { get; set; }

    [JsonPropertyName("playTime")]
    public long PlayTime { get; set; }

    [JsonPropertyName("incomeRate")]
    public float IncomeRate { get; set; }
}

public class SaveLoadSystem
{
    private readonly string _saveDirectory;
    private readonly JsonSerializerOptions _jsonOptions;

    public SaveLoadSystem()
    {
        _saveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NeuroCity", "Saves");
        Directory.CreateDirectory(_saveDirectory);

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task SaveGameAsync(string fileName, WorldState worldState, WorldStatistics stats)
    {
        var saveData = new SaveData
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            WorldName = fileName.Replace(".json", ""),
            Tick = worldState.Tick,
            Buildings = worldState.Buildings,
            Roads = worldState.Roads,
            Environment = worldState.Environment,
            Resources = worldState.Resources,
            Statistics = stats
        };

        var filePath = Path.Combine(_saveDirectory, $"{saveData.WorldName}.json");
        var json = JsonSerializer.Serialize(saveData, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);

        Console.WriteLine($"[SaveLoadSystem] Saved: {filePath}");
    }

    public async Task<SaveData?> LoadGameAsync(string fileName)
    {
        var filePath = Path.Combine(_saveDirectory, $"{fileName}.json");
        
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"[SaveLoadSystem] File not found: {filePath}");
            return null;
        }

        var json = await File.ReadAllTextAsync(filePath);
        var saveData = JsonSerializer.Deserialize<SaveData>(json, _jsonOptions);

        Console.WriteLine($"[SaveLoadSystem] Loaded: {filePath}");
        return saveData;
    }

    public List<string> GetSaveFiles()
    {
        if (!Directory.Exists(_saveDirectory))
            return new List<string>();

        return Directory.GetFiles(_saveDirectory, "*.json")
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .ToList();
    }

    public async Task DeleteSaveAsync(string fileName)
    {
        var filePath = Path.Combine(_saveDirectory, $"{fileName}.json");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Console.WriteLine($"[SaveLoadSystem] Deleted: {filePath}");
        }
    }

    public SaveData? GetLatestSave()
    {
        var saves = GetSaveFiles();
        if (saves.Count == 0) return null;

        var latest = saves
            .Select(s => new { Name = s, Path = Path.Combine(_saveDirectory, $"{s}.json") })
            .OrderByDescending(s => File.GetLastWriteTime(s.Path))
            .FirstOrDefault();

        if (latest == null) return null;

        return LoadGameAsync(latest.Name).Result;
    }
}
