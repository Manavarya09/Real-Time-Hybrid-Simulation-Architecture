using System.Text.Json.Serialization;
using NeuroCity.Server.Player;

namespace NeuroCity.Server.Multiplayer;

public enum ConnectionState
{
    Connecting,
    Authenticating,
    Connected,
    Loading,
    Playing,
    Disconnected
}

public class NetworkPlayer
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = "Player";

    [JsonPropertyName("state")]
    public string State { get; set; } = "Connecting";

    [JsonPropertyName("ping")]
    public int Ping { get; set; }

    [JsonPropertyName("joinedAt")]
    public long JoinedAt { get; set; }

    [JsonPropertyName("kills")]
    public int Kills { get; set; }

    [JsonPropertyName("deaths")]
    public int Deaths { get; set; }

    [JsonPropertyName("score")]
    public int Score { get; set; }

    [JsonPropertyName("isAdmin")]
    public bool IsAdmin { get; set; }

    [JsonPropertyName("isSpectator")]
    public bool IsSpectator { get; set; }

    public NetworkPlayer()
    {
        Id = Guid.NewGuid().ToString();
        JoinedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}

public class NetworkMessage
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("senderId")]
    public string SenderId { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("data")]
    public string Data { get; set; } = string.Empty;
}

public class LobbySettings
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "NeuroCity Server";

    [JsonPropertyName("maxPlayers")]
    public int MaxPlayers { get; set; } = 32;

    [JsonPropertyName("isPublic")]
    public bool IsPublic { get; set; } = true;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("map")]
    public string Map { get; set; } = "default";

    [JsonPropertyName("gameMode")]
    public string GameMode { get; set; } = "sandbox";

    [JsonPropertyName("allowBuilding")]
    public bool AllowBuilding { get; set; } = true;

    [JsonPropertyName("allowDestroying")]
    public bool AllowDestroying { get; set; } = true;

    [JsonPropertyName("tickRate")]
    public int TickRate { get; set; } = 20;

    [JsonPropertyName("isPaused")]
    public bool IsPaused { get; set; } = false;
}

public class MultiplayerSystem
{
    private readonly Dictionary<string, NetworkPlayer> _players = new();
    private readonly Dictionary<string, WebSocket> _connections = new();
    private readonly Queue<NetworkMessage> _messageQueue = new();
    private readonly object _lock = new();
    private LobbySettings _lobbySettings = new();
    private string _serverId;
    private DateTime _startTime;

    public LobbySettings Settings => _lobbySettings;
    public int PlayerCount => _players.Count;
    public DateTime StartTime => _startTime;

    public MultiplayerSystem()
    {
        _serverId = Guid.NewGuid().ToString("N")[..8].ToUpper();
        _startTime = DateTime.UtcNow;
    }

    public void Initialize()
    {
        Console.WriteLine($"[Multiplayer] Server '{_lobbySettings.Name}' initialized");
        Console.WriteLine($"[Multiplayer] Server ID: {_serverId}");
        Console.WriteLine($"[Multiplayer] Max Players: {_lobbySettings.MaxPlayers}");
    }

    public string AddPlayer(WebSocket ws, string name)
    {
        if (_players.Count >= _lobbySettings.MaxPlayers)
        {
            return string.Empty;
        }

        var player = new NetworkPlayer
        {
            Name = name,
            State = "Connected"
        };

        lock (_lock)
        {
            _players[player.Id] = player;
            _connections[player.Id] = ws;
        }

        Console.WriteLine($"[Multiplayer] Player joined: {name} (ID: {player.Id})");
        BroadcastPlayerList();
        
        return player.Id;
    }

    public void RemovePlayer(string playerId)
    {
        lock (_lock)
        {
            if (_players.Remove(playerId))
            {
                _connections.Remove(playerId);
                Console.WriteLine($"[Multiplayer] Player left: {playerId}");
            }
        }
        BroadcastPlayerList();
    }

    public void UpdatePlayerState(string playerId, ConnectionState state)
    {
        lock (_lock)
        {
            if (_players.TryGetValue(playerId, out var player))
            {
                player.State = state.ToString();
            }
        }
    }

    public void HandleMessage(string playerId, NetworkMessage message)
    {
        message.SenderId = playerId;
        message.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        lock (_lock)
        {
            _messageQueue.Enqueue(message);
        }

        ProcessMessage(message);
    }

    private void ProcessMessage(NetworkMessage message)
    {
        switch (message.Type)
        {
            case "playerMove":
                HandlePlayerMove(message);
                break;
            case "playerAction":
                HandlePlayerAction(message);
                break;
            case "chat":
                HandleChat(message);
                break;
            case "command":
                HandleCommand(message);
                break;
            case "ping":
                RespondToPing(message);
                break;
        }
    }

    private void HandlePlayerMove(NetworkMessage msg)
    {
        BroadcastToOthers(msg.SenderId, msg);
    }

    private void HandlePlayerAction(NetworkMessage msg)
    {
        Console.WriteLine($"[Multiplayer] Player action from {msg.SenderId}: {msg.Data}");
        BroadcastToAll(msg);
    }

    private void HandleChat(NetworkMessage msg)
    {
        lock (_lock)
        {
            if (_players.TryGetValue(msg.SenderId, out var player))
            {
                var chatMessage = $"[{player.Name}] {msg.Data}";
                Console.WriteLine($"[Chat] {chatMessage}");
                
                var broadcastMsg = new NetworkMessage
                {
                    Type = "chat",
                    Data = chatMessage,
                    SenderId = msg.SenderId
                };
                BroadcastToAll(broadcastMsg);
            }
        }
    }

    private void HandleCommand(NetworkMessage msg)
    {
        var parts = msg.Data.Split(' ', 2);
        var command = parts[0].ToLower();
        var args = parts.Length > 1 ? parts[1] : "";

        lock (_lock)
        {
            if (!_players.TryGetValue(msg.SenderId, out var player))
                return;

            switch (command)
            {
                case "/me":
                    BroadcastToAll(new NetworkMessage
                    {
                        Type = "chat",
                        Data = $"* {player.Name} {args}"
                    });
                    break;
                case "/kill":
                    player.Deaths++;
                    break;
                case "/score":
                    player.Score += 100;
                    break;
                case "/admin":
                    player.IsAdmin = true;
                    BroadcastToAll(new NetworkMessage
                    {
                        Type = "system",
                        Data = $"{player.Name} is now an admin"
                    });
                    break;
            }
        }
    }

    private void RespondToPing(NetworkMessage msg)
    {
        lock (_lock)
        {
            if (_players.TryGetValue(msg.SenderId, out var player))
            {
                player.Ping = (int)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - msg.Timestamp);
            }
        }
    }

    public void BroadcastToAll(NetworkMessage message)
    {
        var json = JsonSerializer.Serialize(message);
        
        lock (_lock)
        {
            foreach (var (playerId, ws) in _connections)
            {
                if (ws.State == WebSocketState.Open)
                {
                    _ = SendToClient(ws, json);
                }
            }
        }
    }

    public void BroadcastToOthers(string excludePlayerId, NetworkMessage message)
    {
        var json = JsonSerializer.Serialize(message);
        
        lock (_lock)
        {
            foreach (var (playerId, ws) in _connections)
            {
                if (playerId != excludePlayerId && ws.State == WebSocketState.Open)
                {
                    _ = SendToClient(ws, json);
                }
            }
        }
    }

    private async Task SendToClient(WebSocket ws, string message)
    {
        try
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(message);
            await ws.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );
        }
        catch { }
    }

    private void BroadcastPlayerList()
    {
        List<NetworkPlayer> players;
        lock (_lock)
        {
            players = _players.Values.ToList();
        }

        var message = new NetworkMessage
        {
            Type = "playerList",
            Data = JsonSerializer.Serialize(players)
        };
        
        BroadcastToAll(message);
    }

    public List<NetworkPlayer> GetPlayers()
    {
        lock (_lock)
        {
            return _players.Values.ToList();
        }
    }

    public void UpdateSettings(LobbySettings settings)
    {
        _lobbySettings = settings;
        Console.WriteLine($"[Multiplayer] Lobby settings updated: {settings.Name}");
    }

    public string GetServerInfo()
    {
        var uptime = DateTime.UtcNow - _startTime;
        return $"ID: {_serverId}\n" +
               $"Name: {_lobbySettings.Name}\n" +
               $"Players: {_players.Count}/{_lobbySettings.MaxPlayers}\n" +
               $"Map: {_lobbySettings.Map}\n" +
               $"Mode: {_lobbySettings.GameMode}\n" +
               $"Uptime: {uptime:hh\\:mm\\:ss}";
    }

    public void Shutdown()
    {
        lock (_lock)
        {
            _players.Clear();
            _connections.Clear();
        }
        Console.WriteLine("[Multiplayer] Shutdown complete");
    }
}
