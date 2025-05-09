using System;

namespace WallyMapEditor;

public class PropertiesWindowData
{
    public required TimeSpan Time { get; init; }
    public required RaylibCanvas? Canvas { get; init; }
    public required AssetLoader? Loader { get; init; }
    public required PathPreferences PathPrefs { get; init; }
    public required string[]? PowerNames { get; init; }
}