using System.Numerics;
using System.Runtime.InteropServices;
using PixelFactory.Editor.Core;
using PixelFactory.Engine.Core.Rendering;
using PixelFactory.Engine.Graphics.D3D11;

namespace PixelFactory.Editor.Cli;

internal static class ScreenshotRenderer
{
    public static void RenderMeshToBmp(MeshData meshData, string outputPath, int width = 800, int height = 600)
    {
        // Create hidden window for D3D11
        var hwnd = CreateOffscreenWindow(width, height);
        try
        {
            using var device = new D3D11RenderDevice();
            device.Initialize(hwnd, width, height);

            var (vs, ps) = D3D11RenderDevice.CompileEmbeddedShaders();
            using var pipeline = device.CreatePipeline(vs, ps);
            using var constantBuffer = device.CreateBuffer(
                new BufferDescription(64, BufferUsage.Constant), ReadOnlySpan<byte>.Empty);

            var vertexData = MemoryMarshal.AsBytes(meshData.Vertices.AsSpan());
            using var vertexBuffer = device.CreateBuffer(
                new BufferDescription(vertexData.Length, BufferUsage.Vertex), vertexData);

            var indexData = MemoryMarshal.AsBytes(meshData.Indices.AsSpan());
            using var indexBuffer = device.CreateBuffer(
                new BufferDescription(indexData.Length, BufferUsage.Index), indexData);

            // Fit camera to mesh bounds
            var min = meshData.Vertices[0].Position;
            var max = min;
            foreach (var v in meshData.Vertices)
            {
                min = Vector3.Min(min, v.Position);
                max = Vector3.Max(max, v.Position);
            }
            var center = (min + max) * 0.5f;
            var extent = max - min;
            var size = MathF.Max(extent.X, MathF.Max(extent.Y, extent.Z)) * 1.5f;

            var camera = new IsometricCamera(size, (float)width / height);
            camera.Target = center;

            // Render
            var view = camera.ViewMatrix;
            var proj = camera.ProjectionMatrix;
            var wvp = Matrix4x4.Transpose(view * proj);
            var wvpBytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref wvp, 1));

            device.BeginFrame();
            device.UpdateBuffer(constantBuffer, wvpBytes);
            pipeline.Bind();
            device.SetConstantBuffer(0, constantBuffer);
            device.SetVertexBuffer(vertexBuffer, VertexPositionColor.SizeInBytes);
            device.SetIndexBuffer(indexBuffer);
            device.DrawIndexed(meshData.Indices.Length, 0, 0);
            device.EndFrame();

            // Capture and save
            var rgba = device.CaptureBackBuffer();
            WriteBmp(outputPath, rgba, width, height);
        }
        finally
        {
            DestroyWindow(hwnd);
        }
    }

    private static void WriteBmp(string path, byte[] rgba, int width, int height)
    {
        using var fs = File.Create(path);
        using var bw = new BinaryWriter(fs);

        int rowBytes = width * 3;
        int padding = (4 - rowBytes % 4) % 4;
        int dataSize = (rowBytes + padding) * height;
        int fileSize = 54 + dataSize;

        // File header
        bw.Write((byte)'B');
        bw.Write((byte)'M');
        bw.Write(fileSize);
        bw.Write(0);
        bw.Write(54);

        // Info header
        bw.Write(40);
        bw.Write(width);
        bw.Write(height);
        bw.Write((short)1);
        bw.Write((short)24);
        bw.Write(0);
        bw.Write(dataSize);
        bw.Write(0);
        bw.Write(0);
        bw.Write(0);
        bw.Write(0);

        // Pixel data (BMP is bottom-up, BGR)
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                int i = (y * width + x) * 4;
                bw.Write(rgba[i + 2]); // B
                bw.Write(rgba[i + 1]); // G
                bw.Write(rgba[i]);     // R
            }
            for (int p = 0; p < padding; p++)
                bw.Write((byte)0);
        }
    }

    private static nint CreateOffscreenWindow(int width, int height)
    {
        var className = "PFOffscreen_" + Environment.CurrentManagedThreadId;
        _wndProcDelegate = (hwnd, msg, wParam, lParam) => DefWindowProcW(hwnd, msg, wParam, lParam);

        var wc = new WNDCLASSEXW
        {
            cbSize = (uint)Marshal.SizeOf<WNDCLASSEXW>(),
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate),
            hInstance = GetModuleHandleW(null),
            lpszClassName = className,
        };
        RegisterClassExW(ref wc);

        return CreateWindowExW(
            0, className, "",
            0, // not visible
            0, 0, width, height,
            0, 0, wc.hInstance, 0);
    }

    // prevent GC of the delegate
    [ThreadStatic] private static WndProcDelegate? _wndProcDelegate;

    private delegate nint WndProcDelegate(nint hWnd, uint msg, nint wParam, nint lParam);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WNDCLASSEXW
    {
        public uint cbSize;
        public uint style;
        public nint lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public nint hInstance;
        public nint hIcon;
        public nint hCursor;
        public nint hbrBackground;
        [MarshalAs(UnmanagedType.LPWStr)] public string? lpszMenuName;
        [MarshalAs(UnmanagedType.LPWStr)] public string lpszClassName;
        public nint hIconSm;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern ushort RegisterClassExW(ref WNDCLASSEXW lpwcx);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern nint CreateWindowExW(
        int dwExStyle, string lpClassName, string lpWindowName, int dwStyle,
        int x, int y, int nWidth, int nHeight,
        nint hWndParent, nint hMenu, nint hInstance, nint lpParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyWindow(nint hwnd);

    [DllImport("user32.dll")]
    private static extern nint DefWindowProcW(nint hWnd, uint msg, nint wParam, nint lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern nint GetModuleHandleW(string? lpModuleName);
}
