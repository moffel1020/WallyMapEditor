using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using WallyAnmSpinzor;

namespace WallyMapEditor;

public class AssetLoader(string brawlPath, BoneTypes boneTypes)
{
    public string BrawlPath
    {
        get => brawlPath;
        set
        {
            brawlPath = value;
            ClearCache();
            ReloadAnmCache();
        }
    }

    public BoneTypes BoneTypes { get; set; } = boneTypes;

    public TextureCache TextureCache { get; } = new();
    public SwfFileCache SwfFileCache { get; } = new();
    public SwfShapeCache SwfShapeCache { get; } = new();
    public SwfSpriteCache SwfSpriteCache { get; } = new();
    public ConcurrentDictionary<string, AnmClass> AnmClasses { get; set; } = [];

    private void LoadAnmInThread(string name)
    {
        Task.Run(() =>
        {
            string anmPath = Path.Combine(brawlPath, "anims", $"Animation_{name}.anm");
            AnmFile anm;
            using (FileStream file = new(anmPath, FileMode.Open, FileAccess.Read))
                anm = AnmFile.CreateFrom(file);
            foreach ((string className, AnmClass @class) in anm.Classes)
            {
                AnmClasses[className] = @class;
            }
        });
    }

    public Texture2DWrapper LoadTextureFromPath(string path)
    {
        string finalPath = Path.Combine(brawlPath, "mapArt", path);
        TextureCache.Cache.TryGetValue(finalPath, out Texture2DWrapper? texture);
        if (texture is not null)
            return texture;
        TextureCache.LoadInThread(finalPath);
        return Texture2DWrapper.Default;
    }

    public static string GetRealSwfPath(string filename)
    {
        // TODO: this conversion should be done by reading BoneSources.xml
        if (filename.StartsWith("Animation_"))
            return Path.Combine("bones", "Bones_" + filename["Animation_".Length..]);
        return filename;
    }

    public SwfFileData? LoadSwf(string filePath)
    {
        string finalPath = Path.Combine(brawlPath, GetRealSwfPath(filePath));
        SwfFileCache.Cache.TryGetValue(finalPath, out SwfFileData? swf);
        if (swf is not null)
            return swf;
        SwfFileCache.LoadInThread(finalPath);
        return null;
    }

    public Texture2DWrapper? LoadShapeFromSwf(string filePath, ushort shapeId, double animScale)
    {
        SwfFileData? swf = LoadSwf(filePath);
        if (swf is null)
            return null;
        SwfShapeCache.Cache.TryGetValue(new(swf, shapeId, animScale), out Texture2DWrapper? texture);
        if (texture is not null)
            return texture;
        SwfShapeCache.LoadInThread(swf, shapeId, animScale);
        return null;
    }

    public SwfSprite? LoadSpriteFromSwf(string filePath, ushort spriteId)
    {
        SwfFileData? swf = LoadSwf(filePath);
        if (swf is null)
            return null;
        SwfSpriteCache.Cache.TryGetValue(new(swf, spriteId), out SwfSprite? sprite);
        if (sprite is not null)
            return sprite;
        SwfSpriteCache.LoadInThread(swf, spriteId);
        return null;
    }

    public const int MAX_TEXTURE_UPLOADS_PER_FRAME = 5;
    public const int MAX_SWF_TEXTURE_UPLOADS_PER_FRAME = 5;
    public void Upload()
    {
        TextureCache.Upload(MAX_TEXTURE_UPLOADS_PER_FRAME);
        SwfShapeCache.Upload(MAX_SWF_TEXTURE_UPLOADS_PER_FRAME);
    }

    public void ClearCache()
    {
        TextureCache.Clear();
        SwfShapeCache.Clear();
        SwfSpriteCache.Clear();
        SwfFileCache.Clear();
    }

    public void ReloadAnmCache()
    {
        AnmClasses.Clear();
        LoadAnmInThread("MapArtAnims");
        LoadAnmInThread("ATLA_MapArtAnims");
        LoadAnmInThread("GameModes");
    }
}