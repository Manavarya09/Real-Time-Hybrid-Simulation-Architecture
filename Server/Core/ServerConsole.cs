using System.Text.RegularExpressions;

namespace NeuroCity.Server.Core;

public class ConsoleCommand
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Usage { get; set; } = string.Empty;
    public Action<string[]> Execute { get; set; } = null!;
}

public class ServerConsole
{
    private readonly GameEngine _engine;
    private readonly Dictionary<string, ConsoleCommand> _commands = new();
    private bool _isRunning;
    private Task? _consoleTask;

    public ServerConsole(GameEngine engine)
    {
        _engine = engine;
        InitializeCommands();
    }

    private void InitializeCommands()
    {
        _commands["help"] = new ConsoleCommand
        {
            Name = "help",
            Description = "Show all available commands",
            Usage = "help [command]",
            Execute = args => HelpCommand(args)
        };

        _commands["save"] = new ConsoleCommand
        {
            Name = "save",
            Description = "Save the current game",
            Usage = "save [filename]",
            Execute = args => SaveCommand(args)
        };

        _commands["load"] = new ConsoleCommand
        {
            Name = "load",
            Description = "Load a saved game",
            Usage = "load <filename>",
            Execute = args => LoadCommand(args)
        };

        _commands["list"] = new ConsoleCommand
        {
            Name = "list",
            Description = "List saved games",
            Usage = "list",
            Execute = args => ListCommand()
        };

        _commands["time"] = new ConsoleCommand
        {
            Name = "time",
            Description = "Set or get time of day",
            Usage = "time [hour]",
            Execute = args => TimeCommand(args)
        };

        _commands["weather"] = new ConsoleCommand
        {
            Name = "weather",
            Description = "Set weather type",
            Usage = "weather [clear|cloudy|rain|storm|fog|snow]",
            Execute = args => WeatherCommand(args)
        };

        _commands["timescale"] = new ConsoleCommand
        {
            Name = "timescale",
            Description = "Set time scale (speed)",
            Usage = "timescale [0.1-10]",
            Execute = args => TimeScaleCommand(args)
        };

        _commands["spawn"] = new ConsoleCommand
        {
            Name = "spawn",
            Description = "Spawn entities",
            Usage = "spawn [car|citizen] [count]",
            Execute = args => SpawnCommand(args)
        };

        _commands["stats"] = new ConsoleCommand
        {
            Name = "stats",
            Description = "Show game statistics",
            Usage = "stats",
            Execute = args => StatsCommand()
        };

        _commands["resources"] = new ConsoleCommand
        {
            Name = "resources",
            Description = "Show resource levels",
            Usage = "resources",
            Execute = args => ResourcesCommand()
        };

        _commands["build"] = new ConsoleCommand
        {
            Name = "build",
            Description = "Build a structure",
            Usage = "build [type] [x] [z]",
            Execute = args => BuildCommand(args)
        };

        _commands["kick"] = new ConsoleCommand
        {
            Name = "kick",
            Description = "Kick a player",
            Usage = "kick [playerid]",
            Execute = args => KickCommand(args)
        };

        _commands["broadcast"] = new ConsoleCommand
        {
            Name = "broadcast",
            Description = "Send message to all players",
            Usage = "broadcast <message>",
            Execute = args => BroadcastCommand(args)
        };

        _commands["clear"] = new ConsoleCommand
        {
            Name = "clear",
            Description = "Clear the console",
            Usage = "clear",
            Execute = args => Console.Clear()
        };

        _commands["exit"] = new ConsoleCommand
        {
            Name = "exit",
            Description = "Shutdown the server",
            Usage = "exit",
            Execute = args => ExitCommand()
        };
    }

    public void Start()
    {
        _isRunning = true;
        _consoleTask = Task.Run(RunConsole);
        Console.WriteLine("[Console] Server console started");
    }

    public async Task StopAsync()
    {
        _isRunning = false;
        if (_consoleTask != null)
        {
            await _consoleTask;
        }
    }

    private async Task RunConsole()
    {
        while (_isRunning)
        {
            try
            {
                Console.Write("NeuroCity> ");
                var input = await Console.In.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(input)) continue;
                
                ProcessCommand(input);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Console] Error: {ex.Message}");
            }
        }
    }

    public void ProcessCommand(string input)
    {
        input = input.Trim();
        
        var match = Regex.Match(input, @"^(\S+)(?:\s+(.*))?$");
        if (!match.Success) return;

        var commandName = match.Groups[1].Value.ToLower();
        var argsString = match.Groups[2].Success ? match.Groups[2].Value : "";
        var args = argsString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (_commands.TryGetValue(commandName, out var command))
        {
            try
            {
                command.Execute(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Console] Command error: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"Unknown command: {commandName}. Type 'help' for available commands.");
        }
    }

    private void HelpCommand(string[] args)
    {
        if (args.Length > 0 && _commands.TryGetValue(args[0], out var cmd))
        {
            Console.WriteLine($"Usage: {cmd.Usage}");
            Console.WriteLine($"Description: {cmd.Description}");
            return;
        }

        Console.WriteLine("Available commands:");
        foreach (var cmd in _commands.Values)
        {
            Console.WriteLine($"  {cmd.Name,-12} - {cmd.Description}");
        }
    }

    private async void SaveCommand(string[] args)
    {
        var fileName = args.Length > 0 ? args[0] : $"save_{DateTime.Now:yyyyMMdd_HHmmss}";
        await _engine.SaveGameAsync(fileName);
        Console.WriteLine($"Game saved as: {fileName}");
    }

    private async void LoadCommand(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: load <filename>");
            return;
        }
        await _engine.LoadGameAsync(args[0]);
    }

    private void ListCommand()
    {
        var saves = _engine.SaveLoadSystem.GetSaveFiles();
        if (saves.Count == 0)
        {
            Console.WriteLine("No saved games found.");
            return;
        }
        Console.WriteLine("Saved games:");
        foreach (var save in saves)
        {
            Console.WriteLine($"  - {save}");
        }
    }

    private void TimeCommand(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine($"Current time: {_engine.EnvironmentSystem.State.TimeOfDay:F2}");
            return;
        }
        
        if (float.TryParse(args[0], out var hour))
        {
            _engine.EnvironmentSystem.SetTimeOfDay(hour);
            Console.WriteLine($"Time set to: {hour:F2}");
        }
    }

    private void WeatherCommand(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine($"Current weather: {_engine.EnvironmentSystem.State.Weather.Type}");
            return;
        }
        
        _engine.EnvironmentSystem.SetWeather(args[0]);
        Console.WriteLine($"Weather set to: {args[0]}");
    }

    private void TimeScaleCommand(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine($"Current time scale: {_engine.EnvironmentSystem.State.TimeScale:F1}x");
            return;
        }
        
        if (float.TryParse(args[0], out var scale))
        {
            _engine.EnvironmentSystem.SetTimeScale(scale);
            Console.WriteLine($"Time scale set to: {scale:F1}x");
        }
    }

    private void SpawnCommand(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: spawn [car|citizen] [count]");
            return;
        }
        
        Console.WriteLine($"Spawn command executed for: {args[0]}");
    }

    private void StatsCommand()
    {
        var world = _engine.WorldState;
        Console.WriteLine("=== Game Statistics ===");
        Console.WriteLine($"Tick: {world.Tick}");
        Console.WriteLine($"Buildings: {world.Buildings.Count}");
        Console.WriteLine($"Cars: {world.Cars.Count}");
        Console.WriteLine($"Players: {world.Players.Count}");
        Console.WriteLine($"Road Nodes: {world.Roads?.Nodes.Count ?? 0}");
    }

    private void ResourcesCommand()
    {
        var resources = _engine.EconomySystem.Resources.GetResourceDisplay();
        Console.WriteLine("=== Resources ===");
        foreach (var resource in resources)
        {
            Console.WriteLine($"{resource.Key}: {resource.Value:F0}");
        }
    }

    private void BuildCommand(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: build [type] [x] [z]");
            return;
        }
        
        if (float.TryParse(args[1], out var x) && float.TryParse(args[2], out var z))
        {
            Console.WriteLine($"Building {args[0]} at ({x}, {z})");
        }
    }

    private void KickCommand(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: kick [playerid]");
            return;
        }
        
        Console.WriteLine($"Kicking player: {args[0]}");
    }

    private void BroadcastCommand(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: broadcast <message>");
            return;
        }
        
        var message = string.Join(" ", args);
        Console.WriteLine($"[Broadcast] {message}");
    }

    private async void ExitCommand()
    {
        Console.WriteLine("Shutting down server...");
        await _engine.ShutdownAsync();
        Environment.Exit(0);
    }
}
