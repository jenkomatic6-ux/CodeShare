using System.Drawing;

namespace CodeShare;

/// <summary>
/// Central light color palette and typography for the CodeShare UI.
/// </summary>
internal static class AppTheme
{
    // ---- Backgrounds ----
    public static readonly Color WindowBackground = Color.FromArgb(248, 250, 252);
    public static readonly Color CardBackground = Color.FromArgb(255, 255, 255);
    public static readonly Color ElevatedBackground = Color.FromArgb(241, 245, 249);
    public static readonly Color InputBackground = Color.FromArgb(248, 250, 252);
    public static readonly Color BubbleBackground = Color.FromArgb(245, 248, 251);

    // ---- Borders ----
    public static readonly Color Border = Color.FromArgb(226, 232, 240);
    public static readonly Color BorderStrong = Color.FromArgb(203, 213, 225);

    // ---- Text ----
    public static readonly Color TextPrimary = Color.FromArgb(15, 23, 42);
    public static readonly Color TextSecondary = Color.FromArgb(71, 85, 105);
    public static readonly Color TextMuted = Color.FromArgb(100, 116, 139);

    // ---- Brand / accent ----
    // Main CodeShare blue
    public static readonly Color Accent = Color.FromArgb(37, 99, 235);
    public static readonly Color AccentHover = Color.FromArgb(29, 78, 216);
    public static readonly Color AccentPressed = Color.FromArgb(30, 64, 175);
    public static readonly Color AccentMuted = Color.FromArgb(219, 234, 254);

    // ---- Status ----
    // Green = connected / successful
    public static readonly Color Success = Color.FromArgb(22, 163, 74);
    public static readonly Color Warning = Color.FromArgb(217, 119, 6);
    public static readonly Color Danger = Color.FromArgb(220, 38, 38);
    public static readonly Color DangerHover = Color.FromArgb(185, 28, 28);
    public static readonly Color DangerTint = Color.FromArgb(254, 226, 226);

    // ---- Fonts ----
    public const string UiFontFamily = "Segoe UI";
    public const string MonoFontFamily = "Consolas";

    public static readonly Font FontDisplay =
        new(UiFontFamily, 20f, FontStyle.Bold);

    public static readonly Font FontSubtitle =
        new(UiFontFamily, 9.5f, FontStyle.Regular);

    public static readonly Font FontSectionLabel =
        new(UiFontFamily, 8f, FontStyle.Bold);

    public static readonly Font FontBody =
        new(UiFontFamily, 10f, FontStyle.Regular);

    public static readonly Font FontBodyBold =
        new(UiFontFamily, 10f, FontStyle.Bold);

    public static readonly Font FontButton =
        new(UiFontFamily, 9.5f, FontStyle.Bold);

    public static readonly Font FontRoomCode =
        new(UiFontFamily, 17f, FontStyle.Bold);

    public static readonly Font FontRoomLabel =
        new(UiFontFamily, 13f, FontStyle.Bold);

    public static readonly Font FontMono =
        new(MonoFontFamily, 10f, FontStyle.Regular);

    public static readonly Font FontSmall =
        new(UiFontFamily, 8.5f, FontStyle.Regular);

    public static readonly Font FontNameTag =
        new(UiFontFamily, 9f, FontStyle.Bold);

    public static readonly Font FontTimestamp =
        new(UiFontFamily, 8f, FontStyle.Regular);

    public static readonly Font FontLogo =
        new(MonoFontFamily, 13f, FontStyle.Bold);
}