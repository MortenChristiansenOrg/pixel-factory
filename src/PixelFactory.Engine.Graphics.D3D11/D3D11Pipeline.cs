using PixelFactory.Engine.Core.Rendering;
using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace PixelFactory.Engine.Graphics.D3D11;

internal sealed class D3D11Pipeline : IPipeline
{
    private readonly ID3D11DeviceContext _context;
    private readonly ID3D11VertexShader _vertexShader;
    private readonly ID3D11PixelShader _pixelShader;
    private readonly ID3D11InputLayout _inputLayout;
    private readonly ID3D11RasterizerState _rasterizerState;

    public D3D11Pipeline(
        ID3D11Device device,
        ID3D11DeviceContext context,
        ID3D11VertexShader vertexShader,
        ID3D11PixelShader pixelShader,
        ID3D11InputLayout inputLayout)
    {
        _context = context;
        _vertexShader = vertexShader;
        _pixelShader = pixelShader;
        _inputLayout = inputLayout;

        _rasterizerState = device.CreateRasterizerState(new RasterizerDescription
        {
            FillMode = FillMode.Solid,
            CullMode = CullMode.Back,
            FrontCounterClockwise = true,
            DepthClipEnable = true,
        });
    }

    public void Bind()
    {
        _context.VSSetShader(_vertexShader);
        _context.PSSetShader(_pixelShader);
        _context.IASetInputLayout(_inputLayout);
        _context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
        _context.RSSetState(_rasterizerState);
    }

    public void Dispose()
    {
        _rasterizerState.Dispose();
        _inputLayout.Dispose();
        _pixelShader.Dispose();
        _vertexShader.Dispose();
    }
}
