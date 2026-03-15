using System.Text.Json.Serialization;

namespace NeuroCity.Server.Player;

public class PlayerInput
{
    [JsonPropertyName("moveForward")]
    public float MoveForward { get; set; }

    [JsonPropertyName("moveRight")]
    public float MoveRight { get; set; }

    [JsonPropertyName("sprint")]
    public bool Sprint { get; set; }

    [JsonPropertyName("jump")]
    public bool Jump { get; set; }

    [JsonPropertyName("deltaYaw")]
    public float DeltaYaw { get; set; }

    [JsonPropertyName("deltaPitch")]
    public float DeltaPitch { get; set; }
}

public class PlayerSystem
{
    private readonly Dictionary<string, PlayerController> _players = new();
    private readonly object _lock = new();

    public PlayerController? LocalPlayer { get; private set; }

    public void Initialize()
    {
        LocalPlayer = new PlayerController();
        lock (_lock)
        {
            _players[LocalPlayer.Id] = LocalPlayer;
        }
        Console.WriteLine($"[PlayerSystem] Initialized player: {LocalPlayer.Id}");
    }

    public void ProcessInput(string playerId, PlayerInput input)
    {
        PlayerController? player;
        lock (_lock)
        {
            if (!_players.TryGetValue(playerId, out player))
                return;
        }

        player.SetInput(
            input.MoveForward,
            input.MoveRight,
            input.Sprint,
            input.Jump,
            input.DeltaYaw,
            input.DeltaPitch
        );
    }

    public void Update(float deltaTime)
    {
        List<PlayerController> playersCopy;
        lock (_lock)
        {
            playersCopy = new List<PlayerController>(_players.Values);
        }

        foreach (var player in playersCopy)
        {
            player.Update(deltaTime);
        }
    }

    public List<PlayerController> GetAllPlayers()
    {
        lock (_lock)
        {
            return new List<PlayerController>(_players.Values);
        }
    }

    public void Shutdown()
    {
        lock (_lock)
        {
            _players.Clear();
        }
        Console.WriteLine("[PlayerSystem] Shutdown complete");
    }
}
