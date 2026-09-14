using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace CodeShare.Controls;

/// <summary>
/// A small rounded gradient badge with a "&lt;/&gt;" glyph — the
/// CodeShare brand mark used in the header. Purely decorative.
/// </summary>
internal class LogoMark : Control
{
    public LogoMark()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw,
            true);

        DoubleBuffered = true;
        Size = new Size(40, 40);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(RoundedPanel.ResolveBlend(Parent));
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

        var rect = new Rectangle(0, 0, Math.Max(1, Width - 1), Math.Max(1, Height - 1));
        using var path = RoundedPanel.RoundedRect(rect, 11);

        using (var brush = new LinearGradientBrush(rect, AppTheme.AccentHover, AppTheme.AccentPressed, 45f))
            g.FillPath(brush, path);

        TextRenderer.DrawText(
            g,
            "</>",
            AppTheme.FontLogo,
            ClientRectangle,
            Color.White,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
    }
}
