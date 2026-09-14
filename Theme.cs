using System.Drawing;

namespace CodeShare;

/// <summary>
/// Central dark color palette and typography for the CodeShare UI.
/// Keeping every color/font in one place makes the "premium dark" look
/// consistent across every custom control.
/// </summary>
internal static class AppTheme
{
    // ---- Backgrounds ----
    public static readonly Color WindowBackground = Color.FromArgb(255, 10, 10, 14);
    public static readonly Color CardBackground = Color.FromArgb(255, 20, 20, 27);
    public static readonly Color ElevatedBackground = Color.FromArgb(255, 32, 32, 41);
    public static readonly Color InputBackground = Color.FromArgb(255, 26, 26, 34);
    public static readonly Color BubbleBackground = Color.FromArgb(255, 27, 27, 35);

    // ---- Borders ----
    public static readonly Color Border = Color.FromArgb(255, 42, 42, 52);
    public static readonly Color BorderStrong = Color.FromArgb(255, 58, 58, 70);

    // ---- Text ----
    public static readonly Color TextPrimary = Color.FromArgb(255, 244, 244, 247);
    public static readonly Color TextSecondary = Color.FromArgb(255, 160, 160, 173);
    public static readonly Color TextMuted = Color.FromArgb(255, 110, 110, 124);

    // ---- Brand / accent ----
    public static readonly Color Accent = Color.FromArgb(255, 106, 108, 245);
    public static readonly Color AccentHover = Color.FromArgb(255, 127, 129, 248);
    public static readonly Color AccentPressed = Color.FromArgb(255, 84, 86, 214);
    public static readonly Color AccentMuted = Color.FromArgb(255, 46, 45, 90);

    // ---- Status ----
    public static readonly Color Success = Color.FromArgb(255, 61, 214, 140);
    public static readonly Color Warning = Color.FromArgb(255, 245, 166, 53);
    public static readonly Color Danger = Color.FromArgb(255, 240, 90, 96);
    public static readonly Color DangerHover = Color.FromArgb(255, 248, 118, 123);
    public static readonly Color DangerTint = Color.FromArgb(255, 54, 26, 30);

    // ---- Fonts ----
    public const string UiFontFamily = "Segoe UI";
    public const string MonoFontFamily = "Consolas";

    public static readonly Font FontDisplay = new(UiFontFamily, 20f, FontStyle.Bold);
    public static readonly Font FontSubtitle = new(UiFontFamily, 9.5f, FontStyle.Regular);
    public static readonly Font FontSectionLabel = new(UiFontFamily, 8f, FontStyle.Bold);
    public static readonly Font FontBody = new(UiFontFamily, 10f, FontStyle.Regular);
    public static readonly Font FontBodyBold = new(UiFontFamily, 10f, FontStyle.Bold);
    public static readonly Font FontButton = new(UiFontFamily, 9.5f, FontStyle.Bold);
    public static readonly Font FontRoomCode = new(UiFontFamily, 17f, FontStyle.Bold);
    public static readonly Font FontRoomLabel = new(UiFontFamily, 13f, FontStyle.Bold);
    public static readonly Font FontMono = new(MonoFontFamily, 10f, FontStyle.Regular);
    public static readonly Font FontSmall = new(UiFontFamily, 8.5f, FontStyle.Regular);
    public static readonly Font FontNameTag = new(UiFontFamily, 9f, FontStyle.Bold);
    public static readonly Font FontTimestamp = new(UiFontFamily, 8f, FontStyle.Regular);
    public static readonly Font FontLogo = new(MonoFontFamily, 13f, FontStyle.Bold);
}
