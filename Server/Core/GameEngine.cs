using System.Text.Json;
using NeuroCity.Server.Core;
using NeuroCity.Server.Entities;
using NeuroCity.Server.Networking;
using NeuroCity.Server.CityGeneration;
using NeuroCity.Server.Traffic;
using NeuroCity.Server.Player;

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

    private const float TickRate = 20f;
    private const float DeltaTime = 1f / TickRate;

    public WorldState WorldState => _worldState;
    public SimulationLoop SimulationLoop => _simulationLoop;
    public RoadGraph RoadGraph => _roadGraph;
    public PlayerSystem PlayerSystem => _playerSystem;

    public GameEngine()
    {
        _worldState = new WorldState();
        _webSocketServer = new WebSocketServer(this);
        _cityGenerator = new CityGenerator();
        _simulationLoop = new SimulationLoop(this, 20);
        _roadGraph = new RoadGraph();
        _trafficSystem = new TrafficSystem(_roadGraph, 50);
        _playerSystem = new PlayerSystem();
    }

    public async Task InitializeAsync()
    {
        Console.WriteLine("[GameEngine] Initializing...");
        
        _cityGenerator.GenerateCity(_worldState, 20, 20);
        
        Console.WriteLine($"[GameEngine] Generated {_worldState.Buildings.Count} buildings");
        
        var spacing = 15f;
        _roadGraph.GenerateGridRoadNetwork(20f, 20, 20, spacing);
        
        _worldState.Roads = _roadGraph;
        
        _trafficSystem.Initialize();
        
        _playerSystem.Initialize();
        
        await _webSocketServer.StartAsync();
        
        _simulationLoop.Start();
        
        Console.WriteLine("[GameEngine] Initialization complete");
    }

    public void Update(long tick)
    {
        _worldState.Tick = tick;
        _worldState.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
        _trafficSystem.Update(DeltaTime);
        
        _playerSystem.Update(DeltaTime);
        
        _worldState.Cars = _trafficSystem.Cars;
        _worldState.Players = _playerSystem.GetAllPlayers();
        
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

    public async Task ShutdownAsync()
    {
        Console.WriteLine("[GameEngine] Shutting down...");
        _simulationLoop.Stop();
        _trafficSystem.Shutdown();
        _playerSystem.Shutdown();
        await _webSocketServer.StopAsync();
        Console.WriteLine("[GameEngine] Shutdown complete");
    }
}
