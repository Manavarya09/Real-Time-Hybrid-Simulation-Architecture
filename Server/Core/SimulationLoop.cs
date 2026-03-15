namespace NeuroCity.Server.Core;

public class SimulationLoop
{
    private readonly GameEngine _engine;
    private readonly int _ticksPerSecond;
    private readonly int _tickDurationMs;
    private bool _isRunning;
    private Task? _loopTask;
    private long _currentTick;

    public long CurrentTick => _currentTick;
    public bool IsRunning => _isRunning;

    public SimulationLoop(GameEngine engine, int ticksPerSecond = 20)
    {
        _engine = engine;
        _ticksPerSecond = ticksPerSecond;
        _tickDurationMs = 1000 / _ticksPerSecond;
    }

    public void Start()
    {
        if (_isRunning) return;
        
        _isRunning = true;
        _loopTask = Task.Run(RunLoop);
        Console.WriteLine($"[SimulationLoop] Started at {_ticksPerSecond} ticks/second");
    }

    public void Stop()
    {
        _isRunning = false;
        _loopTask?.Wait(TimeSpan.FromSeconds(2));
        Console.WriteLine("[SimulationLoop] Stopped");
    }

    private void RunLoop()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        while (_isRunning)
        {
            var tickStartTime = stopwatch.ElapsedMilliseconds;
            
            _currentTick++;
            _engine.Update(_currentTick);
            
            var tickEndTime = stopwatch.ElapsedMilliseconds;
            var elapsed = tickEndTime - tickStartTime;
            var sleepTime = _tickDurationMs - elapsed;
            
            if (sleepTime > 0)
            {
                Thread.Sleep((int)sleepTime);
            }
        }
    }
}
