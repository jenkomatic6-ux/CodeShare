using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CodeShare.Controls;

/// <summary>
/// A borderless TextBox wrapped in a rounded, themed panel. Exposes just
/// the members Form1 actually uses (Text, MaxLength, Multiline, etc.) so
/// it's a drop-in replacement for a plain TextBox field.
/// </summary>
internal class ModernTextBox : Panel
{
    public TextBox Inner { get; }
    public int CornerRadius { get; set; } = 10;

    private bool focused;

    public ModernTextBox()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw,
            true);

        DoubleBuffered = true;
        Padding = new Padding(12, 8, 12, 8);

        Inner = new TextBox
        {
            BorderStyle = BorderStyle.None,
            BackColor = AppTheme.InputBackground,
            ForeColor = AppTheme.TextPrimary,
            Font = AppTheme.FontBody,
            Dock = DockStyle.Fill,
        };

        Inner.GotFocus += (_, _) => { focused = true; Invalidate(); };
        Inner.LostFocus += (_, _) => { focused = false; Invalidate(); };

        Controls.Add(Inner);
    }

    public string Text
    {
        get => Inner.Text;
        set => Inner.Text = value;
    }

    public int MaxLength
    {
        get => Inner.MaxLength;
        set => Inner.MaxLength = value;
    }

    public bool Multiline
    {
        get => Inner.Multiline;
        set
        {
            Inner.Multiline = value;
            Inner.ScrollBars = value ? ScrollBars.Vertical : ScrollBars.None;
        }
    }

    public bool AcceptsTab
    {
        get => Inner.AcceptsTab;
        set => Inner.AcceptsTab = value;
    }

    public string PlaceholderText
    {
        get => Inner.PlaceholderText;
        set => Inner.PlaceholderText = value;
    }

    public HorizontalAlignment TextAlignment
    {
        get => Inner.TextAlign;
        set => Inner.TextAlign = value;
    }

    public void Clear() => Inner.Clear();

    public new bool Focus() => Inner.Focus();

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(RoundedPanel.ResolveBlend(Parent));
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, Math.Max(1, Width - 1), Math.Max(1, Height - 1));
        using var path = RoundedPanel.RoundedRect(rect, CornerRadius);

        using (var fill = new SolidBrush(AppTheme.InputBackground))
            g.FillPath(fill, path);

        Color borderColor = focused ? AppTheme.Accent : AppTheme.Border;
        using var pen = new Pen(borderColor, focused ? 1.6f : 1f);
        g.DrawPath(pen, path);
    }
}
