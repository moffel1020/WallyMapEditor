using System.Numerics;

namespace WallyMapSpinzor2.Raylib;

public partial class PropertiesWindow
{
    public static bool ShowItemSpawnProps(AbstractItemSpawn i, CommandHistory cmd)
    {
        bool propChanged = false;
        propChanged |= ImGuiExt.DragFloatHistory($"X##props{i.GetHashCode()}", i.X, val => i.X = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory($"Y##props{i.GetHashCode()}", i.Y, val => i.Y = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory($"W##props{i.GetHashCode()}", i.W, val => i.W = val, cmd, minValue: 1);
        propChanged |= ImGuiExt.DragFloatHistory($"H##props{i.GetHashCode()}", i.H, val => i.H = val, cmd, minValue: 1);
        return propChanged;
    }

    public static T DefaultItemSpawn<T>(Vector2 pos) where T : AbstractItemSpawn, new()
    {
        T spawn = new()
        {
            X = pos.X,
            Y = pos.Y
        };
        (spawn.W, spawn.H) = (spawn.DefaultW, spawn.DefaultH);
        if (spawn is ItemSpawn)
            spawn.W = 100;
        return spawn;
    }
}