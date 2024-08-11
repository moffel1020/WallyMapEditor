using System;
using System.Numerics;
using System.IO;
using System.Threading.Tasks;

using Raylib_cs;

using ImGuiNET;
using rlImGui_cs;

using NativeFileDialogSharp;

namespace WallyMapSpinzor2.Raylib;

public class MapOverviewWindow
{
    private bool _open = true;
    public bool Open { get => _open; set => _open = value; }

    private bool _propChanged = false;

    private string? _thumbnailSelectError;

    // type ImGuiInputTextCallback
    public unsafe static int LevelNameFilter(ImGuiInputTextCallbackData* data) => (char)data->EventChar switch
    {
        >= 'a' and <= 'z' => 0,
        >= 'A' and <= 'Z' => 0,
        >= '0' and <= '9' => 0,
        _ => 1,
    };

    public void Show(Level l, CommandHistory cmd, PathPreferences pathPrefs, AssetLoader? loader, SelectionContext selection)
    {
        ImGui.Begin("Map Overview", ref _open);

        if (_propChanged && Rl.IsMouseButtonReleased(MouseButton.Left))
        {
            _propChanged = false;
            cmd.SetAllowMerge(false);
        }

        if (l.Type is not null)
        {
            string newLevelName;
            unsafe
            {
                newLevelName = ImGuiExt.InputTextWithCallback("LevelName", l.Type.LevelName, LevelNameFilter, flags: ImGuiInputTextFlags.CallbackCharFilter);
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Warning: when exporting, the LevelName is used as the name of a new map.\nIf another map exists with that LevelName, it will be overwritten.");
            }

            if (newLevelName != l.Type.LevelName)
            {
                cmd.Add(new PropChangeCommand<string>(val => l.Type.LevelName = l.Desc.LevelName = val, l.Type.LevelName, newLevelName));
                _propChanged = true;
            }

            ImGui.Text($"LevelID: {l.Type.LevelID}");
            ImGui.Separator();

            _propChanged |= ImGuiExt.InputTextHistory("AssetName", l.Type.AssetName ?? "", val => l.Type.AssetName = val == "" ? null : val, cmd);
            if (ImGui.IsItemHovered()) ImGui.SetTooltip("Note: this doesn't do anything in game");
            _propChanged |= ImGuiExt.InputTextHistory("FileName", l.Type.FileName ?? "", val => l.Type.FileName = val == "" ? null : val, cmd);
            if (ImGui.IsItemHovered()) ImGui.SetTooltip("Note: this doesn't do anything in game");
            ImGui.Separator();
            _propChanged |= ImGuiExt.InputTextHistory("DisplayName", l.Type.DisplayName, val => l.Type.DisplayName = val, cmd);
            _propChanged |= ImGuiExt.CheckboxHistory("DevOnly", l.Type.DevOnly, val => l.Type.DevOnly = val, cmd);
            _propChanged |= ImGuiExt.CheckboxHistory("TestLevel", l.Type.TestLevel, val => l.Type.TestLevel = val, cmd);
            ImGui.Separator();

            ImGui.Text($"Playlists: {l.Playlists.Count}");
            string playlistsText = string.Join(",", l.Playlists);
            if (l.Playlists.Count != 0 && ImGui.IsItemHovered()) ImGui.SetTooltip(playlistsText);
            ImGui.SameLine();
            if (ImGui.Button("Edit##playlists")) PlaylistEditPanel.Open = true;
            if (l.Playlists.Count != 0 && ImGui.IsItemHovered()) ImGui.SetTooltip(playlistsText);
            ImGui.Separator();

            ImGui.Text("ThumbnailPNGFile: " + (l.Type.ThumbnailPNGFile ?? "None"));
            if (pathPrefs.BrawlhallaPath is not null)
            {
                string thumbnailPath = Path.Combine(pathPrefs.BrawlhallaPath, "images", "thumbnails");
                ImGui.SameLine();
                if (ImGui.Button("Select##ThumbnailPNGFile"))
                {
                    Task.Run(() =>
                    {
                        DialogResult dialogResult = Dialog.FileOpen("png,jpg", thumbnailPath);
                        if (dialogResult.IsOk)
                        {
                            string path = dialogResult.Path;
                            string newThumnailPNGFile = Path.GetRelativePath(thumbnailPath, path).Replace("\\", "/");
                            if (!Wms2RlUtils.IsInDirectory(pathPrefs.BrawlhallaPath, path))
                            {
                                _thumbnailSelectError = "Thumbnail file has to be inside the brawlhalla directory";
                            }
                            else if (newThumnailPNGFile != l.Type.ThumbnailPNGFile)
                            {
                                cmd.Add(new PropChangeCommand<string?>(val => l.Type.ThumbnailPNGFile = val, l.Type.ThumbnailPNGFile, newThumnailPNGFile));
                                _propChanged = true;
                                _thumbnailSelectError = null;
                            }
                        }
                    });
                }
                if (_thumbnailSelectError is not null)
                {
                    ImGui.PushTextWrapPos();
                    ImGui.Text("[Error]: " + _thumbnailSelectError);
                    ImGui.PopTextWrapPos();
                }

                if (loader is not null)
                {
                    Texture2DWrapper texture = loader.LoadTextureFromPath(Path.Combine(thumbnailPath, l.Type.ThumbnailPNGFile ?? "CorruptFile.png"));
                    rlImGui.ImageSize(texture.Texture, new Vector2(60 * (float)(texture.Width / texture.Height), 60));
                }
            }
            ImGui.Separator();
        }

        _propChanged |= ImGuiExt.DragFloatHistory("default SlowMult##overview", l.Desc.SlowMult, val => l.Desc.SlowMult = val, cmd, speed: 0.05);
        _propChanged |= ImGuiExt.DragIntHistory("default NumFrames##overview", l.Desc.NumFrames, val => l.Desc.NumFrames = val, cmd, minValue: 0);

        if (l.Type is not null && l.Type.LevelName != "Random")
        {
            if (ImGui.CollapsingHeader("Kill Bounds##overview"))
            {
                _propChanged |= ImGuiExt.DragIntHistory("TopKill##killbounds", l.Type.TopKill!.Value, val => l.Type.TopKill = val, cmd, minValue: 1);
                _propChanged |= ImGuiExt.DragIntHistory("BottomKill##killbounds", l.Type.BottomKill!.Value, val => l.Type.BottomKill = val, cmd, minValue: 1);
                _propChanged |= ImGuiExt.DragIntHistory("LeftKill##killbounds", l.Type.LeftKill!.Value, val => l.Type.LeftKill = val, cmd, minValue: 1);
                _propChanged |= ImGuiExt.DragIntHistory("RightKill##killbounds", l.Type.RightKill!.Value, val => l.Type.RightKill = val, cmd, minValue: 1);

                _propChanged |= ImGuiExt.CheckboxHistory("SoftTopKill", l.Type.SoftTopKill ?? true, val => l.Type.SoftTopKill = val ? null : val, cmd);
                ImGuiExt.WithDisabled(l.Type.LeftKill < 200, () =>
                {
                    _propChanged |= ImGuiExt.CheckboxHistory("HardLeftKill", l.Type.HardLeftKill ?? false, val => l.Type.HardLeftKill = val ? val : null, cmd);
                });
                ImGuiExt.WithDisabled(l.Type.RightKill < 200, () =>
                {
                    _propChanged |= ImGuiExt.CheckboxHistory("HardRightKill", l.Type.HardRightKill ?? false, val => l.Type.HardRightKill = val ? val : null, cmd);
                });
            }
        }

        if (ImGui.CollapsingHeader("Camera Bounds##overview"))
        {
            if (l.Type is not null)
            {
                _propChanged |= ImGuiExt.CheckboxHistory("FixedCamera", l.Type.FixedCamera ?? false, val => l.Type.FixedCamera = val ? val : null, cmd);
                _propChanged |= ImGuiExt.CheckboxHistory("FixedWidth", l.Type.FixedWidth ?? false, val => l.Type.FixedWidth = val ? val : null, cmd);
                _propChanged |= ImGuiExt.CheckboxHistory("ShowPlatsDuringMove", l.Type.ShowPlatsDuringMove ?? false, val => l.Type.ShowPlatsDuringMove = val ? val : null, cmd);
                ImGuiExt.WithDisabled(l.Type.ShowPlatsDuringMove == true, () =>
                {
                    _propChanged |= ImGuiExt.CheckboxHistory("ShowLavaLevelDuringMove", l.Type.ShowLavaLevelDuringMove ?? false, val => l.Type.ShowLavaLevelDuringMove = val ? val : null, cmd);
                });
            }
            _propChanged |= PropertiesWindow.ShowCameraBoundsProps(l.Desc.CameraBounds, cmd);
        }

        if (ImGui.CollapsingHeader("Spawn Bot Bounds##overview"))
        {
            _propChanged |= PropertiesWindow.ShowSpawnBotBoundsProps(l.Desc.SpawnBotBounds, cmd);
        }

        if (l.Type is not null && ImGui.CollapsingHeader("Weapon Spawn Color##overview") && l.Type.CrateColorA is not null && l.Type.CrateColorB is not null)
        {
            Color colA = ImGuiExt.ColorPicker3("Outer##crateColorA", new(l.Type.CrateColorA.Value.R, l.Type.CrateColorA.Value.G, l.Type.CrateColorA.Value.B, 255));
            Color colB = ImGuiExt.ColorPicker3("Inner##crateColorB", new(l.Type.CrateColorB.Value.R, l.Type.CrateColorB.Value.G, l.Type.CrateColorB.Value.B, 255));
            CrateColor crateColA = new(colA.R, colA.G, colA.B);
            CrateColor crateColB = new(colB.R, colB.G, colB.B);

            if (crateColA != l.Type.CrateColorA)
            {
                _propChanged = true;
                cmd.Add(new CrateColorChange(l.Type, crateColA, false));
            }

            if (crateColB != l.Type.CrateColorB)
            {
                _propChanged = true;
                cmd.Add(new CrateColorChange(l.Type, crateColB, true));
            }
        }

        if (ImGui.CollapsingHeader("Images##overview"))
        {
            ShowSelectableList(l.Desc.Backgrounds, selection, val => l.Desc.Backgrounds = val, cmd, false);
        }

        ImGui.Separator();

        void addButton<T>(string id, T[] values, Func<Maybe<T>> menu, Action<T[]> change)
        {
            if (ImGui.Button($"+##{id}"))
                ImGui.OpenPopup($"AddObject_{id}");

            if (ImGui.BeginPopup($"AddObject_{id}"))
            {
                Maybe<T> result = menu();
                if (result.TryGetValue(out T? val))
                {
                    cmd.Add(new PropChangeCommand<T[]>(change, values, [.. values, val]));
                    cmd.SetAllowMerge(false);
                    _propChanged = true;
                    selection.Object = val;
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }
        }

        ImGuiExt.HeaderWithWidget("Assets##overview", () =>
        {
            TeamScoreboard? ts = l.Desc.TeamScoreboard;
            if (ts is not null && ImGui.Selectable($"{ts.GetType().Name} {GetExtraObjectInfo(ts)}##selectable{ts.GetHashCode()}", selection.Object == ts))
            {
                selection.Object = ts;
            }
            ShowSelectableList(l.Desc.Assets, selection, val => l.Desc.Assets = val, cmd, movable: true);
            ShowSelectableList(l.Desc.LevelAnims, selection, val => l.Desc.LevelAnims = val, cmd, movable: true);
            ShowSelectableList(l.Desc.AnimatedBackgrounds, selection, val => l.Desc.AnimatedBackgrounds = val, cmd, movable: true);
            ShowSelectableList(l.Desc.LevelAnimations, selection, val => l.Desc.LevelAnimations = val, cmd, movable: true);
        },
        () => addButton("asset", l.Desc.Assets, () => AddObjectPopup.AddAssetMenu(new(0, 0)), val => l.Desc.Assets = val));

        ImGuiExt.HeaderWithWidget("Collisions##overview", () =>
        {
            ShowSelectableList(l.Desc.Collisions, selection, val => l.Desc.Collisions = val, cmd);
            ShowSelectableList(l.Desc.DynamicCollisions, selection, val => l.Desc.DynamicCollisions = val, cmd);
        },
        () => addButton("collision", l.Desc.Collisions, () => AddObjectPopup.AddCollisionMenu(new(0, 0)), val => l.Desc.Collisions = val));

        ImGuiExt.HeaderWithWidget("Respawns##overview", () =>
        {
            ShowSelectableList(l.Desc.Respawns, selection, val => l.Desc.Respawns = val, cmd);
            ShowSelectableList(l.Desc.DynamicRespawns, selection, val => l.Desc.DynamicRespawns = val, cmd);
        },
        () => addButton("respawn", l.Desc.Respawns, () => PropertiesWindow.DefaultRespawn(new(0, 0)), val => l.Desc.Respawns = val));

        ImGuiExt.HeaderWithWidget("Item Spawns##overview", () =>
        {
            ShowSelectableList(l.Desc.ItemSpawns, selection, val => l.Desc.ItemSpawns = val, cmd);
            ShowSelectableList(l.Desc.DynamicItemSpawns, selection, val => l.Desc.DynamicItemSpawns = val, cmd);
        },
        () => addButton("itemspawn", l.Desc.ItemSpawns, () => AddObjectPopup.AddItemSpawnMenu(new(0, 0)), val => l.Desc.ItemSpawns = val));

        if (ImGui.CollapsingHeader("Volumes##overview"))
        {
            ShowSelectableList(l.Desc.Volumes, selection, val => l.Desc.Volumes = val, cmd);
        }

        if (ImGui.CollapsingHeader("NavNodes##overview"))
        {
            ShowSelectableList(l.Desc.NavNodes, selection, val => l.Desc.NavNodes = val, cmd);
            ShowSelectableList(l.Desc.DynamicNavNodes, selection, val => l.Desc.DynamicNavNodes = val, cmd);
        }

        if (ImGui.CollapsingHeader("Sounds##overview"))
        {
            ShowSelectableList(l.Desc.LevelSounds, selection, val => l.Desc.LevelSounds = val, cmd);
        }

        if (ImGui.CollapsingHeader("Horde##overview"))
        {
            ShowSelectableList(l.Desc.WaveDatas, selection, val => l.Desc.WaveDatas = val, cmd);
        }

        ImGui.End();
    }

    private void ShowSelectableList<T>(T[] values, SelectionContext selection, Action<T[]> changeCommand, CommandHistory cmd, bool removable = true, bool movable = false)
        where T : notnull
    {
        for (int i = 0; i < values.Length; i++)
        {
            object? o = values[i];
            if (removable)
            {
                if (ImGui.Button($"x##{o.GetHashCode()}"))
                {
                    if (selection.Object == o) selection.Object = null;

                    T[] result = Wms2RlUtils.RemoveAt(values, i);
                    cmd.Add(new PropChangeCommand<T[]>(changeCommand, values, result));
                    cmd.SetAllowMerge(false);
                    _propChanged |= true;
                }
                ImGui.SameLine();
            }

            if (movable)
            {
                // couldn't get unicode char to work
                if (ImGui.Button($"^##{o.GetHashCode()}"))
                {
                    T[] result = Wms2RlUtils.MoveUp(values, i);
                    cmd.Add(new PropChangeCommand<T[]>(changeCommand, values, result));
                    cmd.SetAllowMerge(false);
                    _propChanged |= true;
                }
                ImGui.SameLine();
                if (ImGui.Button($"v##{o.GetHashCode()}"))
                {
                    T[] result = Wms2RlUtils.MoveDown(values, i);
                    cmd.Add(new PropChangeCommand<T[]>(changeCommand, values, result));
                    cmd.SetAllowMerge(false);
                    _propChanged |= true;
                }
                ImGui.SameLine();
            }

            if (ImGui.Selectable($"{o.GetType().Name} {GetExtraObjectInfo(o)}##selectable{o.GetHashCode()}", selection.Object == o))
                selection.Object = o;
        }
    }

    public static string GetExtraObjectInfo(object o) => o switch
    {
        Background b => $"({b.AssetName})",
        Platform p => $"({p.InstanceName})",
        AnimatedBackground ab => $"({ab.Gfx.AnimClass})",
        Gfx g => $"({g.AnimClass})",
        CustomArt ca => $"({ca.Name})",
        ColorSwap cs => $"({cs.OldColor:X8}->{cs.NewColor:X8})",
        LevelAnim la => $"({la.InstanceName})",

        MovingPlatform mp => $"({mp.PlatID})",
        Respawn r => $"({r.X:0.###}, {r.Y:0.###})",
        AbstractItemSpawn i => $"({i.X:0.###}, {i.Y:0.###}, {i.W:0.###}, {i.H:0.###})",
        AbstractCollision c => $"({c.X1:0.###}, {c.Y1:0.###}, {c.X2:0.###}, {c.Y2:0.###})",
        AbstractVolume v => $"(team {v.Team} - {v.X:0.###}, {v.Y:0.###}, {v.W:0.###}, {v.H:0.###})",
        AbstractAsset a => $"({a.AssetName ?? $"{a.X:0.###}, {a.Y:0.###}"})",
        NavNode n => $"({NavNode.NavIDToString(n.NavID, n.Type)})",

        LevelSound ls => $"({ls.SoundEventName})",

        WaveData w => $"({w.ID})",
        CustomPath cp => $"({cp.Points.Length} points)",
        Point p => $"({p.X:0.###}, {p.Y:0.###})",
        Group g => $"({g.GetCount(2)}/{g.GetCount(3)}/{g.GetCount(4)} {PropertiesWindow.BehaviorToString(g.Behavior)})",

        DynamicCollision dc => $"({dc.PlatID})",
        DynamicItemSpawn di => $"({di.PlatID})",
        DynamicRespawn dr => $"({dr.PlatID})",
        DynamicNavNode dn => $"({dn.PlatID})",

        AbstractKeyFrame kf => $"({PropertiesWindow.FirstKeyFrameNum(kf)})",
        _ => ""
    };
}