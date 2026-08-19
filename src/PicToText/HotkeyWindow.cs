using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace PicToText;

internal sealed class HotkeyWindow : IDisposable
{
    private const int HotkeyId = 0x505454;
    private const int WmHotkey = 0x0312;
    private const uint ModShift = 0x0004;
    private const uint ModWin = 0x0008;
    private const uint VkD = 0x44;

    private readonly HwndSource _source;

    public event Action? HotkeyPressed;

    public HotkeyWindow()
    {
        var parameters = new HwndSourceParameters("PicToTextHotkey")
        {
            Width = 0,
            Height = 0,
            WindowStyle = 0
        };
        _source = new HwndSource(parameters);
        _source.AddHook(WndProc);
    }

    public bool Register() => RegisterHotKey(_source.Handle, HotkeyId, ModShift | ModWin, VkD);

    private IntPtr WndProc(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (message == WmHotkey && wParam.ToInt32() == HotkeyId)
        {
            handled = true;
            HotkeyPressed?.Invoke();
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        UnregisterHotKey(_source.Handle, HotkeyId);
        _source.RemoveHook(WndProc);
        _source.Dispose();
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hwnd, int id, uint modifiers, uint virtualKey);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hwnd, int id);
}
