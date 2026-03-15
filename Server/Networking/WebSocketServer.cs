using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using NeuroCity.Server.Player;

namespace NeuroCity.Server.Networking;

public class WebSocketServer
{
    private readonly GameEngine _engine;
    private HttpListener? _httpListener;
    private readonly List<WebSocket> _connectedClients = new();
    private readonly Dictionary<WebSocket, string> _clientPlayerIds = new();
    private readonly object _clientsLock = new();
    private bool _isRunning;

    public const int Port = 5000;

    public WebSocketServer(GameEngine engine)
    {
        _engine = engine;
    }

    public async Task StartAsync()
    {
        _httpListener = new HttpListener();
        _httpListener.Prefixes.Add($"http://localhost:{Port}/");
        _httpListener.Start();
        _isRunning = true;
        
        Console.WriteLine($"[WebSocketServer] Listening on ws://localhost:{Port}/");
        
        _ = Task.Run(AcceptConnectionsAsync);
    }

    public async Task StopAsync()
    {
        _isRunning = false;
        
        lock (_clientsLock)
        {
            foreach (var client in _connectedClients)
            {
                client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutting down", CancellationToken.None).Wait();
            }
            _connectedClients.Clear();
            _clientPlayerIds.Clear();
        }
        
        _httpListener?.Stop();
        _httpListener?.Close();
        Console.WriteLine("[WebSocketServer] Stopped");
    }

    private async Task AcceptConnectionsAsync()
    {
        while (_isRunning && _httpListener != null)
        {
            try
            {
                var context = await _httpListener.GetContextAsync();
                
                if (context.Request.IsWebSocketRequest)
                {
                    var wsContext = await context.AcceptWebSocketAsync(null);
                    var webSocket = wsContext.WebSocket;
                    
                    lock (_clientsLock)
                    {
                        _connectedClients.Add(webSocket);
                    }
                    
                    Console.WriteLine($"[WebSocketServer] Client connected. Total: {_connectedClients.Count}");
                    
                    _ = HandleClientAsync(webSocket);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
            catch (Exception ex)
            {
                if (_isRunning)
                {
                    Console.WriteLine($"[WebSocketServer] Accept error: {ex.Message}");
                }
            }
        }
    }

    private async Task HandleClientAsync(WebSocket webSocket)
    {
        var buffer = new byte[4096];
        var connectionId = Guid.NewGuid().ToString();
        var playerId = _engine.PlayerSystem.LocalPlayer?.Id ?? connectionId;

        lock (_clientsLock)
        {
            _clientPlayerIds[webSocket] = playerId;
        }

        try
        {
            await _engine.SendInitialStateAsync(connectionId);

            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }
                
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    ProcessClientMessage(playerId, message);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WebSocketServer] Client error: {ex.Message}");
        }
        finally
        {
            lock (_clientsLock)
            {
                _connectedClients.Remove(webSocket);
                _clientPlayerIds.Remove(webSocket);
            }
            
            Console.WriteLine($"[WebSocketServer] Client disconnected. Total: {_connectedClients.Count}");
            
            if (webSocket.State != WebSocketState.Closed)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
            }
        }
    }

    private void ProcessClientMessage(string playerId, string message)
    {
        try
        {
            using var doc = JsonDocument.Parse(message);
            var root = doc.RootElement;

            if (root.TryGetProperty("type", out var typeElement))
            {
                var type = typeElement.GetString();
                
                if (type == "input" && root.TryGetProperty("data", out var dataElement))
                {
                    var input = JsonSerializer.Deserialize<PlayerInput>(dataElement.GetRawText());
                    if (input != null)
                    {
                        _engine.PlayerSystem.ProcessInput(playerId, input);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WebSocketServer] Message parse error: {ex.Message}");
        }
    }

    public async Task BroadcastAsync(string message)
    {
        var bytes = Encoding.UTF8.GetBytes(message);
        var segment = new ArraySegment<byte>(bytes);
        
        List<WebSocket> clientsCopy;
        lock (_clientsLock)
        {
            clientsCopy = new List<WebSocket>(_connectedClients);
        }

        var deadClients = new List<WebSocket>();
        
        foreach (var client in clientsCopy)
        {
            try
            {
                if (client.State == WebSocketState.Open)
                {
                    await client.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                else
                {
                    deadClients.Add(client);
                }
            }
            catch
            {
                deadClients.Add(client);
            }
        }

        if (deadClients.Count > 0)
        {
            lock (_clientsLock)
            {
                foreach (var client in deadClients)
                {
                    _connectedClients.Remove(client);
                    _clientPlayerIds.Remove(client);
                }
            }
        }
    }

    public async Task SendToClientAsync(string connectionId, string message)
    {
        await BroadcastAsync(message);
    }
}
