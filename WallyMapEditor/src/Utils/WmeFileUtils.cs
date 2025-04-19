using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using BrawlhallaSwz;
using SkiaSharp;

namespace WallyMapEditor;

public static partial class WmeUtils
{
    public static SKBitmap LoadSKBitmap(string path)
    {
        SKImageInfo info;
        using (SKCodec codec = SKCodec.Create(path))
            info = codec.Info;
        SKImageInfo desiredInfo = info
            .WithColorType(SKColorType.Rgba8888)
            .WithAlphaType(SKAlphaType.Premul);

        SKBitmap bitmap = SKBitmap.Decode(path, desiredInfo);
        return bitmap;
    }

    public static bool IsInDirectory(string dirPath, string filePath)
    {
        string? dir = Path.GetDirectoryName(filePath);
        while (dir is not null)
        {
            if (dir == dirPath)
                return true;
            dir = Path.GetDirectoryName(dir);
        }
        return false;
    }

    public static bool IsValidBrawlPath([NotNullWhen(true)] string? path)
    {
        if (path is null) return false;

        string[] requiredFiles = ["BrawlhallaAir.swf", "Dynamic.swz", "Game.swz", "Engine.swz"];
        foreach (string name in requiredFiles)
        {
            if (!File.Exists(Path.Combine(path, name)))
            {
                return false;
            }
        }

        return true;
    }

    public static string ChangePathName(string path, Func<string, string> modifier)
    {
        string ext = Path.GetExtension(path);
        string name = Path.GetFileNameWithoutExtension(path);
        string dir = Path.GetDirectoryName(path) ?? "";
        return Path.Combine(dir, Path.ChangeExtension(modifier(name), ext));
    }

    public static string CreateBackupPath(string path, int suffix) => ChangePathName(path, s => $"{s}_Backup{suffix}");

    public static void CreateBackupOfFile(string path)
    {
        int suffix = 1;
        string backupPath;
        do
        {
            backupPath = CreateBackupPath(path, suffix);
            suffix++;
        } while (File.Exists(backupPath));
        File.Copy(path, backupPath, true);
    }

    public static string ForcePathExtension(string path, string extension)
    {
        if (!Path.HasExtension(path) || Path.GetExtension(path) != extension)
            return Path.ChangeExtension(path, extension);
        return path;
    }

    public static IEnumerable<string> GetFilesInSwz(string swzPath, uint key)
    {
        using FileStream stream = new(swzPath, FileMode.Open, FileAccess.Read);
        using SwzReader reader = new(stream, key);
        while (reader.HasNext())
            yield return reader.ReadFile();
    }

    public static string? GetFileInSwzFromPath(string swzPath, string filename, uint key) =>
        GetFilesInSwz(swzPath, key).FirstOrDefault(file => SwzUtils.GetFileName(file) == filename);

    public static void SerializeSwzFilesToPath(string swzPath, IEnumerable<string> swzFiles, uint key)
    {
        using FileStream stream = new(swzPath, FileMode.Create, FileAccess.Write);
        using SwzWriter writer = new(stream, key);
        foreach (string file in swzFiles)
            writer.WriteFile(file);
        writer.Flush();
    }

    public static bool IsSubPathOf(string path, string basePath)
    {
        path = path.Replace('\\', '/');
        path = Path.TrimEndingDirectorySeparator(path);
        string normalizedPath = Path.GetFullPath(path);

        basePath = basePath.Replace('\\', '/');
        basePath = Path.TrimEndingDirectorySeparator(basePath);
        string normalizedBasePath = Path.GetFullPath(basePath);

        return normalizedPath.StartsWith(normalizedBasePath, StringComparison.OrdinalIgnoreCase);
    }
}