using System.Text.Json.Serialization;

namespace NeuroCity.Server.Traffic;

public class TrafficSystem
{
    private readonly RoadGraph _roadGraph;
    private readonly Pathfinding _pathfinding;
    private readonly List<CarAgent> _cars = new();
    private readonly int _maxCars;
    private readonly Random _random = new();
    private float _spawnTimer;
    private readonly float _spawnInterval = 0.5f;

    [JsonPropertyName("cars")]
    public List<CarAgent> Cars => _cars;

    public TrafficSystem(RoadGraph roadGraph, int maxCars = 50)
    {
        _roadGraph = roadGraph;
        _pathfinding = new Pathfinding(roadGraph);
        _maxCars = maxCars;
    }

    public void Initialize()
    {
        Console.WriteLine($"[TrafficSystem] Initializing with {_maxCars} cars...");
        
        SpawnInitialCars();
        
        Console.WriteLine($"[TrafficSystem] Spawned {_cars.Count} cars");
    }

    private void SpawnInitialCars()
    {
        for (int i = 0; i < _maxCars; i++)
        {
            SpawnCar();
        }
    }

    private void SpawnCar()
    {
        if (_roadGraph.Nodes.Count == 0) return;

        var startNode = _roadGraph.GetRandomNode();
        if (startNode == null) return;

        var car = new CarAgent();
        car.SpawnAt(startNode);

        AssignNewDestination(car);

        _cars.Add(car);
    }

    private void AssignNewDestination(CarAgent car)
    {
        if (_roadGraph.Nodes.Count < 2) return;

        RoadNode? destination = null;
        var attempts = 0;
        const int maxAttempts = 10;

        while (destination == null && attempts < maxAttempts)
        {
            var candidate = _roadGraph.GetRandomNode();
            if (candidate != null && candidate.Id != car.CurrentNode?.Id)
            {
                destination = candidate;
            }
            attempts++;
        }

        if (destination == null) return;

        var path = _pathfinding.FindPath(car.CurrentNode!, destination);
        
        if (path != null && path.Count > 1)
        {
            car.SetPath(path);
        }
    }

    public void Update(float deltaTime)
    {
        foreach (var car in _cars)
        {
            car.Update(deltaTime);

            if (car.HasReachedDestination())
            {
                if (car.CurrentNode != null)
                {
                    var newDestination = _roadGraph.GetRandomNode();
                    if (newDestination != null)
                    {
                        var path = _pathfinding.FindPath(car.CurrentNode, newDestination);
                        if (path != null && path.Count > 1)
                        {
                            car.SetPath(path);
                        }
                    }
                }
            }
        }
    }

    public void Shutdown()
    {
        Console.WriteLine($"[TrafficSystem] Shutting down {_cars.Count} cars");
        _cars.Clear();
    }
}
