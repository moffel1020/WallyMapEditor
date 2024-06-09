using System;
using System.IO;
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
}