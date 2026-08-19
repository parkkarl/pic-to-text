using System.Drawing;
using System.Windows;
using System.Windows.Forms;

namespace PicToText;

public partial class App : System.Windows.Application
{
    private HotkeyWindow? _hotkeyWindow;
    private NotifyIcon? _trayIcon;
    private bool _capturing;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _hotkeyWindow = new HotkeyWindow();
        _hotkeyWindow.HotkeyPressed += BeginCapture;

        var menu = new ContextMenuStrip();
        menu.Items.Add("Capture text", null, (_, _) => BeginCapture());
        menu.Items.Add("Exit", null, (_, _) => Shutdown());

        _trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Information,
            Text = "Pic to Text — Shift+Win+D",
            ContextMenuStrip = menu,
            Visible = true
        };
        _trayIcon.DoubleClick += (_, _) => BeginCapture();

        if (!_hotkeyWindow.Register())
        {
            System.Windows.MessageBox.Show(
                "Shift+Win+D is already being used by another application.",
                "Pic to Text",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private async void BeginCapture()
    {
        if (_capturing) return;
        _capturing = true;

        try
        {
            var overlay = new CaptureOverlay();
            var area = overlay.SelectArea();
            if (area is null) return;

            using var image = ScreenCapture.Capture(area.Value);
            var text = await OcrService.RecognizeAsync(image);

            if (string.IsNullOrWhiteSpace(text))
            {
                ShowBalloon("No text found", "Try selecting a clearer or larger area.", ToolTipIcon.Warning);
                return;
            }

            System.Windows.Clipboard.SetText(text);
            ShowBalloon("Text copied", text.Length > 120 ? text[..120] + "…" : text, ToolTipIcon.Info);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(ex.Message, "Pic to Text", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _capturing = false;
        }
    }

    private void ShowBalloon(string title, string body, ToolTipIcon icon)
    {
        if (_trayIcon is null) return;
        _trayIcon.BalloonTipTitle = title;
        _trayIcon.BalloonTipText = body;
        _trayIcon.BalloonTipIcon = icon;
        _trayIcon.ShowBalloonTip(2500);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _hotkeyWindow?.Dispose();
        if (_trayIcon is not null)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
        }
        base.OnExit(e);
    }
}
