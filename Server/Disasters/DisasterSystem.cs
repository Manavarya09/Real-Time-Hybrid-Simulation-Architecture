using System.Text.Json.Serialization;
using NeuroCity.Server.Entities;

namespace NeuroCity.Server.Disasters;

public enum DisasterType
{
    Fire,
    Earthquake,
    Flood,
    Tornado,
    Plague,
    ZombieOutbreak,
    MeteorStrike,
    PowerOutage,
    WaterShortage
}

public enum DisasterState
{
    Inactive,
    Warning,
    Active,
    Subsiding,
    Recovering
}

public class DisasterEffect
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("x")]
    public float X { get; set; }

    [JsonPropertyName("z")]
    public float Z { get; set; }

    [JsonPropertyName("radius")]
    public float Radius { get; set; }

    [JsonPropertyName("intensity")]
    public float Intensity { get; set; }

    [JsonPropertyName("duration")]
    public float Duration { get; set; }

    [JsonPropertyName("elapsed")]
    public float Elapsed { get; set; }
}

public class Disaster
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("state")]
    public string State { get; set; } = "Inactive";

    [JsonPropertyName("x")]
    public float X { get; set; }

    [JsonPropertyName("z")]
    public float Z { get; set; }

    [JsonPropertyName("radius")]
    public float Radius { get; set; }

    [JsonPropertyName("intensity")]
    public float Intensity { get; set; }

    [JsonPropertyName("warningDuration")]
    public float WarningDuration { get; set; }

    [JsonPropertyName("activeDuration")]
    public float ActiveDuration { get; set; }

    [JsonPropertyName("elapsedTime")]
    public float ElapsedTime { get; set; }

    [JsonPropertyName("affectedBuildings")]
    public List<string> AffectedBuildings { get; set; } = new();

    [JsonPropertyName("damageMultiplier")]
    public float DamageMultiplier { get; set; } = 1.0f;

    [JsonPropertyName("economicImpact")]
    public float EconomicImpact { get; set; }

    public bool IsActive => State == "Active";
}

public class DisasterSystem
{
    private readonly List<Disaster> _activeDisasters = new();
    private readonly List<Disaster> _disasterHistory = new();
    private readonly Random _random = new();
    private float _disasterTimer;
    private float _nextDisasterTime;
    private bool _disastersEnabled = true;
    private float _difficultyMultiplier = 1.0f;

    public IReadOnlyList<Disaster> ActiveDisasters => _activeDisasters;
    public bool DisastersEnabled => _disastersEnabled;

    public void Initialize()
    {
        SetNextDisasterTime();
        Console.WriteLine($"[DisasterSystem] Initialized - Next disaster in ~{_nextDisasterTime:F0} seconds");
    }

    private void SetNextDisasterTime()
    {
        _nextDisasterTime = 120f + _random.Next(180);
    }

    public void SetDifficulty(float multiplier)
    {
        _difficultyMultiplier = Math.Clamp(multiplier, 0.5f, 3.0f);
        Console.WriteLine($"[DisasterSystem] Difficulty set to {_difficultyMultiplier:F1}x");
    }

    public void Update(float deltaTime, List<Building> buildings)
    {
        if (!_disastersEnabled) return;

        _disasterTimer += deltaTime;

        foreach (var disaster in _activeDisasters.ToList())
        {
            disaster.ElapsedTime += deltaTime;
            UpdateDisaster(disaster, buildings);

            if (disaster.State == "Recovering" && disaster.ElapsedTime >= disaster.ActiveDuration)
            {
                EndDisaster(disaster);
            }
        }

        if (_disasterTimer >= _nextDisasterTime)
        {
            _disasterTimer = 0;
            TriggerRandomDisaster(buildings);
            SetNextDisasterTime();
        }
    }

    private void UpdateDisaster(Disaster disaster, List<Building> buildings)
    {
        if (disaster.State == "Warning" && disaster.ElapsedTime >= disaster.WarningDuration)
        {
            disaster.State = "Active";
            disaster.ElapsedTime = 0;
            Console.WriteLine($"[DisasterSystem] {disaster.Type} is now ACTIVE at ({disaster.X}, {disaster.Z})");
        }
        else if (disaster.State == "Active" && disaster.ElapsedTime >= disaster.ActiveDuration)
        {
            disaster.State = "Subsiding";
            Console.WriteLine($"[DisasterSystem] {disaster.Type} is subsiding");
        }
        else if (disaster.State == "Subsiding" && disaster.ElapsedTime >= 10f)
        {
            disaster.State = "Recovering";
        }

        if (disaster.State == "Active")
        {
            ApplyDisasterEffects(disaster, buildings);
        }
    }

    private void ApplyDisasterEffects(Disaster disaster, List<Building> buildings)
    {
        foreach (var building in buildings)
        {
            if (disaster.AffectedBuildings.Contains(building.Id)) continue;

            var dx = building.Position.X - disaster.X;
            var dz = building.Position.Z - disaster.Z;
            var distance = MathF.Sqrt(dx * dx + dz * dz);

            if (distance <= disaster.Radius)
            {
                disaster.AffectedBuildings.Add(building.Id);

                var damage = CalculateDamage(disaster, distance);
                ApplyBuildingDamage(building, damage);
            }
        }
    }

    private float CalculateDamage(Disaster disaster, float distance)
    {
        var baseDamage = disaster.Intensity * disaster.DamageMultiplier;
        var distanceFactor = 1f - (distance / disaster.Radius);
        return baseDamage * distanceFactor;
    }

    private void ApplyBuildingDamage(Building building, float damage)
    {
        Console.WriteLine($"[DisasterSystem] Building {building.Id} took {damage:F1} damage from {disaster.Type}");

        switch (building.Type)
        {
            case "residential":
                building.Height *= (1 - damage * 0.01f);
                break;
            case "commercial":
                building.Height *= (1 - damage * 0.008f);
                break;
            case "industrial":
                building.Height *= (1 - damage * 0.015f);
                break;
        }

        building.Height = MathF.Max(1, building.Height);
    }

    private void TriggerRandomDisaster(List<Building> buildings)
    {
        if (buildings.Count == 0) return;

        var disasterType = (DisasterType)_random.Next(Enum.GetValues<DisasterType>().Length);
        var building = buildings[_random.Next(buildings.Count)];

        var disaster = CreateDisaster(disasterType, building.Position.X, building.Position.Z);
        
        _activeDisasters.Add(disaster);
        
        Console.WriteLine($"[DisasterSystem] WARNING: {disaster.Type} approaching! Location: ({disaster.X:F0}, {disaster.Z:F0})");
    }

    private Disaster CreateDisaster(DisasterType type, float x, float z)
    {
        return type switch
        {
            DisasterType.Fire => new Disaster
            {
                Id = Guid.NewGuid().ToString("N")[..8],
                Type = "Fire",
                X = x,
                Z = z,
                Radius = 30f + _random.Next(20),
                Intensity = 5f + _random.Next(5),
                WarningDuration = 10f,
                ActiveDuration = 30f + _random.Next(30),
                DamageMultiplier = 1.2f * _difficultyMultiplier,
                EconomicImpact = 5000
            },
            DisasterType.Earthquake => new Disaster
            {
                Id = Guid.NewGuid().ToString("N")[..8],
                Type = "Earthquake",
                X = x,
                Z = z,
                Radius = 100f + _random.Next(50),
                Intensity = 8f + _random.Next(7),
                WarningDuration = 5f,
                ActiveDuration = 15f + _random.Next(10),
                DamageMultiplier = 2.0f * _difficultyMultiplier,
                EconomicImpact = 25000
            },
            DisasterType.Flood => new Disaster
            {
                Id = Guid.NewGuid().ToString("N")[..8],
                Type = "Flood",
                X = x,
                Z = z,
                Radius = 60f + _random.Next(40),
                Intensity = 4f + _random.Next(4),
                WarningDuration = 20f,
                ActiveDuration = 45f + _random.Next(30),
                DamageMultiplier = 1.5f * _difficultyMultiplier,
                EconomicImpact = 15000
            },
            DisasterType.Tornado => new Disaster
            {
                Id = Guid.NewGuid().ToString("N")[..8],
                Type = "Tornado",
                X = x,
                Z = z,
                Radius = 25f + _random.Next(15),
                Intensity = 10f + _random.Next(10),
                WarningDuration = 8f,
                ActiveDuration = 20f + _random.Next(15),
                DamageMultiplier = 2.5f * _difficultyMultiplier,
                EconomicImpact = 30000
            },
            DisasterType.MeteorStrike => new Disaster
            {
                Id = Guid.NewGuid().ToString("N")[..8],
                Type = "Meteor Strike",
                X = x,
                Z = z,
                Radius = 50f,
                Intensity = 20f,
                WarningDuration = 3f,
                ActiveDuration = 5f,
                DamageMultiplier = 3.0f * _difficultyMultiplier,
                EconomicImpact = 100000
            },
            _ => new Disaster
            {
                Id = Guid.NewGuid().ToString("N")[..8],
                Type = type.ToString(),
                X = x,
                Z = z,
                Radius = 40f,
                Intensity = 5f,
                WarningDuration = 15f,
                ActiveDuration = 30f,
                DamageMultiplier = 1.0f * _difficultyMultiplier,
                EconomicImpact = 5000
            }
        };
    }

    private void EndDisaster(Disaster disaster)
    {
        disaster.State = "Recovering";
        disaster.ElapsedTime = 0;
        
        _disasterHistory.Add(disaster);
        _activeDisasters.Remove(disaster);
        
        Console.WriteLine($"[DisasterSystem] {disaster.Type} ended. Affected {disaster.AffectedBuildings.Count} buildings. Economic impact: ${disaster.EconomicImpact:F0}");
    }

    public void TriggerSpecificDisaster(DisasterType type, float x, float z)
    {
        var disaster = CreateDisaster(type, x, z);
        disaster.State = "Warning";
        _activeDisasters.Add(disaster);
        
        Console.WriteLine($"[DisasterSystem] Manually triggered {type} at ({x}, {z})");
    }

    public void SetDisastersEnabled(bool enabled)
    {
        _disastersEnabled = enabled;
        
        if (!enabled)
        {
            foreach (var disaster in _activeDisasters)
            {
                disaster.State = "Recovering";
            }
            _activeDisasters.Clear();
        }
        
        Console.WriteLine($"[DisasterSystem] Disasters {(enabled ? "enabled" : "disabled")}");
    }

    public Dictionary<string, object> GetDisasterStats()
    {
        return new Dictionary<string, object>
        {
            ["activeCount"] = _activeDisasters.Count,
            ["totalDisasters"] = _disasterHistory.Count,
            ["totalEconomicLoss"] = _disasterHistory.Sum(d => d.EconomicImpact),
            ["mostCommonType"] = _disasterHistory.GroupBy(d => d.Type).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key ?? "None"
        };
    }

    public void Shutdown()
    {
        var totalLoss = _disasterHistory.Sum(d => d.EconomicImpact);
        Console.WriteLine($"[DisasterSystem] Shutdown - Total disasters: {_disasterHistory.Count}, Total economic loss: ${totalLoss:F0}");
    }
}
