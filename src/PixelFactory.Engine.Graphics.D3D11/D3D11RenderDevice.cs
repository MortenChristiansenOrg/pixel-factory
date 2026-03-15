using System.Runtime.InteropServices;
using PixelFactory.Engine.Core.Rendering;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using CoreBufferDescription = PixelFactory.Engine.Core.Rendering.BufferDescription;

namespace PixelFactory.Engine.Graphics.D3D11;

public sealed class D3D11RenderDevice : IRenderDevice
{
    private ID3D11Device _device = null!;
    private ID3D11DeviceContext _context = null!;
    private IDXGISwapChain1 _swapChain = null!;
    private ID3D11RenderTargetView _rtv = null!;
    private ID3D11DepthStencilView _dsv = null!;
    private ID3D11Texture2D _depthBuffer = null!;
    private uint _width;
    private uint _height;

    public string BackendName => "Direct3D 11";

    public void Initialize(nint windowHandle, int width, int height)
    {
        _width = (uint)width;
        _height = (uint)height;

        Vortice.Direct3D11.D3D11.D3D11CreateDevice(
            null,
            DriverType.Hardware,
            DeviceCreationFlags.None,
            [FeatureLevel.Level_11_0],
            out _device,
            out _,
            out _context).CheckError();

        using var dxgiDevice = _device.QueryInterface<IDXGIDevice>();
        using var adapter = dxgiDevice.GetAdapter();
        using var factory = adapter.GetParent<IDXGIFactory2>();

        var swapChainDesc = new SwapChainDescription1
        {
            Width = _width,
            Height = _height,
            Format = Format.R8G8B8A8_UNorm,
            BufferCount = 2,
            BufferUsage = Usage.RenderTargetOutput,
            SampleDescription = new SampleDescription(1, 0),
            SwapEffect = SwapEffect.FlipDiscard,
        };

        _swapChain = factory.CreateSwapChainForHwnd(_device, windowHandle, swapChainDesc);
        CreateRenderTargets();
    }

    public void Resize(int width, int height)
    {
        if (width == 0 || height == 0) return;
        _width = (uint)width;
        _height = (uint)height;

        _context.OMSetRenderTargets(renderTargetView: null!, depthStencilView: null);
        _rtv.Dispose();
        _dsv.Dispose();
        _depthBuffer.Dispose();

        _swapChain.ResizeBuffers(0, _width, _height, Format.Unknown, SwapChainFlags.None);
        CreateRenderTargets();
    }

    public void BeginFrame()
    {
        _context.ClearRenderTargetView(_rtv, new Color4(0.05f, 0.05f, 0.08f, 1f));
        _context.ClearDepthStencilView(_dsv, DepthStencilClearFlags.Depth, 1f, 0);
        _context.OMSetRenderTargets(_rtv, _dsv);

        var viewport = new Viewport(0, 0, _width, _height, 0f, 1f);
        _context.RSSetViewport(viewport);
    }

    public void EndFrame()
    {
    }

    public void Present()
    {
        _swapChain.Present(1, PresentFlags.None);
    }

    public IBuffer CreateBuffer(CoreBufferDescription desc, ReadOnlySpan<byte> initialData)
    {
        var bindFlags = desc.Usage switch
        {
            BufferUsage.Vertex => BindFlags.VertexBuffer,
            BufferUsage.Index => BindFlags.IndexBuffer,
            BufferUsage.Constant => BindFlags.ConstantBuffer,
            _ => throw new ArgumentOutOfRangeException(nameof(desc)),
        };

        var bufferDesc = new Vortice.Direct3D11.BufferDescription
        {
            ByteWidth = (uint)desc.SizeInBytes,
            BindFlags = bindFlags,
            Usage = ResourceUsage.Default,
        };

        ID3D11Buffer nativeBuffer;
        if (initialData.Length > 0)
        {
            unsafe
            {
                fixed (byte* pData = initialData)
                {
                    var subresource = new SubresourceData((nint)pData, (uint)desc.SizeInBytes);
                    nativeBuffer = _device.CreateBuffer(bufferDesc, subresource);
                }
            }
        }
        else
        {
            nativeBuffer = _device.CreateBuffer(bufferDesc);
        }

        return new D3D11Buffer(nativeBuffer, desc.SizeInBytes);
    }

    public IPipeline CreatePipeline(ShaderBytecode vertexShader, ShaderBytecode pixelShader)
    {
        var vs = _device.CreateVertexShader(vertexShader.Data);
        var ps = _device.CreatePixelShader(pixelShader.Data);

        var inputElements = new[]
        {
            new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0),
            new InputElementDescription("COLOR", 0, Format.R32G32B32A32_Float, 12, 0),
        };

        var inputLayout = _device.CreateInputLayout(inputElements, vertexShader.Data);
        return new D3D11Pipeline(_device, _context, vs, ps, inputLayout);
    }

    public void SetVertexBuffer(IBuffer buffer, int stride)
    {
        var d3d11Buffer = (D3D11Buffer)buffer;
        _context.IASetVertexBuffer(0, d3d11Buffer.NativeBuffer, (uint)stride);
    }

    public void SetIndexBuffer(IBuffer buffer)
    {
        var d3d11Buffer = (D3D11Buffer)buffer;
        _context.IASetIndexBuffer(d3d11Buffer.NativeBuffer, Format.R16_UInt, 0);
    }

    public void SetConstantBuffer(int slot, IBuffer buffer)
    {
        var d3d11Buffer = (D3D11Buffer)buffer;
        _context.VSSetConstantBuffer((uint)slot, d3d11Buffer.NativeBuffer);
    }

    public void UpdateBuffer(IBuffer buffer, ReadOnlySpan<byte> data)
    {
        var d3d11Buffer = (D3D11Buffer)buffer;
        _context.UpdateSubresource(data, d3d11Buffer.NativeBuffer);
    }

    public void DrawIndexed(int indexCount, int startIndex, int baseVertex)
    {
        _context.DrawIndexed((uint)indexCount, (uint)startIndex, baseVertex);
    }

    public void SetViewport(int x, int y, int width, int height)
    {
        _context.RSSetViewport(new Viewport(x, y, width, height, 0f, 1f));
    }

    public byte[] CaptureBackBuffer()
    {
        using var backBuffer = _swapChain.GetBuffer<ID3D11Texture2D>(0);
        var desc = backBuffer.Description;
        desc.Usage = ResourceUsage.Staging;
        desc.BindFlags = BindFlags.None;
        desc.CPUAccessFlags = CpuAccessFlags.Read;

        using var staging = _device.CreateTexture2D(desc);
        _context.CopyResource(staging, backBuffer);

        var mapped = _context.Map(staging, 0, MapMode.Read);
        try
        {
            var data = new byte[_width * _height * 4];
            var rowPitch = (int)mapped.RowPitch;
            var rowBytes = (int)_width * 4;
            for (int y = 0; y < _height; y++)
            {
                Marshal.Copy(mapped.DataPointer + y * rowPitch, data, y * rowBytes, rowBytes);
            }
            return data;
        }
        finally
        {
            _context.Unmap(staging, 0);
        }
    }

    public static ShaderBytecode CompileShader(string source, string entryPoint, string profile)
    {
        var result = Compiler.Compile(source, entryPoint, "BasicColor.hlsl", profile);
        return new ShaderBytecode(result.ToArray());
    }

    public static (ShaderBytecode vs, ShaderBytecode ps) CompileEmbeddedShaders()
    {
        var assembly = typeof(D3D11RenderDevice).Assembly;
        using var stream = assembly.GetManifestResourceStream(
            "PixelFactory.Engine.Graphics.D3D11.Shaders.BasicColor.hlsl")
            ?? throw new InvalidOperationException("Embedded shader not found");

        using var reader = new StreamReader(stream);
        var source = reader.ReadToEnd();

        var vs = CompileShader(source, "VSMain", "vs_5_0");
        var ps = CompileShader(source, "PSMain", "ps_5_0");
        return (vs, ps);
    }

    public void Dispose()
    {
        _dsv?.Dispose();
        _depthBuffer?.Dispose();
        _rtv?.Dispose();
        _swapChain?.Dispose();
        _context?.Dispose();
        _device?.Dispose();
    }

    private void CreateRenderTargets()
    {
        using var backBuffer = _swapChain.GetBuffer<ID3D11Texture2D>(0);
        _rtv = _device.CreateRenderTargetView(backBuffer);

        var depthDesc = new Texture2DDescription
        {
            Width = _width,
            Height = _height,
            MipLevels = 1,
            ArraySize = 1,
            Format = Format.D24_UNorm_S8_UInt,
            SampleDescription = new SampleDescription(1, 0),
            Usage = ResourceUsage.Default,
            BindFlags = BindFlags.DepthStencil,
        };

        _depthBuffer = _device.CreateTexture2D(depthDesc);
        _dsv = _device.CreateDepthStencilView(_depthBuffer);
    }
}
