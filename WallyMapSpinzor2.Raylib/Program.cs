using WallyMapSpinzor2.Raylib;

PathPreferences prefs = PathPreferences.Load();
if (args.Length >= 4)
{
    prefs.BrawlhallaPath = args[0];
    prefs.LevelDescPath = args[1];
    prefs.LevelTypePath = args[2];
    prefs.BoneTypesPath = args[3];
}

Editor editor = new(prefs);
editor.Run();