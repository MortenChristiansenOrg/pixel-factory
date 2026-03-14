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

    public void Run()
    {
        _running = true;
        while (_running)
        {
            _renderDevice.BeginFrame();
            // TODO: update systems
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
