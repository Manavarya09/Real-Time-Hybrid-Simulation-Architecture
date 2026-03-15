using System.Text.Json.Serialization;

namespace NeuroCity.Server.Economy;

public enum ResourceType
{
    Money,
    Energy,
    Water,
    Food,
    Materials,
    Population,
    Happiness
}

public class Resource
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public float Amount { get; set; }

    [JsonPropertyName("maxAmount")]
    public float MaxAmount { get; set; }

    [JsonPropertyName("productionRate")]
    public float ProductionRate { get; set; }

    [JsonPropertyName("consumptionRate")]
    public float ConsumptionRate { get; set; }

    public Resource() { }

    public Resource(string type, float amount, float maxAmount)
    {
        Type = type;
        Amount = amount;
        MaxAmount = maxAmount;
    }

    public void Produce(float deltaTime)
    {
        Amount = Math.Min(MaxAmount, Amount + ProductionRate * deltaTime);
    }

    public void Consume(float deltaTime)
    {
        Amount = Math.Max(0, Amount - ConsumptionRate * deltaTime);
    }

    public bool CanAfford(float cost) => Amount >= cost;

    public void Spend(float cost)
    {
        Amount = Math.Max(0, Amount - cost);
    }

    public void Add(float amount)
    {
        Amount = Math.Min(MaxAmount, Amount + amount);
    }
}

public class CityResources
{
    [JsonPropertyName("money")]
    public Resource Money { get; set; } = new("Money", 100000, 1000000);

    [JsonPropertyName("energy")]
    public Resource Energy { get; set; } = new("Energy", 500, 1000);

    [JsonPropertyName("water")]
    public Resource Water { get; set; } = new("Water", 500, 1000);

    [JsonPropertyName("food")]
    public Resource Food { get; set; } = new("Food", 500, 1000);

    [JsonPropertyName("materials")]
    public Resource Materials { get; set; } = new("Materials", 200, 500);

    [JsonPropertyName("population")]
    public Resource Population { get; set; } = new("Population", 0, 10000);

    [JsonPropertyName("happiness")]
    public Resource Happiness { get; set; } = new("Happiness", 70, 100);

    public Dictionary<string, Resource> GetAll()
    {
        return new Dictionary<string, Resource>
        {
            ["Money"] = Money,
            ["Energy"] = Energy,
            ["Water"] = Water,
            ["Food"] = Food,
            ["Materials"] = Materials,
            ["Population"] = Population,
            ["Happiness"] = Happiness
        };
    }
}

public enum BuildingCategory
{
    Residential,
    Commercial,
    Industrial,
    Infrastructure,
    Entertainment,
    Healthcare,
    Education,
    Government
}

public class BuildingStats
{
    [JsonPropertyName("populationCapacity")]
    public int PopulationCapacity { get; set; }

    [JsonPropertyName("energyConsumption")]
    public float EnergyConsumption { get; set; }

    [JsonPropertyName("waterConsumption")]
    public float WaterConsumption { get; set; }

    [JsonPropertyName("foodConsumption")]
    public float FoodConsumption { get; set; }

    [JsonPropertyName("income")]
    public float Income { get; set; }

    [JsonPropertyName("production")]
    public string? Production { get; set; }

    [JsonPropertyName("happinessEffect")]
    public float HappinessEffect { get; set; }

    [JsonPropertyName("constructionCost")]
    public float ConstructionCost { get; set; }

    [JsonPropertyName("maintenanceCost")]
    public float MaintenanceCost { get; set; }
}

public class EconomySystem
{
    private CityResources _resources = new();
    private readonly Dictionary<string, BuildingStats> _buildingTemplates = new();
    private readonly Random _random = new();

    public CityResources Resources => _resources;

    public EconomySystem()
    {
        InitializeBuildingTemplates();
    }

    private void InitializeBuildingTemplates()
    {
        _buildingTemplates["residential"] = new BuildingStats
        {
            PopulationCapacity = 50,
            EnergyConsumption = 5,
            WaterConsumption = 3,
            FoodConsumption = 2,
            Income = 100,
            HappinessEffect = 2,
            ConstructionCost = 5000,
            MaintenanceCost = 50
        };

        _buildingTemplates["commercial"] = new BuildingStats
        {
            EnergyConsumption = 15,
            WaterConsumption = 5,
            FoodConsumption = 0,
            Income = 500,
            Production = "Jobs",
            HappinessEffect = 1,
            ConstructionCost = 15000,
            MaintenanceCost = 150
        };

        _buildingTemplates["industrial"] = new BuildingStats
        {
            EnergyConsumption = 30,
            WaterConsumption = 20,
            FoodConsumption = 0,
            Income = 800,
            Production = "Materials",
            HappinessEffect = -5,
            ConstructionCost = 25000,
            MaintenanceCost = 200
        };

        _buildingTemplates["skyscraper"] = new BuildingStats
        {
            PopulationCapacity = 500,
            EnergyConsumption = 50,
            WaterConsumption = 30,
            FoodConsumption = 20,
            Income = 2000,
            HappinessEffect = 3,
            ConstructionCost = 100000,
            MaintenanceCost = 500
        };

        _buildingTemplates["powerplant"] = new BuildingStats
        {
            EnergyConsumption = -100,
            Income = 0,
            Production = "Energy",
            HappinessEffect = -10,
            ConstructionCost = 50000,
            MaintenanceCost = 300
        };

        _buildingTemplates["waterplant"] = new BuildingStats
        {
            WaterConsumption = -50,
            Income = 0,
            Production = "Water",
            HappinessEffect = 5,
            ConstructionCost = 30000,
            MaintenanceCost = 200
        };

        _buildingTemplates["farm"] = new BuildingStats
        {
            EnergyConsumption = 10,
            WaterConsumption = 30,
            FoodConsumption = -50,
            Income = 200,
            Production = "Food",
            HappinessEffect = 5,
            ConstructionCost = 20000,
            MaintenanceCost = 100
        };

        _buildingTemplates["hospital"] = new BuildingStats
        {
            EnergyConsumption = 25,
            WaterConsumption = 15,
            Income = 0,
            HappinessEffect = 15,
            ConstructionCost = 80000,
            MaintenanceCost = 400
        };

        _buildingTemplates["school"] = new BuildingStats
        {
            EnergyConsumption = 15,
            WaterConsumption = 10,
            Income = 0,
            HappinessEffect = 10,
            ConstructionCost = 40000,
            MaintenanceCost = 200
        };

        _buildingTemplates["park"] = new BuildingStats
        {
            EnergyConsumption = 2,
            WaterConsumption = 5,
            Income = 0,
            HappinessEffect = 20,
            ConstructionCost = 10000,
            MaintenanceCost = 50
        };

        _buildingTemplates["stadium"] = new BuildingStats
        {
            EnergyConsumption = 40,
            WaterConsumption = 20,
            Income = 1000,
            HappinessEffect = 25,
            ConstructionCost = 150000,
            MaintenanceCost = 800
        };
    }

    public void Initialize()
    {
        Console.WriteLine("[EconomySystem] Initialized with resources");
    }

    public void Update(float deltaTime)
    {
        _resources.Money.Produce(deltaTime);
        _resources.Energy.Produce(deltaTime);
        _resources.Water.Produce(deltaTime);
        _resources.Food.Produce(deltaTime);
        _resources.Materials.Produce(deltaTime);
        
        _resources.Money.Consume(deltaTime);
    }

    public BuildingStats? GetBuildingTemplate(string type)
    {
        return _buildingTemplates.TryGetValue(type.ToLower(), out var template) ? template : null;
    }

    public bool CanAffordBuilding(string type, int count = 1)
    {
        var template = GetBuildingTemplate(type);
        if (template == null) return false;
        return _resources.Money.CanAfford(template.ConstructionCost * count);
    }

    public void BuildBuilding(string type)
    {
        var template = GetBuildingTemplate(type);
        if (template == null)
        {
            Console.WriteLine($"[EconomySystem] Unknown building type: {type}");
            return;
        }

        if (!_resources.Money.CanAfford(template.ConstructionCost))
        {
            Console.WriteLine($"[EconomySystem] Not enough money to build {type}");
            return;
        }

        _resources.Money.Spend(template.ConstructionCost);
        _resources.Energy.ConsumptionRate += template.EnergyConsumption;
        _resources.Water.ConsumptionRate += template.WaterConsumption;
        _resources.Food.ConsumptionRate += template.FoodConsumption;
        _resources.Money.ProductionRate += template.Income;
        _resources.Happiness.ProductionRate += template.HappinessEffect;

        Console.WriteLine($"[EconomySystem] Built {type} - Cost: {template.ConstructionCost:C}");
    }

    public void CollectIncome(float amount)
    {
        _resources.Money.Add(amount);
    }

    public void UpdatePopulation(int population)
    {
        _resources.Population.Amount = population;
    }

    public void UpdateHappiness(float delta)
    {
        _resources.Happiness.Amount = Math.Clamp(_resources.Happiness.Amount + delta, 0, 100);
    }

    public Dictionary<string, float> GetResourceDisplay()
    {
        return new Dictionary<string, float>
        {
            ["Money"] = _resources.Money.Amount,
            ["Energy"] = _resources.Energy.Amount,
            ["Water"] = _resources.Water.Amount,
            ["Food"] = _resources.Food.Amount,
            ["Materials"] = _resources.Materials.Amount,
            ["Population"] = _resources.Population.Amount,
            ["Happiness"] = _resources.Happiness.Amount
        };
    }

    public void Shutdown()
    {
        Console.WriteLine("[EconomySystem] Shutdown");
    }
}
