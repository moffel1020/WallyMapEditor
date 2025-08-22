using WallyMapEditor;

CommandLineArgs cmdArgs = CommandLineArgs.CreateFromArgs(args);

PathPreferences prefs = PathPreferences.Load();
prefs.ApplyCmdlineOverrides(cmdArgs);

RenderConfigDefault config = RenderConfigDefault.Load();
config.ApplyCmdlineOverrides(cmdArgs);

RecentlyOpened recentlyOpened = RecentlyOpened.Load();

Editor editor = new(prefs, config, recentlyOpened);
editor.Run();