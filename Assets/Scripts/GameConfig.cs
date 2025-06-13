using UnityEngine;
using Fusion;

public static class GameConfig
{
    // General game settings
    public const string GameVersion = "1.0.0";
    public const int MaxPlayers = 8;
    public const int MinPlayers = 2;

    // Network settings
    public const float NetworkTimeout = 30f; // seconds

    // UI settings
    public const string MainMenuSceneName = "MainMenu";
    public const string LobbySceneName = "Lobby";
}

public static class UISettings
{
    public const float DisabledOpacity = 0.5f;
    public const float EnabledOpacity = 1.0f;

    // Common colors
    public static readonly Color DisabledColor = new Color(1f, 1f, 1f, DisabledOpacity);
    public static readonly Color EnabledColor = new Color(1f, 1f, 1f, EnabledOpacity);
}