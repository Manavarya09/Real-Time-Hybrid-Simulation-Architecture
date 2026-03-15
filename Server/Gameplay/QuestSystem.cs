using System.Text.Json.Serialization;

namespace NeuroCity.Server.Gameplay;

public enum QuestType
{
    Building,
    Population,
    Economy,
    Exploration,
    Discovery,
    TimeBased,
    Collection
}

public enum QuestStatus
{
    Locked,
    Available,
    InProgress,
    Completed,
    Failed
}

public class QuestObjective
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("targetValue")]
    public int TargetValue { get; set; }

    [JsonPropertyName("currentValue")]
    public int CurrentValue { get; set; }

    [JsonPropertyName("isComplete")]
    public bool IsComplete { get; set; }
}

public class Quest
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = "Building";

    [JsonPropertyName("status")]
    public string Status { get; set; } = "Locked";

    [JsonPropertyName("objectives")]
    public List<QuestObjective> Objectives { get; set; } = new();

    [JsonPropertyName("reward")]
    public QuestReward Reward { get; set; } = new();

    [JsonPropertyName("unlockRequirement")]
    public string UnlockRequirement { get; set; } = string.Empty;

    [JsonPropertyName("timeLimit")]
    public float TimeLimit { get; set; }

    [JsonPropertyName("elapsedTime")]
    public float ElapsedTime { get; set; }

    public bool IsComplete => Objectives.All(o => o.IsComplete);

    public float Progress => Objectives.Count > 0 
        ? Objectives.Average(o => (float)o.CurrentValue / o.TargetValue) 
        : 0;
}

public class QuestReward
{
    [JsonPropertyName("money")]
    public int Money { get; set; }

    [JsonPropertyName("experience")]
    public int Experience { get; set; }

    [JsonPropertyName("unlocks")]
    public List<string> Unlocks { get; set; } = new();
}

public class Achievement
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("icon")]
    public string Icon { get; set; } = "🏆";

    [JsonPropertyName("isUnlocked")]
    public bool IsUnlocked { get; set; }

    [JsonPropertyName("unlockedAt")]
    public long UnlockedAt { get; set; }

    [JsonPropertyName("requirement")]
    public string Requirement { get; set; } = string.Empty;

    [JsonPropertyName("progress")]
    public float Progress { get; set; }

    [JsonPropertyName("targetValue")]
    public float TargetValue { get; set; }

    [JsonPropertyName("reward")]
    public QuestReward Reward { get; set; } = new();
}

public class GameplayStats
{
    [JsonPropertyName("totalBuildingsBuilt")]
    public int TotalBuildingsBuilt { get; set; }

    [JsonPropertyName("totalBuildingsDestroyed")]
    public int TotalBuildingsDestroyed { get; set; }

    [JsonPropertyName("totalDistanceTraveled")]
    public float TotalDistanceTraveled { get; set; }

    [JsonPropertyName("totalMoneyEarned")]
    public float TotalMoneyEarned { get; set; }

    [JsonPropertyName("totalMoneySpent")]
    public float TotalMoneySpent { get; set; }

    [JsonPropertyName("peakPopulation")]
    public int PeakPopulation { get; set; }

    [JsonPropertyName("daysSurvived")]
    public int DaysSurvived { get; set; }

    [JsonPropertyName("timePlayed")]
    public float TimePlayed { get; set; }

    [JsonPropertyName("questsCompleted")]
    public int QuestsCompleted { get; set; }

    [JsonPropertyName("achievementsUnlocked")]
    public int AchievementsUnlocked { get; set; }
}

public class QuestSystem
{
    private readonly List<Quest> _quests = new();
    private readonly List<Achievement> _achievements = new();
    private readonly GameplayStats _stats = new();
    private readonly Dictionary<string, int> _counters = new();
    private int _questIdCounter = 1;

    public IReadOnlyList<Quest> Quests => _quests;
    public IReadOnlyList<Achievement> Achievements => _achievements;
    public GameplayStats Stats => _stats;

    public void Initialize()
    {
        InitializeQuests();
        InitializeAchievements();
        Console.WriteLine($"[QuestSystem] Initialized with {_quests.Count} quests and {_achievements.Count} achievements");
    }

    private void InitializeQuests()
    {
        _quests.Add(new Quest
        {
            Id = "q1",
            Name = "First Steps",
            Description = "Build your first residential building",
            Type = "Building",
            UnlockRequirement = "none",
            Objectives = new List<QuestObjective>
            {
                new() { Id = "q1o1", Description = "Build a residential building", TargetValue = 1 }
            },
            Reward = new QuestReward { Money = 1000, Experience = 50 }
        });

        _quests.Add(new Quest
        {
            Id = "q2",
            Name = "Growing City",
            Description = "Reach a population of 100",
            Type = "Population",
            UnlockRequirement = "q1",
            Objectives = new List<QuestObjective>
            {
                new() { Id = "q2o1", Description = "Reach 100 population", TargetValue = 100 }
            },
            Reward = new QuestReward { Money = 5000, Experience = 200 }
        });

        _quests.Add(new Quest
        {
            Id = "q3",
            Name = "Power Up",
            Description = "Build a power plant and connect it",
            Type = "Building",
            UnlockRequirement = "q2",
            Objectives = new List<QuestObjective>
            {
                new() { Id = "q3o1", Description = "Build a power plant", TargetValue = 1 }
            },
            Reward = new QuestReward { Money = 10000, Experience = 500 }
        });

        _quests.Add(new Quest
        {
            Id = "q4",
            Name = "Economic Boom",
            Description = "Earn $100,000 in total income",
            Type = "Economy",
            UnlockRequirement = "q3",
            Objectives = new List<QuestObjective>
            {
                new() { Id = "q4o1", Description = "Earn $100,000", TargetValue = 100000 }
            },
            Reward = new QuestReward { Money = 25000, Experience = 1000 }
        });

        _quests.Add(new Quest
        {
            Id = "q5",
            Name = "Urban Planner",
            Description = "Build 50 buildings of different types",
            Type = "Building",
            UnlockRequirement = "q4",
            Objectives = new List<QuestObjective>
            {
                new() { Id = "q5o1", Description = "Build residential buildings", TargetValue = 20 },
                new() { Id = "q5o2", Description = "Build commercial buildings", TargetValue = 15 },
                new() { Id = "q5o3", Description = "Build industrial buildings", TargetValue = 15 }
            },
            Reward = new QuestReward { Money = 50000, Experience = 2500, Unlocks = new List<string> { "premium_buildings" } }
        });
    }

    private void InitializeAchievements()
    {
        var achievements = new List<Achievement>
        {
            new() { Id = "a1", Name = "First Building", Description = "Build your first building", Requirement = "buildings>=1", TargetValue = 1 },
            new() { Id = "a2", Name = "Construction Crew", Description = "Build 10 buildings", Requirement = "buildings>=10", TargetValue = 10 },
            new() { Id = "a3", Name = "Master Builder", Description = "Build 100 buildings", Requirement = "buildings>=100", TargetValue = 100 },
            new() { Id = "a4", Name = "Small Town", Description = "Reach 50 population", Requirement = "population>=50", TargetValue = 50 },
            new() { Id = "a5", Name = "Growing City", Description = "Reach 500 population", Requirement = "population>=500", TargetValue = 500 },
            new() { Id = "a6", Name = "Metropolis", Description = "Reach 2000 population", Requirement = "population>=2000", TargetValue = 2000 },
            new() { Id = "a7", Name = "Wealthy", Description = "Accumulate $50,000", Requirement = "money>=50000", TargetValue = 50000 },
            new() { Id = "a8", Name = "Millionaire", Description = "Accumulate $500,000", Requirement = "money>=500000", TargetValue = 500000 },
            new() { Id = "a9", Name = "First Day", Description = "Play for 1 day in-game", Requirement = "time>=1440", TargetValue = 1440 },
            new() { Id = "a10", Name = "Dedicated Mayor", Description = "Play for 30 days in-game", Requirement = "time>=43200", TargetValue = 43200 },
            new() { Id = "a11", Name = "Explorer", Description = "Travel 10km", Requirement = "distance>=10000", TargetValue = 10000 },
            new() { Id = "a12", Name = "Road Trip", Description = "Travel 100km", Requirement = "distance>=100000", TargetValue = 100000 },
            new() { Id = "a13", Name = "Powerful", Description = "Build a power plant", Requirement = "powerplant>=1", TargetValue = 1 },
            new() { Id = "a14", Name = "Self-Sufficient", Description = "Build all resource buildings", Requirement = "resources>=1", TargetValue = 1 },
            new() { Id = "a15", Name = "Quest Master", Description = "Complete 5 quests", Requirement = "quests>=5", TargetValue = 5 }
        };

        _achievements.AddRange(achievements);
    }

    public void Update(float deltaTime, int population, float money, int buildings)
    {
        _stats.TimePlayed += deltaTime;
        
        if (population > _stats.PeakPopulation)
            _stats.PeakPopulation = population;

        UpdateQuests(population, money, buildings);
        UpdateAchievements(population, money);
    }

    private void UpdateQuests(int population, float money, int buildings)
    {
        foreach (var quest in _quests.Where(q => q.Status == "InProgress"))
        {
            quest.ElapsedTime += 1/20f;

            foreach (var obj in quest.Objectives)
            {
                switch (obj.Id)
                {
                    case "q1o1":
                        obj.CurrentValue = Math.Min(obj.TargetValue, buildings);
                        break;
                    case "q2o1":
                        obj.CurrentValue = Math.Min(obj.TargetValue, population);
                        break;
                    case "q4o1":
                        obj.CurrentValue = (int)Math.Min(obj.TargetValue, _stats.TotalMoneyEarned);
                        break;
                    case "q5o1":
                    case "q5o2":
                    case "q5o3":
                        obj.CurrentValue = Math.Min(obj.TargetValue, buildings);
                        break;
                }

                obj.IsComplete = obj.CurrentValue >= obj.TargetValue;
            }

            if (quest.IsComplete)
            {
                quest.Status = "Completed";
                _stats.QuestsCompleted++;
                CompleteQuest(quest);
            }

            if (quest.TimeLimit > 0 && quest.ElapsedTime >= quest.TimeLimit)
            {
                quest.Status = "Failed";
            }
        }
    }

    private void UpdateAchievements(int population, float money)
    {
        _counters["population"] = population;
        _counters["money"] = (int)money;
        _counters["buildings"] = _stats.TotalBuildingsBuilt;

        foreach (var achievement in _achievements.Where(a => !a.IsUnlocked))
        {
            var req = achievement.Requirement;
            float currentValue = 0;

            if (req.StartsWith("population>="))
                currentValue = _counters.GetValueOrDefault("population", 0);
            else if (req.StartsWith("money>="))
                currentValue = _counters.GetValueOrDefault("money", 0);
            else if (req.StartsWith("buildings>="))
                currentValue = _counters.GetValueOrDefault("buildings", 0);
            else if (req.StartsWith("time>="))
                currentValue = _stats.TimePlayed;

            achievement.Progress = Math.Min(1, currentValue / achievement.TargetValue);

            if (achievement.Progress >= 1)
            {
                UnlockAchievement(achievement);
            }
        }
    }

    public void CheckQuestUnlocks()
    {
        var completedQuests = _quests.Where(q => q.Status == "Completed").Select(q => q.Id).ToHashSet();

        foreach (var quest in _quests.Where(q => q.Status == "Locked"))
        {
            if (quest.UnlockRequirement == "none" || completedQuests.Contains(quest.UnlockRequirement))
            {
                quest.Status = "Available";
                Console.WriteLine($"[QuestSystem] Quest available: {quest.Name}");
            }
        }
    }

    public void StartQuest(string questId)
    {
        var quest = _quests.FirstOrDefault(q => q.Id == questId && q.Status == "Available");
        if (quest != null)
        {
            quest.Status = "InProgress";
            quest.ElapsedTime = 0;
            Console.WriteLine($"[QuestSystem] Started quest: {quest.Name}");
        }
    }

    private void CompleteQuest(Quest quest)
    {
        Console.WriteLine($"[QuestSystem] Quest completed: {quest.Name} - Reward: ${quest.Reward.Money}");
    }

    private void UnlockAchievement(Achievement achievement)
    {
        achievement.IsUnlocked = true;
        achievement.UnlockedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        _stats.AchievementsUnlocked++;
        Console.WriteLine($"[Achievement] Unlocked: {achievement.Name}");
    }

    public void RecordBuildingBuilt()
    {
        _stats.TotalBuildingsBuilt++;
    }

    public void RecordMoneyEarned(float amount)
    {
        _stats.TotalMoneyEarned += amount;
    }

    public void RecordMoneySpent(float amount)
    {
        _stats.TotalMoneySpent += amount;
    }

    public void RecordTravelDistance(float distance)
    {
        _stats.TotalDistanceTraveled += distance;
    }

    public Dictionary<string, object> GetProgress()
    {
        return new Dictionary<string, object>
        {
            ["questsCompleted"] = _stats.QuestsCompleted,
            ["achievementsUnlocked"] = _stats.AchievementsUnlocked,
            ["totalBuildings"] = _stats.TotalBuildingsBuilt,
            ["peakPopulation"] = _stats.PeakPopulation,
            ["totalMoneyEarned"] = _stats.TotalMoneyEarned
        };
    }

    public void Shutdown()
    {
        Console.WriteLine($"[QuestSystem] Final stats - Quests: {_stats.QuestsCompleted}, Achievements: {_stats.AchievementsUnlocked}");
    }
}
