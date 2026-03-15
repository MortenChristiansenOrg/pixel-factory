using System.Runtime.InteropServices;
using System.Windows.Interop;
using PixelFactory.Engine.Graphics.D3D11;

namespace PixelFactory.Editor.App;

internal sealed class D3D11ViewportHost : HwndHost
{
    private const int WS_CHILD = 0x40000000;
    private const int WS_VISIBLE = 0x10000000;
    private const int HOST_ID = 0x1000;
    private const int GWL_WNDPROC = -4;
    private const uint WM_MOUSEWHEEL = 0x020A;

    private nint _hwndHost;
    private nint _originalWndProc;
    private WndProcDelegate? _wndProcDelegate;

    public D3D11RenderDevice? Device { get; private set; }
    public int ViewportWidth { get; private set; }
    public int ViewportHeight { get; private set; }

    public Action? OnDeviceReady { get; set; }
    public Action<int>? MouseWheelCallback { get; set; }

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        var w = Math.Max(1, (int)ActualWidth);
        var h = Math.Max(1, (int)ActualHeight);

        _hwndHost = CreateWindowExW(
            0, "static", "",
            WS_CHILD | WS_VISIBLE,
            0, 0, w, h,
            hwndParent.Handle, HOST_ID, 0, 0);

        // Subclass the child window to intercept mouse wheel messages
        _wndProcDelegate = SubclassWndProc;
        var newWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate);
        _originalWndProc = SetWindowLongPtrW(_hwndHost, GWL_WNDPROC, newWndProc);

        ViewportWidth = w;
        ViewportHeight = h;

        Device = new D3D11RenderDevice();
        Device.Initialize(_hwndHost, ViewportWidth, ViewportHeight);

        OnDeviceReady?.Invoke();

        return new HandleRef(this, _hwndHost);
    }

    private nint SubclassWndProc(nint hwnd, uint msg, nint wParam, nint lParam)
    {
        if (msg == WM_MOUSEWHEEL)
        {
            int delta = (short)(wParam >> 16);
            MouseWheelCallback?.Invoke(delta);
            return 0;
        }
        return CallWindowProcW(_originalWndProc, hwnd, msg, wParam, lParam);
    }

    protected override void DestroyWindowCore(HandleRef hwnd)
    {
        Device?.Dispose();
        Device = null;
        DestroyWindow(hwnd.Handle);
    }

    public void ResizeViewport(int width, int height)
    {
        if (width <= 0 || height <= 0) return;
        ViewportWidth = width;
        ViewportHeight = height;
        Device?.Resize(width, height);
    }

    private delegate nint WndProcDelegate(nint hwnd, uint msg, nint wParam, nint lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern nint CreateWindowExW(
        int dwExStyle, string lpClassName, string lpWindowName, int dwStyle,
        int x, int y, int nWidth, int nHeight,
        nint hWndParent, nint hMenu, nint hInstance, nint lpParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyWindow(nint hwnd);

    [DllImport("user32.dll")]
    private static extern nint SetWindowLongPtrW(nint hWnd, int nIndex, nint dwNewLong);

    [DllImport("user32.dll")]
    private static extern nint CallWindowProcW(nint lpPrevWndFunc, nint hWnd, uint msg, nint wParam, nint lParam);
}
