using System;
using System.IO;
using System.Numerics;

using Rl = Raylib_cs.Raylib;
using Raylib_cs;

using WallyAnmSpinzor;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Linq;
using SwiffCheese.Wrappers;
using SwfLib.Tags;

namespace WallyMapSpinzor2.Raylib;

public partial class RaylibCanvas : ICanvas<Texture2DWrapper>
{
    private readonly string brawlPath;
    public string[] BoneNames { get; set; }

    public BucketPriorityQueue<(object?, Action)> DrawingQueue { get; } = new(Enum.GetValues<DrawPriorityEnum>().Length);
    public TextureCache TextureCache { get; } = new();
    public SwfFileCache SwfFileCache { get; } = new();
    public SwfShapeCache SwfShapeCache { get; } = new();
    public SwfSpriteCache SwfSpriteCache { get; } = new();
    public ConcurrentDictionary<string, AnmGroup> AnmGroups { get; set; } = [];
    public Matrix4x4 CameraMatrix { get; set; } = Matrix4x4.Identity;

    public RaylibCanvas(string brawlPath, string[] boneNames)
    {
        this.brawlPath = brawlPath;
        this.BoneNames = boneNames;
        LoadAnm("MapArtAnims");
        LoadAnm("ATLA_MapArtAnims");
        LoadAnm("GameModes");
    }

    private void LoadAnm(string name)
    {
        Task.Run(() =>
        {
            string anmPath = Path.Combine(brawlPath, "anims", $"Animation_{name}.anm");
            AnmFile anm;
            using (FileStream file = new(anmPath, FileMode.Open, FileAccess.Read))
                anm = AnmFile.CreateFrom(file);
            foreach ((string groupName, AnmGroup group) in anm.Groups)
            {
                AnmGroups[groupName] = group;
            }
        });
    }

    public void ClearTextureCache()
    {
        TextureCache.Clear();
    }

    public void DrawCircle(double x, double y, double radius, Color color, Transform trans, DrawPriorityEnum priority, object? caller)
    {
        // FIXME: doesn't account for transformations affecting radius (could be turned into an ellipse)
        (x, y) = trans * (x, y);

        DrawingQueue.Push((caller, () =>
        {
            Rl.DrawCircle((int)x, (int)y, (float)radius, Utils.ToRlColor(color));
        }
        ), (int)priority);
    }

    public void DrawLine(double x1, double y1, double x2, double y2, Color color, Transform trans, DrawPriorityEnum priority, object? caller)
    {
        (x1, y1) = trans * (x1, y1);
        (x2, y2) = trans * (x2, y2);

        DrawingQueue.Push((caller, () =>
        {
            Rl.DrawLine((int)x1, (int)y1, (int)x2, (int)y2, Utils.ToRlColor(color));
        }
        ), (int)priority);
    }

    public void DrawLineMultiColor(double x1, double y1, double x2, double y2, Color[] colors, Transform trans, DrawPriorityEnum priority, object? caller)
    {
        if (!Matrix4x4.Invert(CameraMatrix, out Matrix4x4 invertedMat))
            throw new ArgumentException("Camera transform is not invertible");
        Transform cam = Utils.MatrixToTransform(CameraMatrix);
        Transform inv = Utils.MatrixToTransform(invertedMat);

        (x1, y1) = cam * trans * (x1, y1);
        (x2, y2) = cam * trans * (x2, y2);
        if (x1 > x2)
        {
            (x1, x2) = (x2, x1);
            (y1, y2) = (y2, y1);
        }
        double center = (colors.Length - 1) / 2.0;
        float baseOffset = Rlgl.GetLineWidth();
        (double offX, double offY) = (y1 - y2, x2 - x1);
        (offX, offY) = BrawlhallaMath.Normalize(offX, offY);
        for (int i = 0; i < colors.Length; ++i)
        {
            double mult = baseOffset * (i - center);
            DrawLine(x1 + offX * mult, y1 + offY * mult, x2 + offX * mult, y2 + offY * mult, colors[i], inv, priority, caller);
        }
    }

    public void DrawRect(double x, double y, double w, double h, bool filled, Color color, Transform trans, DrawPriorityEnum priority, object? caller)
    {
        DrawingQueue.Push((caller, () =>
        {
            if (filled)
            {
                DrawRectWithTransform(x, y, w, h, trans, color);
            }
            else
            {
                DrawLine(x, y, x + w, y, color, trans, priority, caller);
                DrawLine(x + w, y, x + w, y + h, color, trans, priority, caller);
                DrawLine(x + w, y + h, x, y + h, color, trans, priority, caller);
                DrawLine(x, y + h, x, y, color, trans, priority, caller);
            }
        }
        ), (int)priority);
    }

    public void DrawString(double x, double y, string text, double fontSize, Color color, Transform trans, DrawPriorityEnum priority, object? caller)
    {

    }

    public void DrawTexture(string path, double x, double y, Transform trans, DrawPriorityEnum priority, object? caller)
    {
        Texture2DWrapper texture = LoadTextureFromPath(path);
        DrawingQueue.Push((caller, () =>
        {
            DrawTextureWithTransform(texture.Texture, x + texture.XOff, y + texture.YOff, texture.W, texture.H, trans, Color.FromHex(0xFFFFFFFF));
        }
        ), (int)priority);
    }

    public void DrawTextureRect(string path, double x, double y, double w, double h, Transform trans, DrawPriorityEnum priority, object? caller)
    {
        Texture2DWrapper texture = LoadTextureFromPath(path);
        DrawingQueue.Push((caller, () =>
        {
            DrawTextureWithTransform(texture.Texture, x + texture.XOff, y + texture.YOff, w, h, trans, Color.FromHex(0xFFFFFFFF));
        }
        ), (int)priority);
    }

    private static void DrawTextureWithTransform(Texture2D texture, double x, double y, double w, double h, Transform trans, Color color)
    {
        Rlgl.SetTexture(texture.Id);
        Rlgl.Begin(DrawMode.Quads);
        Rlgl.Color4ub(color.R, color.G, color.B, color.A);
        (double xMin, double yMin) = (x, y);
        (double xMax, double yMax) = (x + w, y + h);
        (double, double)[] texCoords = [(0, 0), (0, 1), (1, 1), (1, 0), (0, 0)];
        (double, double)[] points = [trans * (xMin, yMin), trans * (xMin, yMax), trans * (xMax, yMax), trans * (xMax, yMin), trans * (xMin, yMin)];
        // raylib requires that the points be in counterclockwise order
        if (Utils.IsPolygonClockwise(points))
        {
            Array.Reverse(texCoords);
            Array.Reverse(points);
        }
        for (int i = 0; i < points.Length - 1; ++i)
        {
            Rlgl.TexCoord2f((float)texCoords[i].Item1, (float)texCoords[i].Item2);
            Rlgl.Vertex2f((float)points[i].Item1, (float)points[i].Item2);
            Rlgl.TexCoord2f((float)texCoords[i + 1].Item1, (float)texCoords[i + 1].Item2);
            Rlgl.Vertex2f((float)points[i + 1].Item1, (float)points[i + 1].Item2);
        }
        Rlgl.End();
        Rlgl.SetTexture(0);
    }

    private static void DrawRectWithTransform(double x, double y, double w, double h, Transform trans, Color color)
    {
        Rlgl.Begin(DrawMode.Quads);
        Rlgl.Color4ub(color.R, color.G, color.B, color.A);
        (double xMin, double yMin) = (x, y);
        (double xMax, double yMax) = (x + w, y + h);
        (double, double)[] points = [trans * (xMin, yMin), trans * (xMin, yMax), trans * (xMax, yMax), trans * (xMax, yMin), trans * (xMin, yMin)];
        // raylib requires that the points be in counterclockwise order
        if (Utils.IsPolygonClockwise(points))
        {
            Array.Reverse(points);
        }
        for (int i = 0; i < points.Length - 1; ++i)
        {
            Rlgl.Vertex2f((float)points[i].Item1, (float)points[i].Item2);
            Rlgl.Vertex2f((float)points[i + 1].Item1, (float)points[i + 1].Item2);
        }
        Rlgl.End();
    }

    public Texture2DWrapper LoadTextureFromPath(string path)
    {
        string finalPath = Path.Combine(brawlPath, "mapArt", path);
        TextureCache.Cache.TryGetValue(finalPath, out Texture2DWrapper? texture);
        if (texture is not null) return texture;

        TextureCache.LoadImageAsync(finalPath);
        return Texture2DWrapper.Default; // placeholder white texture until the image is read from disk
    }

    public SwfFileData? LoadSwf(string filePath)
    {
        string finalPath = Path.Combine(brawlPath, filePath);
        SwfFileCache.Cache.TryGetValue(finalPath, out SwfFileData? swf);
        if (swf is not null)
            return swf;
        SwfFileCache.LoadSwfAsync(finalPath);
        return null;
    }

    public Texture2DWrapper? LoadShapeFromSwf(string filePath, ushort shapeId)
    {
        SwfFileData? swf = LoadSwf(filePath);
        if (swf is null)
            return null;
        SwfShapeCache.Cache.TryGetValue((swf, shapeId), out Texture2DWrapper? texture);
        if (texture is not null)
            return texture;
        SwfShapeCache.LoadShapeAsync(swf, shapeId);
        return null;
    }

    public SwfSprite? LoadSpriteFromSwf(string filePath, ushort spriteId)
    {
        SwfFileData? swf = LoadSwf(filePath);
        if (swf is null)
            return null;
        SwfSpriteCache.Cache.TryGetValue((swf, spriteId), out SwfSprite? sprite);
        if (sprite is not null)
            return sprite;
        SwfSpriteCache.LoadSpriteAsync(swf, spriteId);
        return null;
    }

    public const int MAX_TEXTURE_UPLOADS_PER_FRAME = 5;
    public const int MAX_SWF_TEXTURE_UPLOADS_PER_FRAME = 5;
    public void FinalizeDraw()
    {
        TextureCache.UploadImages(MAX_TEXTURE_UPLOADS_PER_FRAME);
        SwfShapeCache.UploadImages(MAX_SWF_TEXTURE_UPLOADS_PER_FRAME);

        while (DrawingQueue.Count > 0)
        {
            (_, Action drawAction) = DrawingQueue.PopMin();
            drawAction();
        }
    }

    public void DrawSwf(string swfPath, string spriteName, int frame, double x, double y, double opacity, Transform trans, DrawPriorityEnum priority, object? caller)
    {
        // wtf
        if (spriteName == "flash.display::MovieClip")
            return;
        SwfFileData? swf = LoadSwf(swfPath);
        if (swf is null)
            return;
        ushort spriteId = swf.SymbolClass[spriteName];
        DrawSwfSprite(swfPath, spriteId, frame, x, y, opacity, trans, priority, caller);
    }

    public void DrawAnim(string animFile, string animClass, string animName, int frame, double x, double y, Transform trans, DrawPriorityEnum priority, object? caller)
    {
        // swf animation
        if (animFile.StartsWith("SFX_"))
        {
            SwfFileData? swfFile = LoadSwf(animFile);
            if (swfFile is null)
                return;
            ushort spriteId = swfFile.SymbolClass[animClass];
            DrawSwfSprite(animFile, spriteId, frame, x, y, 1, trans, priority, caller);
        }
        // anm animation
        else if (animFile.StartsWith("Animation_"))
        {
            if (!AnmGroups.TryGetValue($"{animFile}/{animClass}", out AnmGroup? anmGroup))
                return;
            // anm animation
            AnmAnimation animation = anmGroup.Animations[animName];
            AnmFrame anmFrame = animation.Frames[BrawlhallaMath.SafeMod(frame, animation.Frames.Count)];
            foreach (AnmBone bone in anmFrame.Bones)
            {
                Transform boneTrans = new(bone.ScaleX, bone.RotateSkew1, bone.RotateSkew0, bone.ScaleY, bone.X, bone.Y);
                string swfBonePath = Path.Combine("bones", $"Bones{anmGroup.FileName["Animation".Length..]}");
                string spriteName = BoneNames[bone.Id - 1]; // bone id is 1 indexed
                DrawSwf(swfBonePath, spriteName, bone.Frame - 1, x, y, bone.Opacity, trans * boneTrans, priority, caller);
            }
        }
    }

    public void DrawSwfShape(string filePath, ushort shapeId, double x, double y, double opacity, Transform trans, DrawPriorityEnum priority, object? caller)
    {
        Texture2DWrapper? texture = LoadShapeFromSwf(filePath, shapeId);
        if (texture is null) return;
        DrawingQueue.Push((caller, () =>
        {
            DrawTextureWithTransform(texture.Texture, x + texture.XOff, y + texture.YOff, texture.W, texture.H, trans, Color.FromHex(0xFFFFFFFF) with { A = (byte)(255 * opacity) });
        }
        ), (int)priority);
    }

    public void DrawSwfSprite(string filePath, ushort spriteId, int frame, double x, double y, double opacity, Transform trans, DrawPriorityEnum priority, object? caller)
    {
        SwfFileData? file = LoadSwf(filePath);
        if (file is null) return;
        SwfSprite? sprite = LoadSpriteFromSwf(filePath, spriteId);
        if (sprite is null) return;
        SwfSpriteFrame spriteFrame = sprite.Frames[frame % sprite.Frames.Length];
        foreach ((_, SwfSpriteFrameLayer layer) in spriteFrame.Layers)
        {
            // is a shape
            if (file.ShapeTags.TryGetValue(layer.CharacterId, out DefineShapeXTag? shape))
            {
                ushort shapeId = shape.ShapeID;
                DrawSwfShape(filePath, shapeId, x, y, opacity, trans * Utils.SwfMatrixToTransform(layer.Matrix), priority, caller);
            }
            // is a sprite
            else if (file.SpriteTags.TryGetValue(layer.CharacterId, out DefineSpriteTag? childSprite))
            {
                ushort childSpriteId = childSprite.SpriteID;
                DrawSwfSprite(filePath, childSpriteId, frame + layer.FrameOffset, x, y, opacity, trans * Utils.SwfMatrixToTransform(layer.Matrix), priority, caller);
            }
        }
    }
}