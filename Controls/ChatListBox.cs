using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace CodeShare.Controls;

/// <summary>
/// One received/sent chat message. Immutable — created once when a
/// message arrives from the server and never mutated afterward.
/// </summary>
internal readonly struct ChatMessage
{
    public string Username { get; }
    public string TimeText { get; }
    public string Text { get; }

    public ChatMessage(string username, string timeText, string text)
    {
        Username = username;
        TimeText = timeText;
        Text = text;
    }

    public override string ToString() => $"[{TimeText}] {Username}: {Text}";
}

/// <summary>
/// An owner-drawn ListBox that renders each <see cref="ChatMessage"/> as
/// a rounded "bubble" card (username, timestamp, monospaced body) instead
/// of a flat block of text. Used both for the live message feed and the
/// History dialog.
/// </summary>
internal class ChatListBox : ListBox
{
    private const int BubblePadding = 12;
    private const int BubbleSpacing = 10;
    private const int SidePadding = 4;

    private int lastMeasuredWidth = -1;

    public ChatListBox()
    {
        DrawMode = DrawMode.OwnerDrawVariable;
        BorderStyle = BorderStyle.None;
        SelectionMode = SelectionMode.None;
        IntegralHeight = false;
        BackColor = AppTheme.CardBackground;
        ForeColor = AppTheme.TextPrimary;
        Font = AppTheme.FontMono;

        // We own this subclass, so (unlike a stock ListBox instance) we can
        // set the protected DoubleBuffered flag directly to smooth out
        // scrolling/repaint instead of needing the usual reflection trick.
        DoubleBuffered = true;
    }

    public void AddMessage(ChatMessage message)
    {
        Items.Add(message);

        if (Items.Count > 0)
            TopIndex = Items.Count - 1;
    }

    public void LoadMessages(IEnumerable<ChatMessage> messages)
    {
        BeginUpdate();
        Items.Clear();

        foreach (var message in messages)
            Items.Add(message);

        EndUpdate();

        if (Items.Count > 0)
            TopIndex = Items.Count - 1;
    }

    public void ClearMessages() => Items.Clear();

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        if (Width == lastMeasuredWidth || Items.Count == 0)
            return;

        lastMeasuredWidth = Width;
        RemeasureItems();
    }

    private void RemeasureItems()
    {
        try
        {
            var snapshot = new object[Items.Count];
            for (int i = 0; i < snapshot.Length; i++)
                snapshot[i] = Items[i];

            BeginUpdate();
            Items.Clear();
            Items.AddRange(snapshot);
            EndUpdate();

            if (Items.Count > 0)
                TopIndex = Items.Count - 1;
        }
        catch
        {
            // Non-critical: worst case, wrapping stays as it was until the next message.
        }
    }

    protected override void OnMeasureItem(MeasureItemEventArgs e)
    {
        base.OnMeasureItem(e);

        if (e.Index < 0 || e.Index >= Items.Count)
            return;

        var message = (ChatMessage)Items[e.Index];
        e.ItemHeight = MeasureBubbleHeight(message, e.Graphics);
    }

    private int MeasureBubbleHeight(ChatMessage message, Graphics g)
    {
        int scrollBarAllowance = SystemInformation.VerticalScrollBarWidth + 4;
        int textWidth = Math.Max(60, ClientSize.Width - (SidePadding * 2) - (BubblePadding * 2) - scrollBarAllowance);

        var bodySize = TextRenderer.MeasureText(
            g,
            string.IsNullOrEmpty(message.Text) ? " " : message.Text,
            Font,
            new Size(textWidth, int.MaxValue),
            TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);

        const int headerHeight = 20;
        int bodyHeight = Math.Max(bodySize.Height, Font.Height);

        return headerHeight + bodyHeight + (BubblePadding * 2) + BubbleSpacing;
    }

    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        if (e.Index < 0 || e.Index >= Items.Count)
        {
            base.OnDrawItem(e);
            return;
        }

        var message = (ChatMessage)Items[e.Index];
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

        using (var backBrush = new SolidBrush(AppTheme.CardBackground))
            g.FillRectangle(backBrush, e.Bounds);

        var bubbleRect = new Rectangle(
            e.Bounds.X + SidePadding,
            e.Bounds.Y,
            Math.Max(1, e.Bounds.Width - (SidePadding * 2)),
            Math.Max(1, e.Bounds.Height - BubbleSpacing));

        using (var path = RoundedPanel.RoundedRect(bubbleRect, 10))
        {
            using (var fill = new SolidBrush(AppTheme.BubbleBackground))
                g.FillPath(fill, path);

            using (var pen = new Pen(AppTheme.Border, 1f))
                g.DrawPath(pen, path);
        }

        var innerRect = new Rectangle(
            bubbleRect.X + BubblePadding,
            bubbleRect.Y + BubblePadding,
            Math.Max(1, bubbleRect.Width - (BubblePadding * 2)),
            Math.Max(1, bubbleRect.Height - (BubblePadding * 2)));

        var headerRect = new Rectangle(innerRect.X, innerRect.Y, innerRect.Width, 18);

        var nameSize = TextRenderer.MeasureText(g, message.Username, AppTheme.FontNameTag);

        TextRenderer.DrawText(
            g,
            message.Username,
            AppTheme.FontNameTag,
            new Rectangle(headerRect.X, headerRect.Y, Math.Min(headerRect.Width, nameSize.Width), headerRect.Height),
            AppTheme.Accent,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

        TextRenderer.DrawText(
            g,
            message.TimeText,
            AppTheme.FontTimestamp,
            headerRect,
            AppTheme.TextMuted,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

        var bodyRect = new Rectangle(
            innerRect.X,
            innerRect.Y + headerRect.Height + 4,
            innerRect.Width,
            Math.Max(0, innerRect.Height - headerRect.Height - 4));

        TextRenderer.DrawText(
            g,
            message.Text,
            Font,
            bodyRect,
            AppTheme.TextPrimary,
            TextFormatFlags.WordBreak | TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.NoPadding);
    }
}
