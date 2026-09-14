using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CodeShare.Controls;

/// <summary>
/// A Panel that paints itself as a rounded "card": a rounded-rectangle
/// fill (<see cref="FillColor"/>) with an optional subtle border, sitting
/// on top of whatever its parent actually renders (so the corners blend
/// in rather than showing square artifacts).
/// </summary>
internal class RoundedPanel : Panel
{
    public int CornerRadius { get; set; } = 16;
    public Color FillColor { get; set; } = AppTheme.CardBackground;
    public Color? BorderColor { get; set; } = AppTheme.Border;
    public float BorderThickness { get; set; } = 1f;

    public RoundedPanel()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw,
            true);

        DoubleBuffered = true;
        ForeColor = AppTheme.TextPrimary;
    }

    /// <summary>
    /// Resolves the color a child control (or this panel itself) should
    /// blend its own rounded corners against: the rendered fill of a
    /// RoundedPanel parent, or the parent's plain BackColor otherwise.
    /// </summary>
    public static Color ResolveBlend(Control? parent) =>
        parent is RoundedPanel rp ? rp.FillColor : (parent?.BackColor ?? AppTheme.WindowBackground);

    internal static GraphicsPath RoundedRect(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();

        int d = radius * 2;

        if (d <= 0 || d >= bounds.Width || d >= bounds.Height)
        {
            path.AddRectangle(bounds);
            return path;
        }

        path.StartFigure();
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();

        return path;
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(ResolveBlend(Parent));
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, Math.Max(1, Width - 1), Math.Max(1, Height - 1));

        using var path = RoundedRect(rect, CornerRadius);

        using (var fill = new SolidBrush(FillColor))
            g.FillPath(fill, path);

        if (BorderColor is Color borderColor)
        {
            using var pen = new Pen(borderColor, BorderThickness);
            g.DrawPath(pen, path);
        }
    }
}
