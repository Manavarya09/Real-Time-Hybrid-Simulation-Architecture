using NeuroCity.Server.Core;

Console.WriteLine("===========================================");
Console.WriteLine("  NeuroCity Engine - Server v1.0.0");
Console.WriteLine("===========================================");

var engine = new GameEngine();

Console.CancelKeyPress += async (sender, args) =>
{
    args.Cancel = true;
    await engine.ShutdownAsync();
    Environment.Exit(0);
};

try
{
    await engine.InitializeAsync();
    
    Console.WriteLine("[Main] Press Ctrl+C to shutdown");
    
    await Task.Delay(Timeout.Infinite);
}
catch (Exception ex)
{
    Console.WriteLine($"[Main] Fatal error: {ex.Message}");
    await engine.ShutdownAsync();
    Environment.Exit(1);
}
