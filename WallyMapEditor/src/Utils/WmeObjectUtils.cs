using System;
using System.Diagnostics;
using System.Linq;
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
            T[]? list = GetParentArray(obj, ld);
            if (list is null)
            {
                Rl.TraceLog(TraceLogLevel.Error, CHANGE_ORPHAN_ERROR + obj.GetType().Name);
                return;
            }

            // find object to swap
            int index = Array.FindIndex(list, e => e == obj);
            // if not in current list, this means we are undoing.
            if (index == -1)
            {
                index = Array.FindIndex(list, e => e == newObj);
                // error
                if (index == -1)
                {
                    Rl.TraceLog(TraceLogLevel.Error, CHANGE_ORPHAN_ERROR + obj.GetType().Name);
                    return;
                }
            }

            list[index] = val;
        }, obj, newObj), false);

        return true;
    }

    public static T[]? GetParentArray<T>(T obj, LevelDesc ld)
    {
        object? result = obj switch
        {
            Background => ld.Backgrounds,
            AbstractAsset a => a.GetAbstractAssetParentArray(ld),
            AnimatedBackground => ld.AnimatedBackgrounds,
            LevelAnim => ld.LevelAnims,
            LevelAnimation => ld.LevelAnimations,
            LevelSound => ld.LevelSounds,
            AbstractCollision c => c.Parent?.Children ?? ld.Collisions,
            DynamicCollision => ld.DynamicCollisions,
            ItemSpawn i => i.Parent?.Children ?? ld.ItemSpawns,
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

        // C# can't infer that if obj is U, then T = U. So this hack has to be used
        return (T[]?)result;
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

    public static bool SetParentArray<T>(T obj, LevelDesc ld, T[] newArray)
    {
        if (obj is Background)
        {
            Background[] arr = [.. newArray.Cast<Background>()];
            ld.Backgrounds = arr;
            return true;
        }
        else if (obj is AbstractAsset a)
        {
            AbstractAsset[] arr = [.. newArray.Cast<AbstractAsset>()];
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
            AnimatedBackground[] arr = [.. newArray.Cast<AnimatedBackground>()];
            ld.AnimatedBackgrounds = arr;
            return true;
        }
        else if (obj is LevelAnim)
        {
            LevelAnim[] arr = [.. newArray.Cast<LevelAnim>()];
            ld.LevelAnims = arr;
            return true;
        }
        else if (obj is LevelAnimation)
        {
            LevelAnimation[] arr = [.. newArray.Cast<LevelAnimation>()];
            ld.LevelAnimations = arr;
            return true;
        }
        else if (obj is LevelSound)
        {
            LevelSound[] arr = [.. newArray.Cast<LevelSound>()];
            ld.LevelSounds = arr;
            return true;
        }
        else if (obj is AbstractCollision c)
        {
            AbstractCollision[] arr = [.. newArray.Cast<AbstractCollision>()];
            if (c.Parent is not null)
                c.Parent.Children = arr;
            else
                ld.Collisions = arr;
            return true;
        }
        else if (obj is DynamicCollision)
        {
            DynamicCollision[] arr = [.. newArray.Cast<DynamicCollision>()];
            ld.DynamicCollisions = arr;
            return true;
        }
        else if (obj is ItemSpawn i)
        {
            ItemSpawn[] arr = [.. newArray.Cast<ItemSpawn>()];
            if (i.Parent is not null)
                i.Parent.Children = arr;
            else
                ld.ItemSpawns = arr;
            return true;
        }
        else if (obj is DynamicItemSpawn)
        {
            DynamicItemSpawn[] arr = [.. newArray.Cast<DynamicItemSpawn>()];
            ld.DynamicItemSpawns = arr;
            return true;
        }
        else if (obj is NavNode n)
        {
            NavNode[] arr = [.. newArray.Cast<NavNode>()];
            if (n.Parent is not null)
                n.Parent.Children = arr;
            else
                ld.NavNodes = arr;
            return true;
        }
        else if (obj is DynamicNavNode)
        {
            DynamicNavNode[] arr = [.. newArray.Cast<DynamicNavNode>()];
            ld.DynamicNavNodes = arr;
            return true;
        }
        else if (obj is Respawn r)
        {
            Respawn[] arr = [.. newArray.Cast<Respawn>()];
            if (r.Parent is not null)
                r.Parent.Children = arr;
            else
                ld.Respawns = arr;
            return true;
        }
        else if (obj is DynamicRespawn)
        {
            DynamicRespawn[] arr = [.. newArray.Cast<DynamicRespawn>()];
            ld.DynamicRespawns = arr;
            return true;
        }
        else if (obj is AbstractVolume)
        {
            AbstractVolume[] arr = [.. newArray.Cast<AbstractVolume>()];
            ld.Volumes = arr;
            return true;
        }
        else if (obj is WaveData)
        {
            WaveData[] arr = [.. newArray.Cast<WaveData>()];
            ld.WaveDatas = arr;
            return true;
        }
        else if (obj is CustomPath cp)
        {
            CustomPath[] arr = [.. newArray.Cast<CustomPath>()];
            if (cp.Parent is null) return false;
            cp.Parent.CustomPaths = arr;
            return true;
        }
        else if (obj is Point p)
        {
            Point[] arr = [.. newArray.Cast<Point>()];
            if (p.Parent is null) return false;
            p.Parent.Points = arr;
            return true;
        }
        else if (obj is Group g)
        {
            Group[] arr = [.. newArray.Cast<Group>()];
            if (g.Parent is null) return false;
            g.Parent.Groups = arr;
            return true;
        }
        return false;
    }

    private const string REMOVE_ORPHAN_ERROR = "Attempt to remove orphaned object of type ";
    private const string REMOVE_BAD_PARENT = "Removed object is not a child of its parent";

    public static bool RemoveObject<T>(T obj, LevelDesc ld, CommandHistory cmd)
        where T : class
    {
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

        T[]? parentArray = GetParentArray(obj, ld);
        if (parentArray is null)
        {
            Rl.TraceLog(TraceLogLevel.Error, REMOVE_ORPHAN_ERROR + obj.GetType().Name);
            return false;
        }

        int idx = Array.FindIndex(parentArray, val => val == obj);
        if (idx == -1)
        {
            Rl.TraceLog(TraceLogLevel.Error, REMOVE_BAD_PARENT);
            return false;
        }

        // don't delete background if it's the last one left
        if (obj is Background && parentArray.Length == 1)
            return false;

        T[] removed = RemoveAt(parentArray, idx);
        cmd.Add(new ArrayRemoveCommand<T>(arr => SetParentArray(obj, ld, arr), parentArray, removed, obj), false);
        return true;
    }
}