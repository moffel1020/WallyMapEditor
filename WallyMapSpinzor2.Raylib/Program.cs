using System.Collections.Generic;
using WallyMapSpinzor2.Raylib;

PathPreferences prefs = PathPreferences.Load();
RenderConfigDefault config = RenderConfigDefault.Load();

Dictionary<string, string> cmdArgs = [];
foreach (string arg in args)
{
    string[] parts = arg.Split('=', 2);
    if (parts.Length != 2) continue;
    cmdArgs[parts[0]] = parts[1];
}

if (cmdArgs.TryGetValue("--brawlPath", out string? brawlPath))
    prefs.BrawlhallaPath = brawlPath;
if (cmdArgs.TryGetValue("--levelDesc", out string? levelDesc))
    prefs.LevelDescPath = levelDesc;
if (cmdArgs.TryGetValue("--levelTypes", out string? levelTypes))
    prefs.LevelTypePath = levelTypes;
if (cmdArgs.TryGetValue("--boneTypes", out string? boneTypes))
    prefs.BoneTypesPath = boneTypes;
if (cmdArgs.TryGetValue("--brawlAir", out string? brawlAir))
    prefs.BrawlhallaAirPath = brawlAir;
if (cmdArgs.TryGetValue("--swzKey", out string? swzKey))
    prefs.DecryptionKey = swzKey;

Editor editor = new(prefs, config);
editor.Run();