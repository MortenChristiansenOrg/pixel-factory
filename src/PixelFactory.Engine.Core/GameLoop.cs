using System.Diagnostics;
using PixelFactory.Engine.Core.ECS;
using PixelFactory.Engine.Core.Rendering;

namespace PixelFactory.Engine.Core;

public sealed class GameLoop : IDisposable
{
    private readonly GameWorld _world = new();
    private readonly IRenderDevice _renderDevice;
    private bool _running;

    public GameLoop(IRenderDevice renderDevice)
    {
        _renderDevice = renderDevice;
    }

    public GameWorld World => _world;

    public Action? ProcessMessages { get; set; }
    public Action<GameWorld, float>? OnUpdate { get; set; }
    public Action<GameWorld>? OnRender { get; set; }

    public void Run()
    {
        _running = true;
        var sw = Stopwatch.StartNew();
        var lastTime = sw.Elapsed;

        while (_running)
        {
            ProcessMessages?.Invoke();

            var now = sw.Elapsed;
            var dt = (float)(now - lastTime).TotalSeconds;
            lastTime = now;

            OnUpdate?.Invoke(_world, dt);

            _renderDevice.BeginFrame();
            OnRender?.Invoke(_world);
            _renderDevice.EndFrame();
            _renderDevice.Present();
        }
    }

    public void Stop() => _running = false;

    public void Dispose()
    {
        _world.Dispose();
        _renderDevice.Dispose();
    }
}
