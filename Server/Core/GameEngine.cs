using System.Text.Json;
using NeuroCity.Server.Core;
using NeuroCity.Server.Entities;
using NeuroCity.Server.Networking;
using NeuroCity.Server.CityGeneration;
using NeuroCity.Server.Traffic;
using NeuroCity.Server.Player;
using NeuroCity.Server.Environment;
using NeuroCity.Server.Economy;
using NeuroCity.Server.Physics;

namespace NeuroCity.Server.Core;

public class GameEngine
{
    private readonly WorldState _worldState;
    private readonly WebSocketServer _webSocketServer;
    private readonly CityGenerator _cityGenerator;
    private readonly SimulationLoop _simulationLoop;
    private readonly RoadGraph _roadGraph;
    private readonly TrafficSystem _trafficSystem;
    private readonly PlayerSystem _playerSystem;
    private readonly EnvironmentSystem _environmentSystem;
    private readonly EconomySystem _economySystem;
    private readonly PhysicsWorld _physicsWorld;
    private readonly SaveLoadSystem _saveLoadSystem;

    private const float TickRate = 20f;
    private const float DeltaTime = 1f / TickRate;

    public WorldState WorldState => _worldState;
    public SimulationLoop SimulationLoop => _simulationLoop;
    public RoadGraph RoadGraph => _roadGraph;
    public PlayerSystem PlayerSystem => _playerSystem;
    public EnvironmentSystem EnvironmentSystem => _environmentSystem;
    public EconomySystem EconomySystem => _economySystem;
    public PhysicsWorld PhysicsWorld => _physicsWorld;
    public SaveLoadSystem SaveLoadSystem => _saveLoadSystem;

    public GameEngine()
    {
        _worldState = new WorldState();
        _webSocketServer = new WebSocketServer(this);
        _cityGenerator = new CityGenerator();
        _simulationLoop = new SimulationLoop(this, 20);
        _roadGraph = new RoadGraph();
        _trafficSystem = new TrafficSystem(_roadGraph, 50);
        _playerSystem = new PlayerSystem();
        _environmentSystem = new EnvironmentSystem();
        _economySystem = new EconomySystem();
        _physicsWorld = new PhysicsWorld();
        _saveLoadSystem = new SaveLoadSystem();
    }

    public async Task InitializeAsync()
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║           NeuroCity Engine v2.0 - Starting...            ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
        
        _cityGenerator.GenerateCity(_worldState, 20, 20);
        Console.WriteLine($"[GameEngine] Generated {_worldState.Buildings.Count} buildings");
        
        var spacing = 15f;
        _roadGraph.GenerateGridRoadNetwork(20f, 20, 20, spacing);
        Console.WriteLine($"[GameEngine] Generated {_roadGraph.Nodes.Count} road nodes");
        
        _worldState.Roads = _roadGraph;
        
        _trafficSystem.Initialize();
        
        _playerSystem.Initialize();
        
        _environmentSystem.Initialize();
        
        _economySystem.Initialize();
        
        _physicsWorld.Initialize();
        
        await _webSocketServer.StartAsync();
        
        _simulationLoop.Start();
        
        Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║           NeuroCity Engine v2.0 - Ready!                  ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
    }

    public void Update(long tick)
    {
        _worldState.Tick = tick;
        _worldState.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
        _environmentSystem.Update(DeltaTime);
        
        _economySystem.Update(DeltaTime);
        
        _trafficSystem.Update(DeltaTime);
        
        _playerSystem.Update(DeltaTime);
        
        _physicsWorld.Update(DeltaTime);
        
        _worldState.Cars = _trafficSystem.Cars;
        _worldState.Players = _playerSystem.GetAllPlayers();
        _worldState.Environment = _environmentSystem.State;
        _worldState.Resources = _economySystem.Resources;
        
        OnTick(tick);
    }

    private void OnTick(long tick)
    {
        BroadcastWorldState();
    }

    public async void BroadcastWorldState()
    {
        var json = JsonSerializer.Serialize(_worldState);
        await _webSocketServer.BroadcastAsync(json);
    }

    public async Task SendInitialStateAsync(string connectionId)
    {
        var json = JsonSerializer.Serialize(_worldState);
        await _webSocketServer.SendToClientAsync(connectionId, json);
    }

    public async Task SaveGameAsync(string fileName)
    {
        var stats = new WorldStatistics
        {
            TotalBuildings = _worldState.Buildings.Count,
            TotalCars = _worldState.Cars.Count,
            DaysElapsed = (int)(_worldState.Tick / (20 * 60 * 24))
        };
        
        await _saveLoadSystem.SaveGameAsync(fileName, _worldState, stats);
    }

    public async Task LoadGameAsync(string fileName)
    {
        var saveData = await _saveLoadSystem.LoadGameAsync(fileName);
        if (saveData == null) return;

        _worldState.Buildings = saveData.Buildings;
        _worldState.Roads = saveData.Roads;
        _worldState.Environment = saveData.Environment;
        
        Console.WriteLine($"[GameEngine] Loaded save: {saveData.WorldName}");
    }

    public async Task ShutdownAsync()
    {
        Console.WriteLine("[GameEngine] Shutting down...");
        _simulationLoop.Stop();
        _trafficSystem.Shutdown();
        _playerSystem.Shutdown();
        _environmentSystem.Shutdown();
        _economySystem.Shutdown();
        _physicsWorld.Shutdown();
        await _webSocketServer.StopAsync();
        Console.WriteLine("[GameEngine] Shutdown complete");
    }
}
