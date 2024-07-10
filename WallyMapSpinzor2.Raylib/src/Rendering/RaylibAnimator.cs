using System;
using System.Collections.Generic;
using System.IO;
using Raylib_cs;
using Rl = Raylib_cs.Raylib;
using RlColor = Raylib_cs.Color;
using SwfLib.Tags;
using SwiffCheese.Wrappers;
using WallyAnmSpinzor;

namespace WallyMapSpinzor2.Raylib;

public class RaylibAnimator(RaylibCanvas canvas, AssetLoader loader)
{
    private readonly struct BoneSprite
    {
        public required Texture2DWrapper Texture { get; init; }
        public required Transform Transform { get; init; }
        public required uint Tint { get; init; }
        public required double Opacity { get; init; }
    }

    private sealed class BoneInstance
    {
        public required string FilePath { get; init; }
        public required string OgBoneName { get; init; }
        public required string SpriteName { get; init; }
        public required AnmBone Bone { get; init; }
        public required bool Visible { get; set; }
    }

    public void DrawAnim(Gfx gfx, string animName, int frame, Transform trans, DrawPriorityEnum priority, object? caller, int loopLimit = -1)
    {
        BoneSprite[] bones = BuildAnim(gfx, animName, frame, trans, loopLimit);
        foreach (BoneSprite bone in bones)
        {
            Texture2DWrapper texture = bone.Texture;
            float tintR = bone.Tint == 0 ? 1 : ((byte)(bone.Tint >> 16) / 256f);
            float tintG = bone.Tint == 0 ? 1 : ((byte)(bone.Tint >> 8) / 256f);
            float tintB = bone.Tint == 0 ? 1 : ((byte)(bone.Tint >> 0) / 256f);
            float tintA = (float)bone.Opacity;
            canvas.DrawingQueue.Push((caller, () =>
            {
                RaylibCanvas.DrawTextureWithTransform(texture.Texture, 0, 0, texture.W, texture.H, bone.Transform, tintR: tintR, tintG: tintG, tintB: tintB, tintA: tintA);
            }
            ), (int)priority);
        }
    }

    public int? GetAnimationFrameCount(Gfx gfx, string animName)
    {
        if (gfx.AnimFile.StartsWith("SFX_"))
        {
            SwfFileData? swf = loader.LoadSwf(gfx.AnimFile);
            if (swf is null)
                return null;
            ushort spriteId = swf.SymbolClass[gfx.AnimClass];
            SwfSprite? sprite = loader.LoadSpriteFromSwf(gfx.AnimFile, spriteId);
            if (sprite is null)
                return null;
            return sprite.Frames.Length;
        }
        // anm animation
        else if (gfx.AnimFile.StartsWith("Animation_"))
        {
            if (!loader.AnmClasses.TryGetValue($"{gfx.AnimFile}/{gfx.AnimClass}", out AnmClass? anmClass))
                return null;
            AnmAnimation animation = anmClass.Animations[animName];
            return animation.Frames.Length;
        }
        return null;
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
            uint xgflags = 0;
            foreach (Gfx.AsymmetrySwapFlagEnum flag in xg.AsymmetrySwapFlags)
                xgflags |= 1u << (int)flag;
            uint ygflags = 0;
            foreach (Gfx.AsymmetrySwapFlagEnum flag in yg.AsymmetrySwapFlags)
                ygflags |= 1u << (int)flag;
            if (xgflags != ygflags)
                return false;
            if (xg.UseRightTorso != yg.UseRightTorso)
                return false;
            if (xg.UseRightJaw != yg.UseRightJaw)
                return false;
            if (xg.UseRightEyes != yg.UseRightEyes)
                return false;
            if (xg.UseRightMouth != yg.UseRightMouth)
                return false;
            if (xg.UseRightHair != yg.UseRightHair)
                return false;
            if (xg.UseRightForearm != yg.UseRightForearm)
                return false;
            if (xg.UseTrueLeftRightHands != yg.UseTrueLeftRightHands)
                return false;
            if (xg.UseRightGauntlet != yg.UseRightGauntlet)
                return false;
            if (xg.UseRightKatar != yg.UseRightKatar)
                return false;
            if (xg.BoneOverrides.Count != yg.BoneOverrides.Count)
                return false;
            foreach ((string k, string v) in xg.BoneOverrides)
                if (!yg.BoneOverrides.TryGetValue(k, out string? v_) || v != v_)
                    return false;
            foreach ((string k, string v) in yg.BoneOverrides)
                if (!xg.BoneOverrides.TryGetValue(k, out string? v_) || v != v_)
                    return false;
            // TODO: compare ColorSwap
            return true;
        }

        public int GetHashCode((Gfx, string) obj)
        {
            (Gfx gfx, string anim) = obj;
            uint flags = 0;
            foreach (Gfx.AsymmetrySwapFlagEnum flag in gfx.AsymmetrySwapFlags)
                flags |= 1u << (int)flag;
            HashCode code = new();
            code.Add((anim, gfx.AnimClass, gfx.AnimFile, gfx.AnimScale, flags));
            foreach (CustomArt ca in gfx.CustomArts)
                code.Add((ca.Right, ca.Type, ca.FileName, ca.Name));
            // use sorted dictionary for consistent pair order (is there a better way to do this?)
            foreach ((string k, string v) in new SortedDictionary<string, string>(gfx.BoneOverrides))
                code.Add((k, v));
            // TODO: hash ColorSwap
            return code.ToHashCode();
        }
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
        Rl.ClearBackground(RlColor.Blank);
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

    private (double x, double y, double w, double h)? CalculateAnimBounds(Gfx gfx, string animName, int frame, Transform trans, int loopLimit = -1)
    {
        BoneSprite[] bones = BuildAnim(gfx, animName, frame, trans, loopLimit);
        double xMin = double.MaxValue, xMax = double.MinValue, yMin = double.MaxValue, yMax = double.MinValue;
        foreach (BoneSprite bone in bones)
        {
            Transform t = bone.Transform;
            Texture2DWrapper txt = bone.Texture;
            (double, double)[] points = [t * (0, 0), t * (txt.Width, 0), t * (0, txt.Height), t * (txt.Width, txt.Height)];
            foreach ((double x, double y) in points)
            {
                xMin = Math.Min(xMin, x);
                xMax = Math.Max(xMax, x);
                yMin = Math.Min(yMin, y);
                yMax = Math.Max(yMax, y);
            }
        }
        if (xMin == double.MaxValue || xMax == double.MinValue || yMin == double.MaxValue || yMax == double.MinValue)
            return (0, 0, 0, 0);
        return (xMin, yMin, xMax - xMin, yMax - yMin);
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


    #region building
    private BoneSprite[] BuildAnim(Gfx gfx, string animName, int frame, Transform trans, int loopLimit = -1)
    {
        trans *= Transform.CreateScale(gfx.AnimScale, gfx.AnimScale);
        // swf animation
        if (gfx.AnimFile.StartsWith("SFX_"))
        {
            SwfFileData? swf = loader.LoadSwf(gfx.AnimFile);
            if (swf is null)
                return [];
            ushort spriteId = swf.SymbolClass[gfx.AnimClass];
            return BuildSwfSprite(gfx.AnimFile, spriteId, frame, gfx.AnimScale, gfx.Tint, 1, trans, loopLimit);
        }
        // anm animation
        else if (gfx.AnimFile.StartsWith("Animation_"))
        {
            if (!loader.AnmClasses.TryGetValue($"{gfx.AnimFile}/{gfx.AnimClass}", out AnmClass? anmClass))
                return [];
            AnmAnimation animation = anmClass.Animations[animName];

            if (loopLimit != -1 && Math.Abs(frame) >= loopLimit * animation.Frames.Length)
                return [];
            AnmFrame anmFrame = animation.Frames[BrawlhallaMath.SafeMod(frame, animation.Frames.Length)];

            List<BoneInstance> bones = GetBoneInstances(anmFrame.Bones, gfx);
            SetAsymBonesVisibility(bones, gfx, trans.ScaleX * trans.ScaleY < 0);

            List<BoneSprite> result = [];
            foreach (BoneInstance instance in bones)
            {
                if (!instance.Visible)
                    continue;
                SwfFileData? swf = loader.LoadSwf(instance.FilePath);
                if (swf is null)
                    continue;
                if (!swf.SymbolClass.TryGetValue(instance.SpriteName, out ushort spriteId))
                    continue;
                AnmBone bone = instance.Bone;
                Transform boneTrans = new(bone.ScaleX, bone.RotateSkew1, bone.RotateSkew0, bone.ScaleY, bone.X, bone.Y);
                result.AddRange(BuildSwfSprite(instance.FilePath, spriteId, bone.Frame - 1, gfx.AnimScale, gfx.Tint, bone.Opacity, trans * boneTrans));
            }
            return [.. result];
        }
        return [];
    }

    private List<BoneInstance> GetBoneInstances(AnmBone[] bones, Gfx gfx)
    {
        List<BoneInstance> instances = [];
        bool otherHand = false;
        string handBoneName = "";
        foreach (AnmBone bone in bones)
        {
            string boneName = loader.BoneNames[bone.Id - 1]; // bone id is 1 indexed
            (int, bool)? boneType;
            if (BoneDatabase.BoneTypeDict.TryGetValue(boneName, out (int, bool) boneType_))
                boneType = boneType_;
            else
                boneType = null;
            bool mirrored;
            if (boneType is null)
            {
                mirrored = false;
            }
            else
            {
                (int type, bool dir) = boneType.Value;
                if (type == 1 || type == 8 || type == 9 || type == 13 || type == 14 || type == 15 || type == 16 || type == 17)
                {
                    double det = bone.ScaleX * bone.ScaleY - bone.RotateSkew0 * bone.RotateSkew1;
                    mirrored = (det < 0) != dir;
                }
                else
                    mirrored = false;
            }

            string finalBoneName = boneName;
            if (gfx.BoneOverrides.TryGetValue(boneName, out string? overridenBoneName))
            {
                finalBoneName = overridenBoneName;
            }
            else if (BoneDatabase.AsymSwapDict.TryGetValue(boneName, out string? otherBoneName) &&
                Array.TrueForAll(gfx.AsymmetrySwapFlags, f => (int)f != boneType?.Item1))
            {
                finalBoneName = otherBoneName;
            }

            bool right = boneType is not null && boneType.Value.Item1 == 1 && (otherHand ? !mirrored : mirrored);
            bool isOtherHand = false;
            if (boneType is not null && boneType.Value.Item1 == 1)
            {
                isOtherHand = otherHand && handBoneName == finalBoneName;
                handBoneName = isOtherHand ? "" : finalBoneName;
                otherHand = !otherHand;
            }
            else
            {
                otherHand = false;
                handBoneName = "";
            }
            CustomArt? customArt = FindCustomArt(boneName, finalBoneName, gfx.CustomArts, right, out bool needSwfLoad);
            if (needSwfLoad)
                continue;
            string customArtSuffix = customArt is not null ? $"_{customArt.Name}" : "";
            // wtf
            if (finalBoneName == "flash.display::MovieClip")
                continue;
            bool visible = boneType switch
            {
                null => true,
                _ => boneType.Value.Item1 switch
                {
                    8 when finalBoneName == "a_Torso1R" => false,
                    1 when isOtherHand => false,
                    10 when finalBoneName == "a_WeaponFistsForearmR" => false,
                    10 when finalBoneName == "a_WeaponFistsForearmRightR" => false,
                    2 when BoneDatabase.ForearmVariantDict.ContainsValue(finalBoneName) => false,
                    12 when BoneDatabase.KatarVariantDict.ContainsValue(finalBoneName) => false,
                    _ => true,
                }
            };
            string swfPath = customArt?.FileName ?? Path.Combine("bones", $"Bones{gfx.AnimFile["Animation".Length..]}");
            instances.Add(new()
            {
                FilePath = swfPath,
                SpriteName = finalBoneName + customArtSuffix,
                OgBoneName = boneName,
                Bone = bone,
                Visible = visible,
            });
        }
        return instances;
    }

    private static void SetAsymBonesVisibility(IReadOnlyList<BoneInstance> bones, Gfx gfx, bool spriteMirrored)
    {
        bool useRightTorso = gfx.UseRightTorso;
        bool useRightJaw = gfx.UseRightJaw;
        bool useRightEyes = gfx.UseRightEyes;
        bool useRightMouth = gfx.UseRightMouth;
        bool useRightHair = gfx.UseRightHair;
        bool useRightGauntlet1 = gfx.UseRightGauntlet;
        bool useRightGauntlet2 = gfx.UseRightGauntlet;
        int rightKatarUses = gfx.UseRightKatar ? 2 : 0;
        int rightForearmUses = gfx.UseRightForearm ? 2 : 0;
        int trueLeftRightHandsUses = gfx.UseTrueLeftRightHands ? 4 : 0;
        for (int i = 0; i < bones.Count; ++i)
        {
            BoneInstance instance = bones[i];
            bool mirrored = false;
            bool hand = false;
            if (BoneDatabase.BoneTypeDict.TryGetValue(instance.OgBoneName, out (int, bool) value))
            {
                (int type, bool dir) = value;
                if (type == 1 || type == 8 || type == 9 || type == 13 || type == 14 || type == 16 || type == 17)
                {
                    double det = instance.Bone.ScaleX * instance.Bone.ScaleY - instance.Bone.RotateSkew0 * instance.Bone.RotateSkew1;
                    mirrored = (det < 0) != dir;
                }
                hand = type == 1;
            }

            void doVisibilitySwap()
            {
                if (i < bones.Count - 1)
                {
                    bones[i].Visible = mirrored == spriteMirrored;
                    bones[i + 1].Visible = mirrored != spriteMirrored;
                }
            }

            if (useRightTorso && instance.OgBoneName == "a_Torso1")
            {
                doVisibilitySwap();
                useRightTorso = false;
            }
            else if (useRightJaw && instance.OgBoneName == "a_Jaw")
            {
                doVisibilitySwap();
                useRightJaw = false;
            }
            else if (useRightEyes && instance.OgBoneName.StartsWith("a_Eyes"))
            {
                doVisibilitySwap();
                useRightEyes = false;
            }
            else if (useRightMouth && instance.OgBoneName.StartsWith("a_Mouth"))
            {
                doVisibilitySwap();
                useRightMouth = false;
            }
            else if (useRightHair && instance.OgBoneName.StartsWith("a_Hair"))
            {
                doVisibilitySwap();
                useRightHair = false;
            }
            else if (useRightGauntlet1 && instance.OgBoneName == "a_WeaponFistsForearm")
            {
                doVisibilitySwap();
                useRightGauntlet1 = false;
            }
            else if (useRightGauntlet2 && instance.OgBoneName == "a_WeaponFistsForearmRight")
            {
                doVisibilitySwap();
                useRightGauntlet2 = false;
            }
            else if (rightKatarUses > 0 && BoneDatabase.KatarVariantDict.ContainsKey(instance.OgBoneName))
            {
                doVisibilitySwap();
                rightKatarUses--;
            }
            else if (rightForearmUses > 0 && BoneDatabase.ForearmVariantDict.ContainsKey(instance.OgBoneName))
            {
                doVisibilitySwap();
                rightForearmUses--;
            }
            else if (trueLeftRightHandsUses > 0 && hand)
            {
                bones[i].Visible = (i % 2 == 0) ? !spriteMirrored : spriteMirrored;
                trueLeftRightHandsUses--;
            }
        }
    }

    private CustomArt? FindCustomArt(string ogBoneName, string boneName, CustomArt[] customArts, bool right, out bool needSwfLoad)
    {
        needSwfLoad = false;
        uint artType = BoneDatabase.ArtTypeDict.GetValueOrDefault(ogBoneName, 0u);
        for (int i = customArts.Length - 1; i >= 0; --i)
        {
            CustomArt ca = customArts[i];
            if ((right || !ca.Right) && (artType == 0 || ca.Type == 0 || ca.Type == artType))
            {
                // check that new sprite would exist
                SwfFileData? swf = loader.LoadSwf(ca.FileName);
                if (swf is null)
                {
                    needSwfLoad = true;
                    return null;
                }
                if (swf.SymbolClass.ContainsKey($"{boneName}_{ca.Name}"))
                    return ca;
            }
        }
        return null;
    }

    private BoneSprite[] BuildSwfSprite(string filePath, ushort spriteId, int frame, double animScale, uint tint, double opacity, Transform trans, int loopLimit = -1)
    {
        SwfFileData? file = loader.LoadSwf(filePath);
        if (file is null) return [];
        SwfSprite? sprite = loader.LoadSpriteFromSwf(filePath, spriteId);
        if (sprite is null) return [];

        if (loopLimit != -1 && Math.Abs(frame) >= loopLimit * sprite.Frames.Length)
            return [];
        List<BoneSprite> result = [];
        SwfSpriteFrame spriteFrame = sprite.Frames[BrawlhallaMath.SafeMod(frame, sprite.Frames.Length)];
        foreach ((_, SwfSpriteFrameLayer layer) in spriteFrame.Layers)
        {
            // is a shape
            if (file.ShapeTags.TryGetValue(layer.CharacterId, out DefineShapeXTag shape))
            {
                ushort shapeId = shape.ShapeID;
                result.AddRange(BuildSwfShape(filePath, shapeId, animScale, tint, opacity, trans * Wms2RlUtils.SwfMatrixToTransform(layer.Matrix)));
            }
            // is a sprite
            else if (file.SpriteTags.TryGetValue(layer.CharacterId, out DefineSpriteTag? childSprite))
            {
                ushort childSpriteId = childSprite.SpriteID;
                result.AddRange(BuildSwfSprite(filePath, childSpriteId, frame + layer.FrameOffset, animScale, tint, opacity, trans * Wms2RlUtils.SwfMatrixToTransform(layer.Matrix)));
            }
        }
        return [.. result];
    }

    private BoneSprite[] BuildSwfShape(string filePath, ushort shapeId, double animScale, uint tint, double opacity, Transform trans)
    {
        Texture2DWrapper? texture = loader.LoadShapeFromSwf(filePath, shapeId, animScale);
        if (texture is null) return [];
        return [new()
        {
            Texture = texture,
            Transform = trans * Transform.CreateTranslate(texture.XOff, texture.YOff),
            Tint = tint,
            Opacity = opacity,
        }];
    }
    #endregion
}