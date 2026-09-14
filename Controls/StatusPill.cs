using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace CodeShare.Controls;

/// <summary>
/// A small rounded "pill" showing a colored status dot + text
/// (Connecting / Connected / In room / etc.). Auto-sizes its width to
/// the current text; call <see cref="SetStatus"/> whenever the status
/// changes, then reposition it (it does not reposition itself).
/// </summary>
internal class StatusPill : Control
{
    private string statusText = "";
    private Color dotColor = AppTheme.TextMuted;

    public StatusPill()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw,
            true);

        DoubleBuffered = true;
        Height = 30;
        Font = AppTheme.FontBodyBold;
        ForeColor = AppTheme.TextPrimary;
    }

    public void SetStatus(string text, Color color)
    {
        statusText = text;
        dotColor = color;

        var textSize = TextRenderer.MeasureText(statusText, Font);
        Width = textSize.Width + 40;

        Invalidate();
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
        using var path = RoundedPanel.RoundedRect(rect, Height / 2);

        using (var fill = new SolidBrush(AppTheme.ElevatedBackground))
            g.FillPath(fill, path);

        using (var pen = new Pen(AppTheme.Border, 1f))
            g.DrawPath(pen, path);

        const int dotSize = 8;
        int dotY = (Height - dotSize) / 2;

        using (var dotBrush = new SolidBrush(dotColor))
            g.FillEllipse(dotBrush, 14, dotY, dotSize, dotSize);

        var textRect = new Rectangle(14 + dotSize + 8, 0, Math.Max(0, Width - (14 + dotSize + 8) - 12), Height);

        TextRenderer.DrawText(
            g,
            statusText,
            Font,
            textRect,
            ForeColor,
            TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding);
    }
}
