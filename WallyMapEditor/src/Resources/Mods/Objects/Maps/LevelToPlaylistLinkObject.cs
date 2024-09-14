using System;
using System.IO;

namespace WallyMapEditor.Mod;

public sealed class LevelToPlaylistLinkObject
{
    internal enum VersionEnum : byte
    {
        Base = 1,
        LATEST = Base,
    }

    public required string LevelName { get; set; }
    public required string[] Playlists { get; set; }

    internal static LevelToPlaylistLinkObject Get(Stream stream)
    {
        Span<byte> buf = stackalloc byte[2];
        _ = (VersionEnum)stream.GetU8();
        string levelname = stream.GetStr(buf);
        string[] playlists = stream.GetStr(buf).Split(',');
        return new()
        {
            LevelName = levelname,
            Playlists = playlists,
        };
    }

    internal void Put(Stream stream)
    {
        Span<byte> buf = stackalloc byte[2];
        stream.PutU8((byte)VersionEnum.LATEST);
        stream.PutStr(buf, LevelName);
        stream.PutStr(buf, string.Join(',', Playlists));
    }
}