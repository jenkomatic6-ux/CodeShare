using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace CodeShare.Controls;

internal enum ButtonKind
{
    Primary,
    Secondary,
    Ghost,
    Danger,
}

/// <summary>
/// A flat, owner-drawn button with rounded corners and hover/press states.
/// Paints an opaque background first (matching whatever it sits on) so it
/// never shows square corner artifacts against a rounded card.
/// </summary>
internal class ModernButton : Button
{
    public ButtonKind Kind { get; set; } = ButtonKind.Secondary;
    public int CornerRadius { get; set; } = 10;

    private bool hovering;
    private bool pressed;

    public ModernButton()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw,
            true);

        DoubleBuffered = true;

        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        UseVisualStyleBackColor = false;

        ForeColor = AppTheme.TextPrimary;
        Font = AppTheme.FontButton;
        Cursor = Cursors.Hand;
        TextAlign = ContentAlignment.MiddleCenter;

        MouseEnter += (_, _) => { hovering = true; Invalidate(); };
        MouseLeave += (_, _) => { hovering = false; pressed = false; Invalidate(); };
        MouseDown += (_, _) => { pressed = true; Invalidate(); };
        MouseUp += (_, _) => { pressed = false; Invalidate(); };
        EnabledChanged += (_, _) => Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

        Color blend = RoundedPanel.ResolveBlend(Parent);

        using (var eraser = new SolidBrush(blend))
            g.FillRectangle(eraser, ClientRectangle);

        var (fill, border, text) = ResolveColors(blend);

        var rect = new Rectangle(0, 0, Math.Max(1, Width - 1), Math.Max(1, Height - 1));
        using var path = RoundedPanel.RoundedRect(rect, CornerRadius);

        using (var fillBrush = new SolidBrush(fill))
            g.FillPath(fillBrush, path);

        if (border is Color borderColor)
        {
            using var pen = new Pen(borderColor, 1f);
            g.DrawPath(pen, path);
        }

        TextRenderer.DrawText(
            g,
            Text,
            Font,
            ClientRectangle,
            text,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
    }

    private (Color fill, Color? border, Color text) ResolveColors(Color blend)
    {
        if (!Enabled)
        {
            return Kind switch
            {
                ButtonKind.Primary => (AppTheme.AccentMuted, (Color?)null, AppTheme.TextMuted),
                _ => (blend, AppTheme.Border, AppTheme.TextMuted),
            };
        }

        switch (Kind)
        {
            case ButtonKind.Primary:
                Color primaryFill = pressed ? AppTheme.AccentPressed : hovering ? AppTheme.AccentHover : AppTheme.Accent;
                return (primaryFill, (Color?)null, Color.White);

            case ButtonKind.Secondary:
                Color secondaryFill = pressed
                    ? AppTheme.CardBackground
                    : hovering
                        ? AppTheme.BorderStrong
                        : AppTheme.ElevatedBackground;
                return (secondaryFill, AppTheme.Border, AppTheme.TextPrimary);

            case ButtonKind.Danger:
                Color dangerFill = (pressed || hovering) ? AppTheme.DangerTint : blend;
                Color dangerText = hovering ? AppTheme.DangerHover : AppTheme.Danger;
                return (dangerFill, AppTheme.Danger, dangerText);

            case ButtonKind.Ghost:
            default:
                Color ghostFill = pressed ? AppTheme.CardBackground : hovering ? AppTheme.ElevatedBackground : blend;
                Color ghostText = hovering ? AppTheme.TextPrimary : AppTheme.TextSecondary;
                return (ghostFill, AppTheme.Border, ghostText);
        }
    }
}
