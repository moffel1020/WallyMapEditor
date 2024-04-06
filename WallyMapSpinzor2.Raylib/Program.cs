string brawlPath = args[0];
string dumpPath = args[1];
string fileName = args[2];

WallyMapSpinzor2.Raylib.Editor editor = new(brawlPath, dumpPath, fileName);
editor.Run();