using System.Text.Json.Serialization;
using NeuroCity.Server.Entities;

namespace NeuroCity.Server.AI;

public enum CitizenState
{
    Idle,
    Walking,
    Working,
    Shopping,
    Resting,
    Traveling,
    Entertainment
}

public class Citizen
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("x")]
    public float X { get; set; }

    [JsonPropertyName("y")]
    public float Y { get; set; }

    [JsonPropertyName("z")]
    public float Z { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; } = "Idle";

    [JsonPropertyName("age")]
    public int Age { get; set; }

    [JsonPropertyName("happiness")]
    public float Happiness { get; set; } = 70f;

    [JsonPropertyName("energy")]
    public float Energy { get; set; } = 100f;

    [JsonPropertyName("money")]
    public float Money { get; set; }

    [JsonPropertyName("homeX")]
    public float HomeX { get; set; }

    [JsonPropertyName("homeZ")]
    public float HomeZ { get; set; }

    [JsonPropertyName("workX")]
    public float WorkX { get; set; }

    [JsonPropertyName("workZ")]
    public float WorkZ { get; set; }

    [JsonPropertyName("targetX")]
    public float TargetX { get; set; }

    [JsonPropertyName("targetZ")]
    public float TargetZ { get; set; }

    [JsonPropertyName("speed")]
    public float Speed { get; set; } = 3f;

    public Citizen()
    {
        Id = Guid.NewGuid().ToString();
    }
}

public class PopulationSystem
{
    private readonly List<Citizen> _citizens = new();
    private readonly Random _random = new();
    private readonly List<Building> _buildings;
    private readonly RoadGraph _roadGraph;
    private readonly EconomySystem _economySystem;
    private int _maxPopulation = 1000;
    private float _spawnTimer;
    private float _spawnInterval = 0.5f;

    public IReadOnlyList<Citizen> Citizens => _citizens;

    public PopulationSystem(List<Building> buildings, RoadGraph roadGraph, EconomySystem economySystem)
    {
        _buildings = buildings;
        _roadGraph = roadGraph;
        _economySystem = economySystem;
    }

    public void Initialize()
    {
        SpawnInitialPopulation();
        Console.WriteLine($"[PopulationSystem] Initialized with {_citizens.Count} citizens");
    }

    private void SpawnInitialPopulation()
    {
        var residentialBuildings = _buildings.Where(b => b.Type == "residential").ToList();
        var workBuildings = _buildings.Where(b => b.Type == "commercial" || b.Type == "industrial").ToList();

        for (int i = 0; i < 100; i++)
        {
            var citizen = CreateCitizen();
            
            if (residentialBuildings.Count > 0)
            {
                var home = residentialBuildings[_random.Next(residentialBuildings.Count)];
                citizen.HomeX = home.Position.X;
                citizen.HomeZ = home.Position.Z;
                citizen.X = citizen.HomeX;
                citizen.Z = citizen.HomeZ;
            }

            if (workBuildings.Count > 0 && _random.NextDouble() > 0.3)
            {
                var work = workBuildings[_random.Next(workBuildings.Count)];
                citizen.WorkX = work.Position.X;
                citizen.WorkZ = work.Position.Z;
            }

            _citizens.Add(citizen);
        }
    }

    private Citizen CreateCitizen()
    {
        return new Citizen
        {
            Age = _random.Next(18, 70),
            Money = _random.Next(100, 5000),
            Happiness = _random.Next(50, 90),
            Energy = 100f,
            State = "Idle"
        };
    }

    public void Update(float deltaTime)
    {
        UpdateCitizens(deltaTime);
        SpawnNewCitizens(deltaTime);
        UpdateEconomy(deltaTime);
    }

    private void UpdateCitizens(float deltaTime)
    {
        foreach (var citizen in _citizens)
        {
            UpdateCitizenBehavior(citizen, deltaTime);
        }
    }

    private void UpdateCitizenBehavior(Citizen citizen, float deltaTime)
    {
        citizen.Energy = Math.Min(100f, citizen.Energy - deltaTime * 0.5f);
        citizen.Happiness = Math.Clamp(citizen.Happiness + deltaTime * 0.1f, 0, 100);

        switch (citizen.State)
        {
            case "Idle":
                HandleIdleState(citizen, deltaTime);
                break;
            case "Walking":
            case "Traveling":
                HandleMovement(citizen, deltaTime);
                break;
            case "Working":
                HandleWorking(citizen, deltaTime);
                break;
            case "Shopping":
                HandleShopping(citizen, deltaTime);
                break;
            case "Resting":
                HandleResting(citizen, deltaTime);
                break;
            case "Entertainment":
                HandleEntertainment(citizen, deltaTime);
                break;
        }
    }

    private void HandleIdleState(Citizen citizen, float deltaTime)
    {
        var roll = _random.NextDouble();

        if (citizen.Energy < 30)
        {
            citizen.State = "Resting";
            citizen.TargetX = citizen.HomeX;
            citizen.TargetZ = citizen.HomeZ;
        }
        else if (citizen.WorkX != 0 && roll < 0.4)
        {
            citizen.State = "Working";
            citizen.TargetX = citizen.WorkX;
            citizen.TargetZ = citizen.WorkZ;
        }
        else if (roll < 0.7)
        {
            citizen.State = "Walking";
            citizen.TargetX = citizen.X + (_random.NextSingle() - 0.5f) * 100;
            citizen.TargetZ = citizen.Z + (_random.NextSingle() - 0.5f) * 100;
        }
        else if (roll < 0.85)
        {
            citizen.State = "Shopping";
        }
        else
        {
            citizen.State = "Entertainment";
        }
    }

    private void HandleMovement(Citizen citizen, float deltaTime)
    {
        var dx = citizen.TargetX - citizen.X;
        var dz = citizen.TargetZ - citizen.Z;
        var distance = MathF.Sqrt(dx * dx + dz * dz);

        if (distance < 1f)
        {
            OnDestinationReached(citizen);
            return;
        }

        var moveAmount = citizen.Speed * deltaTime;
        citizen.X += (dx / distance) * moveAmount;
        citizen.Z += (dz / distance) * moveAmount;
        citizen.Energy -= deltaTime * 2f;
    }

    private void OnDestinationReached(Citizen citizen)
    {
        switch (citizen.State)
        {
            case "Walking":
                citizen.State = "Idle";
                break;
            case "Traveling":
                citizen.State = "Idle";
                break;
            case "Working":
                citizen.State = "Working";
                citizen.Energy = Math.Min(100f, citizen.Energy + 20f);
                break;
            case "Resting":
                citizen.State = "Idle";
                citizen.Energy = 100f;
                break;
            default:
                citizen.State = "Idle";
                break;
        }
    }

    private void HandleWorking(Citizen citizen, float deltaTime)
    {
        if (_random.NextDouble() < 0.01)
        {
            citizen.Money += deltaTime * 10f;
            citizen.Energy -= deltaTime * 5f;
        }

        if (citizen.Energy < 20)
        {
            citizen.State = "Resting";
            citizen.TargetX = citizen.HomeX;
            citizen.TargetZ = citizen.HomeZ;
        }
    }

    private void HandleShopping(Citizen citizen, float deltaTime)
    {
        citizen.Money -= deltaTime * 5f;
        citizen.Happiness = Math.Min(100f, citizen.Happiness + deltaTime * 3f);

        if (citizen.Money < 10 || citizen.Happiness > 80)
        {
            citizen.State = "Idle";
        }
    }

    private void HandleResting(Citizen citizen, float deltaTime)
    {
        citizen.Energy = Math.Min(100f, citizen.Energy + deltaTime * 20f);
        citizen.Happiness = Math.Min(100f, citizen.Happiness + deltaTime * 2f);

        if (citizen.Energy > 80)
        {
            citizen.State = "Idle";
        }
    }

    private void HandleEntertainment(Citizen citizen, float deltaTime)
    {
        citizen.Money -= deltaTime * 3f;
        citizen.Happiness = Math.Min(100f, citizen.Happiness + deltaTime * 5f);

        if (citizen.Money < 10 || citizen.Happiness > 90)
        {
            citizen.State = "Idle";
        }
    }

    private void SpawnNewCitizens(float deltaTime)
    {
        if (_citizens.Count >= _maxPopulation) return;

        _spawnTimer += deltaTime;
        if (_spawnTimer >= _spawnInterval)
        {
            _spawnTimer = 0;
            
            if (_economySystem.Resources.Happiness.Amount > 50)
            {
                var spawnChance = _economySystem.Resources.Happiness.Amount / 200f;
                if (_random.NextDouble() < spawnChance)
                {
                    var citizen = CreateCitizen();
                    var residential = _buildings.Where(b => b.Type == "residential").ToList();
                    if (residential.Count > 0)
                    {
                        var home = residential[_random.Next(residential.Count)];
                        citizen.HomeX = home.Position.X;
                        citizen.HomeZ = home.Position.Z;
                        citizen.X = citizen.HomeX;
                        citizen.Z = citizen.HomeZ;
                        _citizens.Add(citizen);
                    }
                }
            }
        }
    }

    private void UpdateEconomy(float deltaTime)
    {
        var totalMoney = _citizens.Sum(c => c.Money);
        var avgHappiness = _citizens.Count > 0 ? _citizens.Average(c => c.Happiness) : 50;
        
        _economySystem.Resources.Population.Amount = _citizens.Count;
        _economySystem.Resources.Happiness.Amount = (float)avgHappiness;
    }

    public int GetPopulation() => _citizens.Count;

    public void Shutdown()
    {
        _citizens.Clear();
        Console.WriteLine("[PopulationSystem] Shutdown");
    }
}
