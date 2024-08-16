namespace WallyMapEditor.Mod;

internal enum ObjectTypeEnum : byte
{
    Header = 1,
    NewFileInSwz = 2,
    OverwriteFileInSwz = 3,
    AddToFileInSwz = 4,
    ExtraFile = 5,
    LevelDesc = 6,
    LevelType = 7,
    LevelSetType = 8,
    LevelToPlaylistLink = 9,
}