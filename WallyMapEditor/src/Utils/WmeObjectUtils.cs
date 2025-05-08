using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Raylib_cs;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public static partial class WmeUtils
{
    // NOTE: this does not check of child of LevelDesc
    public static bool IsObjectChildOf(object? child, object? parent) => child switch
    {
        AbstractAsset a => parent == a.Parent || IsObjectChildOf(a.Parent, parent),
        AbstractCollision c => parent == c.Parent,
        AbstractItemSpawn i => parent == i.Parent,
        Respawn r => parent == r.Parent,
        NavNode n => parent == n.Parent,
        AbstractKeyFrame k => parent == k.Parent || IsObjectChildOf(k.Parent, parent),
        _ => false,
    };

    private const string CHANGE_ORPHAN_ERROR = "Attempt to change type of orphaned ";

    public static bool ObjectChangeType<T>(T obj, LevelDesc ld, CommandHistory cmd, Func<T, Maybe<T>> menu)
        where T : class
    {
        Maybe<T> maybeNew = menu(obj);
        if (!maybeNew.TryGetValue(out T? newObj))
            return false;

        cmd.Add(new SelectPropChangeCommand<T>(val =>
        {
            // we are changing from `current` to `val`
            object current = val == obj ? newObj : val == newObj ? obj : throw new UnreachableException();

            object[]? list = GetParentArray(current, ld);
            if (list is null)
            {
                Rl.TraceLog(TraceLogLevel.Error, CHANGE_ORPHAN_ERROR + current.GetType().Name);
                return;
            }

            int index = Array.FindIndex(list, e => e == current);
            if (index == -1)
            {
                Rl.TraceLog(TraceLogLevel.Error, CHANGE_ORPHAN_ERROR + current.GetType().Name);
                return;
            }

            list[index] = val;
        }, obj, newObj), false);

        return true;
    }

    private static object[]? GetParentArray(object obj, LevelDesc ld)
    {
        return obj switch
        {
            Background => ld.Backgrounds,
            AbstractAsset a => a.GetAbstractAssetParentArray(ld),
            AnimatedBackground => ld.AnimatedBackgrounds,
            LevelAnim => ld.LevelAnims,
            LevelAnimation => ld.LevelAnimations,
            LevelSound => ld.LevelSounds,
            AbstractCollision c => c.Parent?.Children ?? ld.Collisions,
            DynamicCollision => ld.DynamicCollisions,
            AbstractItemSpawn i => i.Parent?.Children ?? ld.ItemSpawns,
            DynamicItemSpawn => ld.DynamicItemSpawns,
            NavNode n => n.Parent?.Children ?? ld.NavNodes,
            DynamicNavNode => ld.DynamicNavNodes,
            Respawn r => r.Parent?.Children ?? ld.Respawns,
            DynamicRespawn => ld.DynamicRespawns,
            AbstractVolume => ld.Volumes,
            WaveData => ld.WaveDatas,
            CustomPath cp => cp.Parent?.CustomPaths,
            Point p => p.Parent?.Points,
            Group g => g.Parent?.Groups,
            _ => null,
        };
    }

    private static AbstractAsset[] GetAbstractAssetParentArray(this AbstractAsset a, LevelDesc desc) =>
        a.Parent is null
            ? desc.Assets
            : a.Parent switch
            {
                MovingPlatform mp => mp.Assets,
                Platform p when p.AssetChildren is not null => p.AssetChildren,
                _ => throw new UnreachableException(),
            };

    private static bool SetParentArray(object obj, LevelDesc ld, object[] newArray)
    {
        static void ThrowIfBad<T>(object[] arr)
        {
            foreach (object element in arr)
            {
                if (element is not T)
                    throw new InvalidOperationException("unsafe input to SetParentArray");
            }
        }

        if (obj is Background)
        {
            ThrowIfBad<Background>(newArray);
            Background[] arr = Unsafe.As<Background[]>(newArray);

            ld.Backgrounds = arr;
            return true;
        }
        else if (obj is AbstractAsset a)
        {
            ThrowIfBad<AbstractAsset>(newArray);
            AbstractAsset[] arr = Unsafe.As<AbstractAsset[]>(newArray);

            if (a.Parent is MovingPlatform mp)
            {
                mp.Assets = arr;
                return true;
            }
            else if (a.Parent is Platform p && p.AssetChildren is not null)
            {
                p.AssetChildren = arr;
                return true;
            }
            else if (a.Parent is null)
            {
                ld.Assets = arr;
                return true;
            }
            return false;
        }
        else if (obj is AnimatedBackground)
        {
            ThrowIfBad<AnimatedBackground>(newArray);
            AnimatedBackground[] arr = Unsafe.As<AnimatedBackground[]>(newArray);

            ld.AnimatedBackgrounds = arr;
            return true;
        }
        else if (obj is LevelAnim)
        {
            ThrowIfBad<LevelAnim>(newArray);
            LevelAnim[] arr = Unsafe.As<LevelAnim[]>(newArray);

            ld.LevelAnims = arr;
            return true;
        }
        else if (obj is LevelAnimation)
        {
            ThrowIfBad<LevelAnimation>(newArray);
            LevelAnimation[] arr = Unsafe.As<LevelAnimation[]>(newArray);

            ld.LevelAnimations = arr;
            return true;
        }
        else if (obj is LevelSound)
        {
            ThrowIfBad<LevelSound>(newArray);
            LevelSound[] arr = Unsafe.As<LevelSound[]>(newArray);

            ld.LevelSounds = arr;
            return true;
        }
        else if (obj is AbstractCollision c)
        {
            ThrowIfBad<AbstractCollision>(newArray);
            AbstractCollision[] arr = Unsafe.As<AbstractCollision[]>(newArray);

            if (c.Parent is not null)
                c.Parent.Children = arr;
            else
                ld.Collisions = arr;
            return true;
        }
        else if (obj is DynamicCollision)
        {
            ThrowIfBad<DynamicCollision>(newArray);
            DynamicCollision[] arr = Unsafe.As<DynamicCollision[]>(newArray);

            ld.DynamicCollisions = arr;
            return true;
        }
        else if (obj is AbstractItemSpawn i)
        {
            ThrowIfBad<AbstractItemSpawn>(newArray);
            AbstractItemSpawn[] arr = Unsafe.As<AbstractItemSpawn[]>(newArray);

            if (i.Parent is not null)
                i.Parent.Children = arr;
            else
                ld.ItemSpawns = arr;
            return true;
        }
        else if (obj is DynamicItemSpawn)
        {
            ThrowIfBad<DynamicItemSpawn>(newArray);
            DynamicItemSpawn[] arr = Unsafe.As<DynamicItemSpawn[]>(newArray);

            ld.DynamicItemSpawns = arr;
            return true;
        }
        else if (obj is NavNode n)
        {
            ThrowIfBad<NavNode>(newArray);
            NavNode[] arr = Unsafe.As<NavNode[]>(newArray);

            if (n.Parent is not null)
                n.Parent.Children = arr;
            else
                ld.NavNodes = arr;
            return true;
        }
        else if (obj is DynamicNavNode)
        {
            ThrowIfBad<DynamicNavNode>(newArray);
            DynamicNavNode[] arr = Unsafe.As<DynamicNavNode[]>(newArray);

            ld.DynamicNavNodes = arr;
            return true;
        }
        else if (obj is Respawn r)
        {
            ThrowIfBad<Respawn>(newArray);
            Respawn[] arr = Unsafe.As<Respawn[]>(newArray);

            if (r.Parent is not null)
                r.Parent.Children = arr;
            else
                ld.Respawns = arr;
            return true;
        }
        else if (obj is DynamicRespawn)
        {
            ThrowIfBad<DynamicRespawn>(newArray);
            DynamicRespawn[] arr = Unsafe.As<DynamicRespawn[]>(newArray);

            ld.DynamicRespawns = arr;
            return true;
        }
        else if (obj is AbstractVolume)
        {
            ThrowIfBad<AbstractVolume>(newArray);
            AbstractVolume[] arr = Unsafe.As<AbstractVolume[]>(newArray);

            ld.Volumes = arr;
            return true;
        }
        else if (obj is WaveData)
        {
            ThrowIfBad<WaveData>(newArray);
            WaveData[] arr = Unsafe.As<WaveData[]>(newArray);

            ld.WaveDatas = arr;
            return true;
        }
        else if (obj is CustomPath cp)
        {
            ThrowIfBad<CustomPath>(newArray);
            CustomPath[] arr = Unsafe.As<CustomPath[]>(newArray);

            if (cp.Parent is null) return false;
            cp.Parent.CustomPaths = arr;
            return true;
        }
        else if (obj is Point p)
        {
            ThrowIfBad<Point>(newArray);
            Point[] arr = Unsafe.As<Point[]>(newArray);

            if (p.Parent is null) return false;
            p.Parent.Points = arr;
            return true;
        }
        else if (obj is Group g)
        {
            ThrowIfBad<Group>(newArray);
            Group[] arr = Unsafe.As<Group[]>(newArray);

            if (g.Parent is null) return false;
            g.Parent.Groups = arr;
            return true;
        }
        return false;
    }

    private const string REMOVE_ORPHAN_ERROR = "Attempt to remove orphaned object of type ";
    private const string REMOVE_BAD_PARENT = "Removed object is not a child of its parent";

    public static bool RemoveObject(object obj, LevelDesc ld, CommandHistory cmd)
    {
        if (obj is null)
            return false;

        // special cases
        if (obj is LevelDesc || obj is Level || obj is CameraBounds || obj is SpawnBotBounds)
            return false;
        if (obj is TeamScoreboard ts)
        {
            if (ld.TeamScoreboard != ts)
            {
                Rl.TraceLog(TraceLogLevel.Error, REMOVE_ORPHAN_ERROR + obj.GetType().Name);
                return false;
            }
            cmd.Add(new PropChangeCommand<TeamScoreboard?>(val => ld.TeamScoreboard = val, ld.TeamScoreboard, ts), false);
            return true;
        }

        object[]? parentArray = GetParentArray(obj, ld);
        if (parentArray is null)
        {
            Rl.TraceLog(TraceLogLevel.Error, REMOVE_ORPHAN_ERROR + obj.GetType().Name);
            return false;
        }

        // don't delete background if it's the last one left
        if (obj is Background && parentArray.Length == 1)
            return false;

        int idx = Array.FindIndex(parentArray, val => val == obj);
        if (idx == -1)
        {
            Rl.TraceLog(TraceLogLevel.Error, REMOVE_BAD_PARENT);
            return false;
        }

        object[] removed = RemoveAt(parentArray, idx);
        cmd.Add(new ArrayRemoveCommand<object>(arr => SetParentArray(obj, ld, arr), parentArray, removed, obj), false);
        return true;
    }

    public static bool MoveObject(object obj, double dx, double dy, CommandHistory cmd)
    {
        if (obj is AbstractAsset a)
        {
            // TODO: if MovingPlatform, we might also want to move Animation's first keyframe

            cmd.Add(new PropChangeCommand<double, double>(
                (val1, val2) => (a.X, a.Y) = (val1, val2),
                a.X, a.Y,
                a.X + dx, a.Y + dy
            ));
            return true;
        }
        else if (obj is AnimatedBackground ab)
        {
            cmd.Add(new PropChangeCommand<double, double>(
                (val1, val2) => (ab.Position_X, ab.Position_Y) = (val1, val2),
                ab.Position_X, ab.Position_Y,
                ab.Position_X + dx, ab.Position_Y + dy
            ));
            return true;
        }
        else if (obj is KeyFrame k)
        {
            cmd.Add(new PropChangeCommand<double, double>(
                (val1, val2) => (k.X, k.Y) = (val1, val2),
                k.X, k.Y,
                k.X + dx, k.Y + dy
            ));
            return true;
        }
        else if (obj is LevelAnim la)
        {
            cmd.Add(new PropChangeCommand<double, double>(
                (val1, val2) => (la.X, la.Y) = (val1, val2),
                la.X, la.Y,
                la.X + dx, la.Y + dy
            ));
            return true;
        }
        else if (obj is LevelAnimation lan)
        {
            cmd.Add(new PropChangeCommand<double, double>(
                (val1, val2) => (lan.PositionX, lan.PositionY) = (val1, val2),
                lan.PositionX, lan.PositionY,
                lan.PositionX + dx, lan.PositionY + dy
            ));
            return true;
        }
        else if (obj is AbstractCollision c)
        {
            // conveniently, pressure plates are mutually exclusive with AnchorX and AnchorY
            if (c.AnchorX is null && c.AnchorY is null && c is AbstractPressurePlateCollision pc)
            {
                // TODO: update FireOffsetX and FireOffsetY?
                cmd.Add(new PropChangeCommand<double, double, double, double, double, double>(
                    (val1, val2, val3, val4, val5, val6) => (pc.X1, pc.Y1, pc.X2, pc.Y2, pc.AnimOffsetX, pc.AnimOffsetY) = (val1, val2, val3, val4, val5, val6),
                    pc.X1, pc.Y1, pc.X2, pc.Y2, pc.AnimOffsetX, pc.AnimOffsetY,
                    pc.X1 + dx, pc.Y1 + dy, pc.X2 + dx, pc.Y2 + dy, pc.AnimOffsetX + dx, pc.AnimOffsetY + dy
                ));
            }
            else
            {
                double? anchorX = c.AnchorX;
                double? newAnchorX = anchorX is not null ? anchorX.Value + dx : null;
                double? anchorY = c.AnchorY;
                double? newAnchorY = anchorY is not null ? anchorY.Value + dy : null;

                cmd.Add(new PropChangeCommand<double, double, double, double, double?, double?>(
                    (val1, val2, val3, val4, val5, val6) => (c.X1, c.Y1, c.X2, c.Y2, c.AnchorX, c.AnchorY) = (val1, val2, val3, val4, val5, val6),
                    c.X1, c.Y1, c.X2, c.Y2, c.AnchorX, c.AnchorY,
                    c.X1 + dx, c.Y1 + dy, c.X2 + dx, c.Y2 + dy, newAnchorX, newAnchorY
                ));
            }
            return true;
        }
        else if (obj is DynamicCollision dc)
        {
            cmd.Add(new PropChangeCommand<double, double>(
                (val1, val2) => (dc.X, dc.Y) = (val1, val2),
                dc.X, dc.Y,
                dc.X + dx, dc.Y + dy
            ));
            return true;
        }
        else if (obj is AbstractItemSpawn i)
        {
            cmd.Add(new PropChangeCommand<double, double>(
                (val1, val2) => (i.X, i.Y) = (val1, val2),
                i.X, i.Y,
                i.X + dx, i.Y + dy
            ));
            return true;
        }
        else if (obj is DynamicItemSpawn di)
        {
            cmd.Add(new PropChangeCommand<double, double>(
                (val1, val2) => (di.X, di.Y) = (val1, val2),
                di.X, di.Y,
                di.X + dx, di.Y + dy
            ));
            return true;
        }
        else if (obj is NavNode n)
        {
            cmd.Add(new PropChangeCommand<double, double>(
                (val1, val2) => (n.X, n.Y) = (val1, val2),
                n.X, n.Y,
                n.X + dx, n.Y + dy
            ));
            return true;
        }
        else if (obj is DynamicNavNode dn)
        {
            cmd.Add(new PropChangeCommand<double, double>(
                (val1, val2) => (dn.X, dn.Y) = (val1, val2),
                dn.X, dn.Y,
                dn.X + dx, dn.Y + dy
            ));
            return true;
        }
        else if (obj is Respawn r)
        {
            cmd.Add(new PropChangeCommand<double, double>(
                (val1, val2) => (r.X, r.Y) = (val1, val2),
                r.X, r.Y,
                r.X + dx, r.Y + dy
            ));
            return true;
        }
        else if (obj is DynamicRespawn dr)
        {
            cmd.Add(new PropChangeCommand<double, double>(
                (val1, val2) => (dr.X, dr.Y) = (val1, val2),
                dr.X, dr.Y,
                dr.X + dx, dr.Y + dy
            ));
            return true;
        }
        else if (obj is AbstractVolume v)
        {
            cmd.Add(new PropChangeCommand<int, int>(
                (val1, val2) => (v.X, v.Y) = (val1, val2),
                v.X, v.Y,
                (int)Math.Round(v.X + dx), (int)Math.Round(v.Y + dy)
            ));
            return true;
        }
        else if (obj is Point p)
        {
            cmd.Add(new PropChangeCommand<double, double>(
                (val1, val2) => (p.X, p.Y) = (val1, val2),
                p.X, p.Y,
                p.X + dx, p.Y + dy
            ));
            return true;
        }
        else if (obj is CameraBounds cb)
        {
            cmd.Add(new PropChangeCommand<double, double>(
                (val1, val2) => (cb.X, cb.Y) = (val1, val2),
                cb.X, cb.Y,
                cb.X + dx, cb.Y + dy
            ));
            return true;
        }
        else if (obj is SpawnBotBounds sb)
        {
            cmd.Add(new PropChangeCommand<double, double>(
                (val1, val2) => (sb.X, sb.Y) = (val1, val2),
                sb.X, sb.Y,
                sb.X + dx, sb.Y + dy
            ));
            return true;
        }
        else if (obj is TeamScoreboard ts)
        {
            int newTeamRedX = (int)Math.Round(ts.RedTeamX + dx);
            int newTeamBlueX = (int)Math.Round(ts.BlueTeamX + dx);
            int newY = (int)Math.Round(ts.Y + dy);
            // DoubleDigitsOnesX and DoubleDigitsTensX are offsets
            cmd.Add(new PropChangeCommand<int, int, int, double>(
                (val1, val2, val3, val4) => (ts.RedTeamX, ts.BlueTeamX, ts.Y, ts.DoubleDigitsY) = (val1, val2, val3, val4),
                ts.RedTeamX, ts.BlueTeamX, ts.Y, ts.DoubleDigitsY,
                newTeamRedX, newTeamBlueX, newY, ts.DoubleDigitsY + dy
            ));
            return true;
        }
        // this `else` exists to ensure we `return true` in every case above
        else
            return false;
    }
}