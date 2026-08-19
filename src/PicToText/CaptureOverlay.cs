using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PicToText;

internal sealed class CaptureOverlay : Form
{
    private Point _start;
    private Point _current;
    private bool _dragging;

    public Rectangle? Selection { get; private set; }

    public CaptureOverlay()
    {
        Bounds = SystemInformation.VirtualScreen;
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        ShowInTaskbar = false;
        TopMost = true;
        DoubleBuffered = true;
        Cursor = Cursors.Cross;
        BackColor = Color.Black;
        Opacity = 0.28;
        KeyPreview = true;
    }

    public Rectangle? SelectArea()
    {
        ShowDialog();
        return Selection;
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        _start = e.Location;
        _current = e.Location;
        _dragging = true;
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (!_dragging) return;
        _current = e.Location;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (!_dragging || e.Button != MouseButtons.Left) return;
        _dragging = false;
        var local = Normalize(_start, e.Location);
        if (local.Width >= 3 && local.Height >= 3)
        {
            Selection = new Rectangle(local.X + Bounds.X, local.Y + Bounds.Y, local.Width, local.Height);
            DialogResult = DialogResult.OK;
        }
        Close();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            Selection = null;
            DialogResult = DialogResult.Cancel;
            Close();
        }
        base.OnKeyDown(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (!_dragging) return;

        var area = Normalize(_start, _current);
        using var fill = new SolidBrush(Color.FromArgb(180, Color.White));
        using var pen = new Pen(Color.FromArgb(0, 120, 215), 2) { DashStyle = DashStyle.Solid };
        e.Graphics.FillRectangle(fill, area);
        e.Graphics.DrawRectangle(pen, area);
    }

    private static Rectangle Normalize(Point a, Point b) => new(
        Math.Min(a.X, b.X),
        Math.Min(a.Y, b.Y),
        Math.Abs(a.X - b.X),
        Math.Abs(a.Y - b.Y));
}
