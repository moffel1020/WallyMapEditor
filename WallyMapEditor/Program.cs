using WallyMapEditor;

CommandLineArgs cmdArgs = CommandLineArgs.CreateFromArgs(args);

PathPreferences prefs = PathPreferences.Load();
prefs.ApplyCmdlineOverrides(cmdArgs);

RenderConfigDefault config = RenderConfigDefault.Load();
config.ApplyCmdlineOverrides(cmdArgs);

Editor editor = new(prefs, config);
editor.Run();