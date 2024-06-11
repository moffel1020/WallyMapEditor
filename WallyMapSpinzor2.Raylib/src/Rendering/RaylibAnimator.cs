using System;
using System.Collections.Generic;
using System.IO;
using Raylib_cs;
using Rl = Raylib_cs.Raylib;
using SwfLib.Tags;
using SwiffCheese.Wrappers;
using WallyAnmSpinzor;

namespace WallyMapSpinzor2.Raylib;

public class RaylibAnimator(RaylibCanvas canvas, AssetLoader loader)
{
    public void DrawAnim(Gfx gfx, string animName, int frame, Transform trans, DrawPriorityEnum priority, object? caller, int loopLimit = -1)
    {
        /*
        NOTE: the game goes over the list from the end until it finds a CustomArt that matches
        this only matters for CustomArt with RIGHT and for AsymmetrySwapFlags.
        we don't need that yet so just take last.
        */
        CustomArt? customArt = gfx.CustomArts.Length == 0 ? null : gfx.CustomArts[^1];
        string customArtSuffix = customArt is not null ? $"_{customArt.Name}" : "";
        // swf animation
        if (gfx.AnimFile.StartsWith("SFX_"))
        {
            SwfFileData? swf = loader.LoadSwf(gfx.AnimFile);
            if (swf is null)
                return;
            ushort spriteId = swf.SymbolClass[gfx.AnimClass + customArtSuffix];
            DrawSwfSprite(gfx.AnimFile, spriteId, frame, gfx.AnimScale, gfx.Tint, 1, trans, priority, caller, loopLimit);
        }
        // anm animation
        else if (gfx.AnimFile.StartsWith("Animation_"))
        {
            if (!loader.AnmClasses.TryGetValue($"{gfx.AnimFile}/{gfx.AnimClass}", out AnmClass? anmClass))
                return;
            // anm animation
            AnmAnimation animation = anmClass.Animations[animName];

            if (loopLimit != -1 && Math.Abs(frame) >= loopLimit * animation.Frames.Count)
                return;

            AnmFrame anmFrame = animation.Frames[BrawlhallaMath.SafeMod(frame, animation.Frames.Count)];
            foreach (AnmBone bone in anmFrame.Bones)
            {
                Transform boneTrans = new(bone.ScaleX, bone.RotateSkew1, bone.RotateSkew0, bone.ScaleY, bone.X, bone.Y);
                string swfPath = Path.Combine("bones", $"Bones{anmClass.FileName["Animation".Length..]}");
                string spriteName = loader.BoneNames[bone.Id - 1] + customArtSuffix; // bone id is 1 indexed
                // wtf
                if (spriteName == "flash.display::MovieClip")
                    return;
                SwfFileData? swf = loader.LoadSwf(swfPath);
                if (swf is null)
                    return;
                ushort spriteId = swf.SymbolClass[spriteName];
                DrawSwfSprite(swfPath, spriteId, bone.Frame - 1, gfx.AnimScale, gfx.Tint, bone.Opacity, trans * boneTrans, priority, caller);
            }
        }
    }

    public void DrawSwfShape(string filePath, ushort shapeId, double animScale, uint tint, double opacity, Transform trans, DrawPriorityEnum priority, object? caller)
    {
        float tintR = tint == 0 ? 1 : ((byte)(tint >> 16) / 256f);
        float tintG = tint == 0 ? 1 : ((byte)(tint >> 8) / 256f);
        float tintB = tint == 0 ? 1 : ((byte)(tint >> 0) / 256f);
        float tintA = (float)opacity;
        Texture2DWrapper? texture = loader.LoadShapeFromSwf(filePath, shapeId, animScale);
        if (texture is null) return;
        canvas.DrawingQueue.Push((caller, () =>
        {
            RaylibCanvas.DrawTextureWithTransform(texture.Texture, 0, 0, texture.W, texture.H, trans * Transform.CreateTranslate(texture.XOff, texture.YOff), tintR: tintR, tintG: tintG, tintB: tintB, tintA: tintA);
        }
        ), (int)priority);
    }

    public void DrawSwfSprite(string filePath, ushort spriteId, int frame, double animScale, uint tint, double opacity, Transform trans, DrawPriorityEnum priority, object? caller, int loopLimit = -1)
    {
        SwfFileData? file = loader.LoadSwf(filePath);
        if (file is null) return;
        SwfSprite? sprite = loader.LoadSpriteFromSwf(filePath, spriteId);
        if (sprite is null) return;

        if (loopLimit != -1 && Math.Abs(frame) >= loopLimit * sprite.Frames.Length)
            return;

        SwfSpriteFrame spriteFrame = sprite.Frames[BrawlhallaMath.SafeMod(frame, sprite.Frames.Length)];
        foreach ((_, SwfSpriteFrameLayer layer) in spriteFrame.Layers)
        {
            // is a shape
            if (file.ShapeTags.TryGetValue(layer.CharacterId, out DefineShapeXTag? shape))
            {
                ushort shapeId = shape.ShapeID;
                DrawSwfShape(filePath, shapeId, animScale, tint, opacity, trans * Utils.SwfMatrixToTransform(layer.Matrix), priority, caller);
            }
            // is a sprite
            else if (file.SpriteTags.TryGetValue(layer.CharacterId, out DefineSpriteTag? childSprite))
            {
                ushort childSpriteId = childSprite.SpriteID;
                DrawSwfSprite(filePath, childSpriteId, frame + layer.FrameOffset, animScale, tint, opacity, trans * Utils.SwfMatrixToTransform(layer.Matrix), priority, caller);
            }
        }
    }

    private sealed class GfxHasher : IEqualityComparer<(Gfx, string)>
    {
        public bool Equals((Gfx, string) x, (Gfx, string) y)
        {
            (Gfx xg, string xa) = x;
            (Gfx yg, string ya) = y;
            if (xa != ya)
                return false;
            if (xg.AnimClass != yg.AnimClass)
                return false;
            if (xg.AnimFile != yg.AnimFile)
                return false;
            if (xg.AnimScale != yg.AnimScale)
                return false;
            if (xg.CustomArts.Length != yg.CustomArts.Length)
                return false;
            for (int i = 0; i < xg.CustomArts.Length; ++i)
            {
                CustomArt xca = xg.CustomArts[i];
                CustomArt yca = yg.CustomArts[i];
                if (xca.Right != yca.Right)
                    return false;
                if (xca.Type != yca.Type)
                    return false;
                if (xca.FileName != yca.FileName)
                    return false;
                if (xca.Name != yca.Name)
                    return false;
            }
            return true;
        }

        public int GetHashCode((Gfx, string) obj)
        {
            (Gfx gfx, string anim) = obj;
            int hash = (anim, gfx.AnimClass, gfx.AnimFile, gfx.AnimScale).GetHashCode();
            foreach (CustomArt ca in gfx.CustomArts)
            {
                int caHash = (ca.Right, ca.Type, ca.FileName, ca.Name).GetHashCode();
                hash = HashCode.Combine(hash, caHash);
            }
            return hash;
        }
    }

    private readonly Dictionary<(Gfx, string), RenderRect> RenderRectCache = new(new GfxHasher());
    private RenderTexture2D? Empty;

    public void ClearCache()
    {
        foreach ((_, RenderRect rect) in RenderRectCache)
            rect.Dispose();
        RenderRectCache.Clear();
        if (Empty is not null)
            Rl.UnloadRenderTexture(Empty.Value);
        Empty = null;
    }

    public Texture2D? AnimToTexture(Gfx gfx, string animName, int frame, bool withDebug = false)
    {
        (double, double, double, double)? bounds = CalculateAnimBounds(gfx, animName, frame, Transform.IDENTITY);
        if (bounds is null)
            return null;
        if (bounds == (0, 0, 0, 0))
        {
            Empty ??= Rl.LoadRenderTexture(1, 1);
            return Empty.Value.Texture;
        }
        (double x, double y, double w, double h) = bounds.Value;
        // see if render texture was already created
        if (!RenderRectCache.TryGetValue((gfx, animName), out RenderRect rect))
            rect = new(x, y, w, h);
        else
            rect.UpdateWith(x, y, w, h);
        RenderRectCache[(gfx, animName)] = rect;
        Rl.BeginTextureMode(rect.RenderTexture);
        Rl.ClearBackground(Raylib_cs.Color.Blank);
        canvas.DrawAnim(gfx, animName, frame, Transform.CreateTranslate(-rect.Rect.XMin, -rect.Rect.YMin), DrawPriorityEnum.BACKGROUND, null);
        if (withDebug)
        {
            canvas.DrawRect(rect.Rect.XMin, rect.Rect.YMin, rect.Rect.Width, rect.Rect.Height, false, Color.FromHex(0x568917FF), Transform.CreateTranslate(-rect.Rect.XMin, -rect.Rect.YMin), DrawPriorityEnum.MIDGROUND, null);
            canvas.DrawRect(x, y, w, h, false, Color.FromHex(0x1758FFFF), Transform.CreateTranslate(-rect.Rect.XMin, -rect.Rect.YMin), DrawPriorityEnum.FOREGROUND, null);
        }
        canvas.FinalizeDraw();
        Rl.EndTextureMode();
        return rect.RenderTexture.Texture;
    }

    public (double x, double y, double w, double h)? CalculateAnimBounds(Gfx gfx, string animName, int frame, Transform trans)
    {
        (double, double)[]? points = CalculateAnimPoints(gfx, animName, frame, trans);
        if (points is null)
            return null;
        double xMin = double.MaxValue, xMax = double.MinValue, yMin = double.MaxValue, yMax = double.MinValue;
        foreach ((double x, double y) in points)
        {
            xMin = Math.Min(xMin, x);
            xMax = Math.Max(xMax, x);
            yMin = Math.Min(yMin, y);
            yMax = Math.Max(yMax, y);
        }
        if (xMin == double.MaxValue || xMax == double.MinValue || yMin == double.MaxValue || yMax == double.MinValue)
            return (0, 0, 0, 0);
        return (xMin, yMin, xMax - xMin, yMax - yMin);
    }

    private (double, double)[]? CalculateAnimPoints(Gfx gfx, string animName, int frame, Transform trans)
    {
        CustomArt? customArt = gfx.CustomArts.Length == 0 ? null : gfx.CustomArts[^1];
        string customArtSuffix = customArt is not null ? $"_{customArt.Name}" : "";
        // swf animation
        if (gfx.AnimFile.StartsWith("SFX_"))
        {
            SwfFileData? swf = loader.LoadSwf(gfx.AnimFile);
            if (swf is null)
                return null;
            ushort spriteId = swf.SymbolClass[gfx.AnimClass + customArtSuffix];
            return CalculateSwfSpritePoints(gfx.AnimFile, spriteId, frame, gfx.AnimScale, trans);
        }
        // anm animation
        else if (gfx.AnimFile.StartsWith("Animation_"))
        {
            if (!loader.AnmClasses.TryGetValue($"{gfx.AnimFile}/{gfx.AnimClass}", out AnmClass? anmClass))
                return null;
            List<(double, double)> result = [];
            // anm animation
            AnmAnimation animation = anmClass.Animations[animName];
            AnmFrame anmFrame = animation.Frames[BrawlhallaMath.SafeMod(frame, animation.Frames.Count)];
            foreach (AnmBone bone in anmFrame.Bones)
            {
                Transform boneTrans = new(bone.ScaleX, bone.RotateSkew1, bone.RotateSkew0, bone.ScaleY, bone.X, bone.Y);
                string swfPath = Path.Combine("bones", $"Bones{anmClass.FileName["Animation".Length..]}");
                string spriteName = loader.BoneNames[bone.Id - 1] + customArtSuffix; // bone id is 1 indexed
                // wtf
                if (spriteName == "flash.display::MovieClip")
                    continue;
                SwfFileData? swf = loader.LoadSwf(swfPath);
                if (swf is null)
                    continue;
                ushort spriteId = swf.SymbolClass[spriteName];
                result.AddRange(CalculateSwfSpritePoints(swfPath, spriteId, bone.Frame - 1, gfx.AnimScale, trans * boneTrans) ?? []);
            }
            return [.. result];
        }
        return null;
    }

    private (double, double)[]? CalculateSwfShapePoints(string filePath, ushort shapeId, double animScale, Transform trans)
    {
        Texture2DWrapper? texture = loader.LoadShapeFromSwf(filePath, shapeId, animScale);
        if (texture is null)
            return null;
        Transform shapeTrans = trans * Transform.CreateTranslate(texture.XOff, texture.YOff);
        return [shapeTrans * (0, 0), shapeTrans * (0, texture.H), shapeTrans * (texture.W, texture.H), shapeTrans * (texture.W, 0)];
    }

    private (double, double)[]? CalculateSwfSpritePoints(string filePath, ushort spriteId, int frame, double animScale, Transform trans)
    {
        SwfFileData? file = loader.LoadSwf(filePath);
        if (file is null)
            return null;
        SwfSprite? sprite = loader.LoadSpriteFromSwf(filePath, spriteId);
        if (sprite is null)
            return null;
        SwfSpriteFrame spriteFrame = sprite.Frames[BrawlhallaMath.SafeMod(frame, sprite.Frames.Length)];
        List<(double, double)> result = [];
        foreach ((_, SwfSpriteFrameLayer layer) in spriteFrame.Layers)
        {
            // is a shape
            if (file.ShapeTags.TryGetValue(layer.CharacterId, out DefineShapeXTag? shape))
            {
                ushort shapeId = shape.ShapeID;
                result.AddRange(CalculateSwfShapePoints(filePath, shapeId, animScale, trans * Utils.SwfMatrixToTransform(layer.Matrix)) ?? []);
            }
            // is a sprite
            else if (file.SpriteTags.TryGetValue(layer.CharacterId, out DefineSpriteTag? childSprite))
            {
                ushort childSpriteId = childSprite.SpriteID;
                result.AddRange(CalculateSwfSpritePoints(filePath, childSpriteId, frame + layer.FrameOffset, animScale, trans * Utils.SwfMatrixToTransform(layer.Matrix)) ?? []);
            }
        }
        return [.. result];
    }
}