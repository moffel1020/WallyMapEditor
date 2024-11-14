using System;
using System.Numerics;
using System.IO;
using System.Threading.Tasks;

using WallyMapSpinzor2;

using Raylib_cs;
using ImGuiNET;
using rlImGui_cs;
using NativeFileDialogSharp;

namespace WallyMapEditor;

public class MapOverviewWindow
{
    private bool _open = true;
    public bool Open { get => _open; set => _open = value; }

    private bool _propChanged = false;

    private string? _thumbnailSelectError;
    private string? _assetDirSelectError;

    public void Show(Level l, CommandHistory cmd, PathPreferences pathPrefs, AssetLoader? loader, SelectionContext selection)
    {
        ImGui.Begin("Map Overview", ref _open);

        if (_propChanged && Rl.IsMouseButtonReleased(MouseButton.Left))
        {
            _propChanged = false;
            cmd.SetAllowMerge(false);
        }

        _propChanged |= ImGuiExt.InputTextWithFilterHistory("LevelName", l.Desc.LevelName, val =>
        {
            l.Desc.LevelName = val;
            if (l.Type is not null) l.Type.LevelName = val;
        }, cmd);
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Warning: when exporting, the LevelName is used as the name of a new map.\nIf another map exists with that LevelName, it will be overwritten.");
        }

        ImGui.Text("AssetDir: " + l.Desc.AssetDir);
        if (pathPrefs.BrawlhallaPath is not null)
        {
            ImGui.SameLine();
            if (ImGui.Button("Select##AssetDir"))
            {
                string mapArtPath = Path.Combine(pathPrefs.BrawlhallaPath, "mapArt");
                Task.Run(() =>
                {
                    DialogResult dialogResult = Dialog.FolderPicker(mapArtPath);
                    if (dialogResult.IsOk)
                    {
                        string path = dialogResult.Path;
                        string newAssetDir = Path.GetRelativePath(mapArtPath, path).Replace("\\", "/");
                        if (!WmeUtils.IsInDirectory(pathPrefs.BrawlhallaPath, path))
                        {
                            _assetDirSelectError = "AssetDir has to be inside the brawlhalla directory";
                        }
                        else if (newAssetDir != l.Desc.AssetDir)
                        {
                            cmd.Add(new PropChangeCommand<string>(val => l.Desc.AssetDir = val, l.Desc.AssetDir, newAssetDir));
                            _propChanged = true;
                            _assetDirSelectError = null;
                            loader?.ClearCache();
                        }
                    }
                });
            }
            if (_assetDirSelectError is not null)
            {
                ImGui.PushTextWrapPos();
                ImGui.Text("[Error]: " + _assetDirSelectError);
                ImGui.PopTextWrapPos();
            }
        }

        if (l.Type is not null)
        {
            _propChanged |= ImGuiExt.InputUIntHistory("LevelID", l.Type.LevelID, val => l.Type.LevelID = val, cmd);
            ImGui.SameLine();
            ImGui.Text("(!)");
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("There must not be two maps with the same LevelID");

            if (l.Type.LevelID > LevelTypes.MAX_LEVEL_ID)
                ImGui.Text($"WARNING: LevelID must not exceed {LevelTypes.MAX_LEVEL_ID}");
            ImGui.Separator();

            _propChanged |= ImGuiExt.InputTextHistory("AssetName", l.Type.AssetName ?? "", val => l.Type.AssetName = val == "" ? null : val, cmd);
            if (ImGui.IsItemHovered()) ImGui.SetTooltip("Note: this doesn't do anything in game");

            _propChanged |= ImGuiExt.InputTextHistory("FileName", l.Type.FileName ?? "", val => l.Type.FileName = val == "" ? null : val, cmd);
            if (ImGui.IsItemHovered()) ImGui.SetTooltip("Note: this doesn't do anything in game");

            ImGui.Separator();

            _propChanged |= ImGuiExt.InputTextHistory("DisplayName", l.Type.DisplayName, val => l.Type.DisplayName = val, cmd);
            ImGuiExt.HintTooltip(Strings.UI_DISPLAY_NAME_TOOLTIP);

            _propChanged |= ImGuiExt.CheckboxHistory("DevOnly", l.Type.DevOnly, val => l.Type.DevOnly = val, cmd);

            _propChanged |= ImGuiExt.CheckboxHistory("TestLevel", l.Type.TestLevel, val => l.Type.TestLevel = val, cmd);
            ImGuiExt.HintTooltip(Strings.UI_TEST_LEVEL_TOOLTIP);

            _propChanged |= ImGuiExt.DragNullableUIntHistory("MinNumOnlineGamesBeforeRandom", l.Type.MinNumOnlineGamesBeforeRandom, 0, val => l.Type.MinNumOnlineGamesBeforeRandom = val, cmd, speed: 0.1f);

            ImGui.Separator();

            ImGui.Text($"Playlists: {l.Playlists.Count}");
            string playlistsText = string.Join(",", l.Playlists);
            if (l.Playlists.Count != 0 && ImGui.IsItemHovered()) ImGui.SetTooltip(playlistsText);
            ImGui.SameLine();
            if (ImGui.Button("Edit##playlists")) PlaylistEditPanel.Open = true;
            if (l.Playlists.Count != 0 && ImGui.IsItemHovered()) ImGui.SetTooltip(playlistsText);
            ImGui.Separator();

            ImGui.Text("ThumbnailPNGFile: " + (l.Type.ThumbnailPNGFile ?? "None"));
            ImGuiExt.HintTooltip(Strings.UI_LEVEL_THUMBNAIL_TOOLTIP);
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
                            if (!WmeUtils.IsInDirectory(pathPrefs.BrawlhallaPath, path))
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

            _propChanged |= ImGuiExt.CheckboxHistory("NegateOverlaps##overview", l.Type.NegateOverlaps ?? false, val => l.Type.NegateOverlaps = !val ? null : val, cmd);
            _propChanged |= ImGuiExt.DragUIntHistory("Extra StartFrame##overview", l.Type.StartFrame ?? 0, val => l.Type.StartFrame = val == 0 ? null : val, cmd);
            ImGuiExt.HintTooltip(Strings.UI_EXTRA_SF_TOOLTIP);
        }

        _propChanged |= ImGuiExt.DragDoubleHistory("Default SlowMult##overview", l.Desc.SlowMult, val => l.Desc.SlowMult = val, cmd, speed: 0.05f);
        _propChanged |= ImGuiExt.DragIntHistory("Default NumFrames##overview", l.Desc.NumFrames, val => l.Desc.NumFrames = val, cmd, minValue: 0);

        if (l.Type is not null && l.Type.LevelName != "Random")
        {
            if (ImGui.CollapsingHeader("Kill Bounds##overview"))
            {
                ImGui.Text("Kill bounds");
                ImGuiExt.HintTooltip(Strings.UI_KILL_BOUNDS_TOOLTIP);

                _propChanged |= ImGuiExt.DragUIntHistory("TopKill##killbounds", l.Type.TopKill!.Value, val => l.Type.TopKill = val, cmd);
                _propChanged |= ImGuiExt.DragUIntHistory("BottomKill##killbounds", l.Type.BottomKill!.Value, val => l.Type.BottomKill = val, cmd);
                _propChanged |= ImGuiExt.DragUIntHistory("LeftKill##killbounds", l.Type.LeftKill!.Value, val => l.Type.LeftKill = val, cmd);
                _propChanged |= ImGuiExt.DragUIntHistory("RightKill##killbounds", l.Type.RightKill!.Value, val => l.Type.RightKill = val, cmd);

                _propChanged |= ImGuiExt.CheckboxHistory("SoftTopKill", l.Type.SoftTopKill ?? true, val => l.Type.SoftTopKill = val ? null : val, cmd);
                ImGuiExt.HintTooltip(Strings.UI_SOFT_TOP_TOOLTIP);
                using (ImGuiExt.DisabledIf(l.Type.LeftKill < 200))
                    _propChanged |= ImGuiExt.CheckboxHistory("HardLeftKill", l.Type.HardLeftKill ?? false, val => l.Type.HardLeftKill = val ? val : null, cmd);
                ImGuiExt.HintTooltip(Strings.UI_HARD_LEFT_TOOLTIP);
                using (ImGuiExt.DisabledIf(l.Type.RightKill < 200))
                    _propChanged |= ImGuiExt.CheckboxHistory("HardRightKill", l.Type.HardRightKill ?? false, val => l.Type.HardRightKill = val ? val : null, cmd);
                ImGuiExt.HintTooltip(Strings.UI_HARD_RIGHT_TOOLTIP);
            }
        }

        if (ImGui.CollapsingHeader("Camera##overview"))
        {
            ImGui.Text("Camera bounds");
            ImGuiExt.HintTooltip(Strings.UI_CAMERA_BOUNDS_TOOLTIP);
            _propChanged |= PropertiesWindow.ShowCameraBoundsProps(l.Desc.CameraBounds, cmd);
            if (l.Type is not null)
            {
                _propChanged |= ImGuiExt.CheckboxHistory("FixedCamera", l.Type.FixedCamera ?? false, val => l.Type.FixedCamera = val ? val : null, cmd);
                _propChanged |= ImGuiExt.CheckboxHistory("FixedWidth", l.Type.FixedWidth ?? false, val => l.Type.FixedWidth = val ? val : null, cmd);
                _propChanged |= ImGuiExt.CheckboxHistory("ShowPlatsDuringMove", l.Type.ShowPlatsDuringMove ?? false, val => l.Type.ShowPlatsDuringMove = val ? val : null, cmd);
                using (ImGuiExt.DisabledIf(l.Type.ShowPlatsDuringMove == true))
                    _propChanged |= ImGuiExt.CheckboxHistory("ShowLavaLevelDuringMove", l.Type.ShowLavaLevelDuringMove ?? false, val => l.Type.ShowLavaLevelDuringMove = val ? val : null, cmd);
            }
        }

        if (l.Type is not null && ImGui.CollapsingHeader("Music##overview"))
        {
            _propChanged |= ImGuiExt.InputNullableTextWithFilterHistory("BGMusic", l.Type.BGMusic, "Level09Theme", val => l.Type.BGMusic = val, cmd, 64);
            _propChanged |= ImGuiExt.InputNullableTextWithFilterHistory("StreamerBGMusic", l.Type.StreamerBGMusic, "Level09Theme", val => l.Type.StreamerBGMusic = val, cmd, 64);
            ImGuiExt.HintTooltip(Strings.UI_STREAMER_BG_MUSIC_TOOLTIP);
        }

        if (ImGui.CollapsingHeader("Sidekick Bounds"))
        {
            ImGui.Text("Spawn bot bounds");
            ImGuiExt.HintTooltip(Strings.UI_SIDEKICK_BOUNDS_TOOLTIP);
            _propChanged |= PropertiesWindow.ShowSpawnBotBoundsProps(l.Desc.SpawnBotBounds, cmd);
        }

        if (l.Type is not null && ImGui.CollapsingHeader("Bot Behavior##overview"))
        {
            _propChanged |= ImGuiExt.CheckboxHistory("IsClimbMap", l.Type.IsClimbMap ?? false, val => l.Type.IsClimbMap = val ? val : null, cmd);
            ImGuiExt.HintTooltip(Strings.UI_IS_CLIMB_MAP_TOOLTIP);
            _propChanged |= ImGuiExt.CheckboxHistory("AIStrictRecover", l.Type.AIStrictRecover ?? false, val => l.Type.AIStrictRecover = val ? val : null, cmd);
            ImGuiExt.HintTooltip(Strings.UI_STRICT_RECOVER_TOOLTIP);
            _propChanged |= ImGuiExt.DragNullableDoubleHistory("AIPanicLine", l.Type.AIPanicLine, 0, val => l.Type.AIPanicLine = val, cmd);
            _propChanged |= ImGuiExt.DragNullableDoubleHistory("AIGroundLine", l.Type.AIGroundLine, 0, val => l.Type.AIGroundLine = val, cmd);
        }

        if (l.Type is not null && ImGui.CollapsingHeader("Colors##overview"))
        {
            WmsColor? crateToWms(CrateColor? c) => c is null ? null : new(c.Value.R, c.Value.G, c.Value.B, 255);
            CrateColor? wmsToCrate(WmsColor? c) => c is null ? null : new(c.Value.R, c.Value.G, c.Value.B);

            // defaults taken from Brawlhaven
            ImGui.SeparatorText("Weapon spawn");
            _propChanged |= ImGuiExt.NullableColorPicker3History("Outer", crateToWms(l.Type.CrateColorA), WmsColor.FromHex(0xff7c5b), val => l.Type.CrateColorA = wmsToCrate(val), cmd);
            _propChanged |= ImGuiExt.NullableColorPicker3History("Inner", crateToWms(l.Type.CrateColorB), WmsColor.FromHex(0xffc1b3), val => l.Type.CrateColorB = wmsToCrate(val), cmd);

            ImGui.SeparatorText("Midground");
            ImGuiExt.HintTooltip(Strings.UI_MIDGROUND_SECTION_TOOLTIP);
            _propChanged |= ImGuiExt.ColorPicker3HexHistory("Midground tint", l.Type.MidgroundTint ?? 0, val => l.Type.MidgroundTint = val, cmd);
            _propChanged |= ImGuiExt.DragDoubleHistory("Midground tint fraction", l.Type.MidgroundFraction ?? 0, val => l.Type.MidgroundFraction = val == 0 ? null : val, cmd, minValue: 0, maxValue: 1, speed: 0.05f);
            _propChanged |= ImGuiExt.ColorPicker3HexHistory("Midground tint offset", l.Type.MidgroundOffset ?? 0, val => l.Type.MidgroundOffset = val, cmd);

            ImGui.SeparatorText("Sidekick");
            _propChanged |= ImGuiExt.ColorPicker3HexHistory("Sidekick tint", l.Type.BotTint ?? 0, val => l.Type.BotTint = val, cmd);
            _propChanged |= ImGuiExt.DragDoubleHistory("Sidekick tint fraction", l.Type.BotFraction ?? 0.5, val => l.Type.BotFraction = val == 0.5 ? null : val, cmd, minValue: 0, maxValue: 1, speed: 0.05f);
            _propChanged |= ImGuiExt.ColorPicker3HexHistory("Sidekick tint offset", l.Type.BotOffset ?? 0, val => l.Type.BotOffset = val, cmd);

            ImGui.SeparatorText("Color exclusions");
            _propChanged |= ImGuiExt.GenericStringComboHistory(
                "AvoidTeamColor", l.Type.AvoidTeamColor, val => l.Type.AvoidTeamColor = val,
                c => c == TeamColorEnum.Default ? "None" : c.ToString(),
                s => Enum.TryParse(s, out TeamColorEnum e) ? e : TeamColorEnum.Default,
                Enum.GetValues<TeamColorEnum>(), cmd
            );
            ImGuiExt.HintTooltip(Strings.UI_AVOID_TEAM_COLOR_TOOLTIP);
            ImGui.Text("TeamColorOrder");
            ImGuiExt.HintTooltip(Strings.UI_TEAM_COLOR_ORDER_TOOLTIP);
            _propChanged |= TeamColorOrder(l.Type.TeamColorOrder, val => l.Type.TeamColorOrder = val, cmd);

            ImGui.SeparatorText("Misc");
            _propChanged |= ImGuiExt.ColorPicker3HexHistory("Shadow Tint##overview", (uint)(l.Type.ShadowTint ?? 0), val => l.Type.ShadowTint = val == 0 ? null : (int)val, cmd);
            ImGuiExt.HintTooltip(Strings.UI_SHADOW_TINT_TOOLTIP);
        }

        if (ImGui.CollapsingHeader("Backgrounds##overview"))
        {
            ShowSelectableList(l.Desc.Backgrounds, selection, val => l.Desc.Backgrounds = val, cmd, false);
        }

        ImGui.Separator();

        void addButton(string id, Action<double, double> menu)
        {
            if (ImGui.Button($"+##{id}"))
                ImGui.OpenPopup($"AddObject_{id}");

            if (ImGui.BeginPopup($"AddObject_{id}"))
            {
                menu(l.Desc.CameraBounds.X + l.Desc.CameraBounds.W / 2, l.Desc.CameraBounds.Y + l.Desc.CameraBounds.H / 2);
                ImGui.EndPopup();
            }
        }

        ImGuiExt.HeaderWithWidget("Assets##overview", () =>
        {
            TeamScoreboard? ts = l.Desc.TeamScoreboard;
            if (ts is not null && ImGui.Selectable($"{ts.GetType().Name} {GetExtraObjectInfo(ts)}###selectable{ts.GetHashCode()}", selection.Object == ts))
            {
                selection.Object = ts;
            }
            ShowSelectableList(l.Desc.Assets, selection, val => l.Desc.Assets = val, cmd, movable: true);
            if (l.Desc.LevelAnims.Length > 0) ImGui.Separator();
            ShowSelectableList(l.Desc.LevelAnims, selection, val => l.Desc.LevelAnims = val, cmd, movable: true);
            if (l.Desc.AnimatedBackgrounds.Length > 0) ImGui.Separator();
            ShowSelectableList(l.Desc.AnimatedBackgrounds, selection, val => l.Desc.AnimatedBackgrounds = val, cmd, movable: true);
            if (l.Desc.LevelAnimations.Length > 0) ImGui.Separator();
            ShowSelectableList(l.Desc.LevelAnimations, selection, val => l.Desc.LevelAnimations = val, cmd, movable: true);
        },
        () => addButton("asset", (x, y) => AddObjectPopup.AddMovingPlatformMenuHistory(x, y, l, selection, cmd)));

        ImGuiExt.HeaderWithWidget("Collisions##overview", () =>
        {
            ShowSelectableList(l.Desc.Collisions, selection, val => l.Desc.Collisions = val, cmd);
            ShowSelectableList(l.Desc.DynamicCollisions, selection, val => l.Desc.DynamicCollisions = val, cmd);
        },
        () => addButton("collision", (x, y) => AddObjectPopup.AddDynamicCollisionMenuHistory(x, y, l, selection, cmd)));

        ImGuiExt.HeaderWithWidget("Respawns##overview", () =>
        {
            ShowSelectableList(l.Desc.Respawns, selection, val => l.Desc.Respawns = val, cmd);
            ShowSelectableList(l.Desc.DynamicRespawns, selection, val => l.Desc.DynamicRespawns = val, cmd);
        },
        () => addButton("respawn", (x, y) => AddObjectPopup.AddDynamicRespawnMenuHistory(x, y, l, selection, cmd)));

        ImGuiExt.HeaderWithWidget("Item Spawns##overview", () =>
        {
            ShowSelectableList(l.Desc.ItemSpawns, selection, val => l.Desc.ItemSpawns = val, cmd);
            ShowSelectableList(l.Desc.DynamicItemSpawns, selection, val => l.Desc.DynamicItemSpawns = val, cmd);
        },
        () => addButton("itemspawn", (x, y) => AddObjectPopup.AddDynamicItemSpawnMenuHistory(x, y, l, selection, cmd)));

        ImGuiExt.HeaderWithWidget("NavNodes##overview", () =>
        {
            ShowSelectableList(l.Desc.NavNodes, selection, val => l.Desc.NavNodes = val, cmd);
            ShowSelectableList(l.Desc.DynamicNavNodes, selection, val => l.Desc.DynamicNavNodes = val, cmd);
        },
        () => addButton("navnode", (x, y) => AddObjectPopup.AddDynamicNavNodeMenuHistory(x, y, l, selection, cmd)));

        ImGuiExt.HeaderWithWidget("Volumes##overview", () =>
        {
            ShowSelectableList(l.Desc.Volumes, selection, val => l.Desc.Volumes = val, cmd);
        },
        () => addButton("volume", (x, y) => AddObjectPopup.AddVolumeMenuHistory(x, y, l, selection, cmd)));

        if (ImGui.CollapsingHeader("Sounds##overview"))
        {
            ShowSelectableList(l.Desc.LevelSounds, selection, val => l.Desc.LevelSounds = val, cmd);
        }

        ImGuiExt.HeaderWithWidget("Horde##overview", () =>
        {
            ShowSelectableList(l.Desc.WaveDatas, selection, val => l.Desc.WaveDatas = val, cmd);
        },
        () =>
        {
            if (ImGui.Button("+##wave"))
            {
                AddObjectPopup.AddWaveDataMenuHistory(l, selection, cmd);
            }
        });

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
                    T[] result = WmeUtils.RemoveAt(values, i);
                    cmd.Add(new ArrayRemoveCommand<T>(changeCommand, values, result, values[i]));
                    cmd.SetAllowMerge(false);
                    _propChanged |= true;
                }
                ImGui.SameLine();
            }

            if (movable)
            {
                // couldn't get unicode char to work
                if (ImGuiExt.ButtonDisabledIf(i == 0, $"^##{o.GetHashCode()}"))
                {
                    T[] result = WmeUtils.MoveUp(values, i);
                    cmd.Add(new PropChangeCommand<T[]>(changeCommand, values, result));
                    cmd.SetAllowMerge(false);
                    _propChanged |= true;
                }
                ImGui.SameLine();
                if (ImGuiExt.ButtonDisabledIf(i == values.Length - 1, $"v##{o.GetHashCode()}"))
                {
                    T[] result = WmeUtils.MoveDown(values, i);
                    cmd.Add(new PropChangeCommand<T[]>(changeCommand, values, result));
                    cmd.SetAllowMerge(false);
                    _propChanged |= true;
                }
                ImGui.SameLine();
            }

            if (ImGui.Selectable($"{o.GetType().Name} {GetExtraObjectInfo(o)}###selectable{o.GetHashCode()}", selection.Object == o))
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
        AbstractVolume v => $"(team {v.Team}, {v.X:0.###}, {v.Y:0.###}, {v.W:0.###}, {v.H:0.###})",
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

    private static bool TeamColorOrder(TeamColorEnum[] order, Action<TeamColorEnum[]> setOrder, CommandHistory cmd)
    {
        bool _propChanged = false;
        for (int i = 0; i < order.Length; i++)
        {
            TeamColorEnum c = order[i];

            if (ImGuiExt.ButtonDisabledIf(i == 0, $"^##{c}"))
            {
                TeamColorEnum[] result = WmeUtils.MoveUp(order, i);
                cmd.Add(new PropChangeCommand<TeamColorEnum[]>(setOrder, order, result));
                cmd.SetAllowMerge(false);
                _propChanged |= true;
            }
            ImGui.SameLine();
            if (ImGuiExt.ButtonDisabledIf(i == order.Length - 1, $"v##{c}"))
            {
                TeamColorEnum[] result = WmeUtils.MoveDown(order, i);
                cmd.Add(new PropChangeCommand<TeamColorEnum[]>(setOrder, order, result));
                cmd.SetAllowMerge(false);
                _propChanged |= true;
            }
            ImGui.SameLine();
            ImGui.Text(c.ToString());
        }
        return _propChanged;
    }
}