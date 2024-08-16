using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

using ImGuiNET;
using NativeFileDialogSharp;
using Raylib_cs;

namespace WallyMapSpinzor2.Raylib;

public class RenderConfigWindow
{
    private bool _open = false;
    public bool Open { get => _open; set => _open = value; }

    private static void LoadConfig(RenderConfig config, string path)
    {
        XElement element;
        using (FileStream stream = new(path, FileMode.Open, FileAccess.Read))
            element = XElement.Load(stream);
        config.Deserialize(element);
    }

    private static void SaveConfig(RenderConfig config, string path)
    {
        WmeUtils.SerializeToPath(config, path);
    }

    public void Show(RenderConfig config, RenderConfigDefault configDefault, PathPreferences prefs)
    {
        ImGui.Begin("Render Config", ref _open);

        ImGui.SeparatorText("Loading##config");
        if (ImGui.Button("Load config from file##config"))
        {
            //TODO: give a reasonable default folder (a Config folder inside the appdata?)
            Task.Run(() =>
            {
                DialogResult result = Dialog.FileOpen("xml", prefs.ConfigFolderPath);
                if (result.IsOk)
                {
                    prefs.ConfigFolderPath = Path.GetDirectoryName(result.Path);
                    try
                    {
                        LoadConfig(config, result.Path);
                    }
                    catch (Exception e)
                    {
                        Rl.TraceLog(TraceLogLevel.Error, $"Loading config failed with error: {e.Message}");
                        Rl.TraceLog(TraceLogLevel.Trace, e.StackTrace);
                    }
                }
            });
        }

        if (ImGui.Button("Save config to file##config"))
        {
            //TODO: give a reasonable default folder (a Config folder inside the appdata?)
            Task.Run(() =>
            {
                DialogResult result = Dialog.FileSave("xml", prefs.ConfigFolderPath);
                if (result.IsOk)
                {
                    prefs.ConfigFolderPath = Path.GetDirectoryName(result.Path);
                    try
                    {
                        SaveConfig(config, result.Path);
                    }
                    catch (Exception e)
                    {
                        Rl.TraceLog(TraceLogLevel.Error, $"Saving config failed with error: {e.Message}");
                        Rl.TraceLog(TraceLogLevel.Trace, e.StackTrace);
                    }
                }
            });
        }

        ImGui.Separator();

        if (ImGui.Button("Reset to custom default##config"))
        {
            config.Deserialize(configDefault.SerializeToXElement());
        }

        if (ImGui.Button("Reset to base default##config"))
        {
            config.Deserialize(RenderConfig.Default.SerializeToXElement());
        }

        if (ImGui.Button("Set as custom default##config"))
        {
            configDefault.Deserialize(config.SerializeToXElement());
        }

        ImGui.SeparatorText("General##config");
        config.RenderSpeed = ImGuiExt.DragDouble("Render speed##config", config.RenderSpeed, speed: 0.1f);
        if (ImGui.Button("Reset time##config"))
            config.Time = TimeSpan.FromSeconds(0);

        ImGui.SeparatorText("Bounds##config");
        config.ShowCameraBounds = ImGuiExt.Checkbox("Camera bounds##config", config.ShowCameraBounds);
        config.ShowKillBounds = ImGuiExt.Checkbox("Kill bounds##config", config.ShowKillBounds);
        config.ShowSpawnBotBounds = ImGuiExt.Checkbox("Spawn bot bounds##config", config.ShowSpawnBotBounds);

        ImGui.SeparatorText("Collisions##config");
        config.ShowCollision = ImGuiExt.Checkbox("Collisions##config", config.ShowCollision);
        config.ShowCollisionNormalOverride = ImGuiExt.Checkbox("Normal overrides##config", config.ShowCollisionNormalOverride);
        config.ShowFireOffsetLocation = ImGuiExt.Checkbox("Pressure plate fire offset##config", config.ShowFireOffsetLocation);
        config.ShowFireOffsetLine = ImGuiExt.Checkbox("Pressure plate line to fire offset##config", config.ShowFireOffsetLine);
        config.ShowFireDirection = ImGuiExt.Checkbox("Pressure plate fire direction##config", config.ShowFireDirection);

        ImGui.SeparatorText("Spawns##config");
        config.ShowRespawn = ImGuiExt.Checkbox("Respawns##config", config.ShowRespawn);
        config.ShowItemSpawn = ImGuiExt.Checkbox("Item spawns##config", config.ShowItemSpawn);

        ImGui.SeparatorText("Assets##config");
        config.ShowAssets = ImGuiExt.Checkbox("Assets##config", config.ShowAssets);
        config.ShowBackground = ImGuiExt.Checkbox("Backgrounds##config", config.ShowBackground);
        config.AnimatedBackgrounds = ImGuiExt.Checkbox("Animate backgrounds##config", config.AnimatedBackgrounds);

        ImGui.SeparatorText("NavNodes##config");
        config.ShowNavNode = ImGuiExt.Checkbox("NavNodes##config", config.ShowNavNode);

        ImGui.SeparatorText("Volumes##config");
        config.ShowGoal = ImGuiExt.Checkbox("Goals##config", config.ShowGoal);
        config.ShowNoDodgeZone = ImGuiExt.Checkbox("No dodge zones##config", config.ShowNoDodgeZone);
        config.ShowVolume = ImGuiExt.Checkbox("Plain volumes##config", config.ShowVolume);

        ImGui.SeparatorText("Others##config");
        config.NoSkulls = ImGuiExt.Checkbox("NoSkulls##config", config.NoSkulls);
        config.ScoringType = ImGuiExt.EnumCombo("Scoring Type##config", config.ScoringType);
        config.Theme = ImGuiExt.EnumCombo("Theme##config", config.Theme);
        config.Hotkey = ImGuiExt.EnumCombo("Hotkey##config", config.Hotkey);

        ImGui.Separator();
        if (ImGui.TreeNode("Gamemode Config##config"))
        {
            config.BlueScore = ImGuiExt.SliderInt("Blue team score##config", config.BlueScore, minValue: 0, maxValue: 99);
            config.RedScore = ImGuiExt.SliderInt("Red team score##config", config.RedScore, minValue: 0, maxValue: 99);
            ImGui.Separator();
            config.ShowPickedPlatform = ImGuiExt.Checkbox("Highlight platform king platform##config", config.ShowPickedPlatform);
            ImGuiExt.WithDisabled(!config.ShowPickedPlatform, () =>
            {
                config.PickedPlatform = ImGuiExt.SliderInt("Platform king platform index##config", config.PickedPlatform, minValue: 0, maxValue: 9);
            });
            ImGui.Separator();
            config.ShowZombieSpawns = ImGuiExt.Checkbox("Show zombie spawns##config", config.ShowZombieSpawns);
            ImGui.Separator();
            config.ShowRingRopes = ImGuiExt.Checkbox("Show brawldown ropes##config", config.ShowRingRopes);
            ImGui.Separator();
            if (ImGui.TreeNode("Bombsketball##config"))
            {
                config.ShowBombsketballTargets = ImGuiExt.Checkbox("Show bombsketball targets##config", config.ShowBombsketballTargets);
                config.UseBombsketballDigitSize = ImGuiExt.Checkbox("Bombsketball digit size fix##config", config.UseBombsketballDigitSize);
                ImGui.Separator();
                double[] frames = [7500 / 16.0, 3000 / 10.0, 7500 / 16.0];
                for (int i = 0; i < 3; ++i)
                {
                    config.ShowBombsketballBombTimers[i] = ImGuiExt.Checkbox($"Show timer {i + 1}##config", config.ShowBombsketballBombTimers[i]);
                    config.BombsketballBombTimerFrames[i] = ImGuiExt.DragDouble($"Timer {i + 1} frames##config", config.BombsketballBombTimerFrames[i], minValue: 0, maxValue: frames[i] - 1 / 16.0);
                }
            }
            ImGui.Separator();
            if (ImGui.TreeNode("Horde##config"))
            {
                config.ShowHordeDoors = ImGuiExt.Checkbox("Show horde doors##config", config.ShowHordeDoors);
                ImGuiExt.WithDisabled(!config.ShowHordeDoors, () =>
                {
                    // we're gonna assume for now we won't be dealing with more than 2
                    config.DamageHordeDoors[0] = ImGuiExt.SliderInt("Door 1 damage##config", config.DamageHordeDoors[0], minValue: 0, maxValue: 24);
                    config.DamageHordeDoors[1] = ImGuiExt.SliderInt("Door 2 damage##config", config.DamageHordeDoors[1], minValue: 0, maxValue: 24);
                });
                config.HordePathType = ImGuiExt.EnumCombo("Horde path type##config", config.HordePathType);
                ImGuiExt.WithDisabled(config.HordePathType == RenderConfig.PathConfigEnum.NONE, () =>
                {
                    config.HordePathIndex = ImGuiExt.SliderInt("Horde path index##config", config.HordePathIndex, 0, 19);
                    ImGuiExt.WithDisabled(config.HordePathType != RenderConfig.PathConfigEnum.CUSTOM, () =>
                    {
                        config.HordeWave = ImGuiExt.SliderInt("Horde wave##config", config.HordeWave, 0, 99);
                    });
                    ImGuiExt.WithDisabled(config.HordePathType == RenderConfig.PathConfigEnum.CUSTOM, () =>
                    {
                        config.HordeRandomSeed = (uint)ImGuiExt.DragInt("Horde random seed##config", (int)config.HordeRandomSeed, minValue: 0);
                    });
                });

                ImGui.TreePop();
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Size##config"))
        {
            config.RadiusRespawn = ImGuiExt.DragDouble("Respawn radius##config", config.RadiusRespawn, minValue: 0);
            config.RadiusZombieSpawn = ImGuiExt.DragDouble("Zombie spawn radius##config", config.RadiusZombieSpawn, minValue: 0);
            config.RadiusNavNode = ImGuiExt.DragDouble("NavNode radius##config", config.RadiusNavNode, minValue: 0);
            config.RadiusHordePathPoint = ImGuiExt.DragDouble("Horde path point radius##config", config.RadiusHordePathPoint, minValue: 0);
            config.RadiusFireOffsetLocation = ImGuiExt.DragDouble("Fire offset location radius##config", config.RadiusFireOffsetLocation, minValue: 0);
            config.LengthCollisionNormal = ImGuiExt.DragDouble("Collision normal length##config", config.LengthCollisionNormal, minValue: 0);
            config.LengthFireDirectionArrow = ImGuiExt.DragDouble("Fire direction arrow length##config", config.LengthFireDirectionArrow, minValue: 0);
            config.OffsetNavLineArrowSide = ImGuiExt.DragDouble("Offset navnode arrow side##config", config.OffsetNavLineArrowSide, minValue: 0);
            config.OffsetNavLineArrowBack = ImGuiExt.DragDouble("Offset navnode arrow back##config", config.OffsetNavLineArrowBack);
            config.OffsetHordePathArrowSide = ImGuiExt.DragDouble("Offset horde path arrow side##config", config.OffsetHordePathArrowSide, minValue: 0);
            config.OffsetHordePathArrowBack = ImGuiExt.DragDouble("Offset horde path arrow back##config", config.OffsetHordePathArrowBack);
            config.OffsetFireDirectionArrowSide = ImGuiExt.DragDouble("Offset fire direction arrow side##config", config.OffsetFireDirectionArrowSide, minValue: 0);
            config.OffsetFireDirectionArrowBack = ImGuiExt.DragDouble("Offset fire direction arrow back##config", config.OffsetFireDirectionArrowBack);
            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Colors##Colors"))
        {
            if (ImGui.TreeNode("Bounds##configColors"))
            {
                config.ColorCameraBounds = ImGuiExt.ColorPicker4("Camera bounds##configColors", config.ColorCameraBounds);
                config.ColorKillBounds = ImGuiExt.ColorPicker4("Kill bounds##configColors", config.ColorKillBounds);
                config.ColorSpawnBotBounds = ImGuiExt.ColorPicker4("Spawn bot bounds##configColors", config.ColorSpawnBotBounds);
                ImGui.TreePop();
            }
            if (ImGui.TreeNode("Collisions##configColors"))
            {
                config.ColorHardCollision = ImGuiExt.ColorPicker4("Hard##configColors", config.ColorHardCollision);
                config.ColorSoftCollision = ImGuiExt.ColorPicker4("Soft##configColors", config.ColorSoftCollision);
                config.ColorGameModeHardCollision = ImGuiExt.ColorPicker4("Gamemode hard##configColors", config.ColorGameModeHardCollision);
                config.ColorBouncyHardCollision = ImGuiExt.ColorPicker4("Bouncy hard##configColors", config.ColorBouncyHardCollision);
                config.ColorBouncySoftCollision = ImGuiExt.ColorPicker4("Bouncy soft##configColors", config.ColorBouncySoftCollision);
                config.ColorBouncyNoSlideCollision = ImGuiExt.ColorPicker4("No slide##configColors", config.ColorNoSlideCollision);
                config.ColorTriggerCollision = ImGuiExt.ColorPicker4("Trigger##configColors", config.ColorTriggerCollision);
                config.ColorStickyCollision = ImGuiExt.ColorPicker4("Sticky##configColors", config.ColorStickyCollision);
                config.ColorItemIgnoreCollision = ImGuiExt.ColorPicker4("Item ignore##configColors", config.ColorItemIgnoreCollision);
                config.ColorPressurePlateCollision = ImGuiExt.ColorPicker4("Pressure plate##configColors", config.ColorPressurePlateCollision);
                config.ColorSoftPressurePlateCollision = ImGuiExt.ColorPicker4("Soft pressure plate##configColors", config.ColorSoftPressurePlateCollision);
                config.ColorLavaCollision = ImGuiExt.ColorPicker4("Lava##configColors", config.ColorLavaCollision);
                ImGui.Spacing();
                config.ColorCollisionNormal = ImGuiExt.ColorPicker4("Collision normal##configColors", config.ColorCollisionNormal);
                config.ColorFireOffset = ImGuiExt.ColorPicker4("Pressure plate fire offset##configColors", config.ColorFireOffset);
                config.ColorFireOffsetLine = ImGuiExt.ColorPicker4("Pressure plate line to fire location##configColors", config.ColorFireOffsetLine);
                config.ColorFireDirection = ImGuiExt.ColorPicker4("Pressure plate fire direction arrow##configColors", config.ColorFireDirection);
                ImGui.TreePop();
            }
            if (ImGui.TreeNode("Respawns##configColors"))
            {
                config.ColorRespawn = ImGuiExt.ColorPicker4("Respawn##configColors", config.ColorRespawn);
                config.ColorInitialRespawn = ImGuiExt.ColorPicker4("Initial respawn##configColors", config.ColorInitialRespawn);
                config.ColorExpandedInitRespawn = ImGuiExt.ColorPicker4("Expanded init respawn##configColors", config.ColorExpandedInitRespawn);
                config.ColorZombieSpawns = ImGuiExt.ColorPicker4("Zombie spawns##configColors", config.ColorZombieSpawns);
                ImGui.TreePop();
            }
            if (ImGui.TreeNode("Item spawns##configColors"))
            {
                config.ColorItemSpawn = ImGuiExt.ColorPicker4("Item spawn##configColors", config.ColorItemSpawn);
                config.ColorItemInitSpawn = ImGuiExt.ColorPicker4("Item init spawn##configColors", config.ColorItemInitSpawn);
                config.ColorItemSet = ImGuiExt.ColorPicker4("Item set##configColors", config.ColorItemSet);
                config.ColorTeamItemInitSpawn = ImGuiExt.ColorPicker4("Team item init spawn##configColors", config.ColorTeamItemInitSpawn);
                ImGui.TreePop();
            }

            if (ImGui.TreeNode("NavNodes##configColors"))
            {
                config.ColorNavNode = ImGuiExt.ColorPicker4("NavNode##configColors", config.ColorNavNode);
                config.ColorNavNodeW = ImGuiExt.ColorPicker4("NavNodeW##configColors", config.ColorNavNodeW);
                config.ColorNavNodeL = ImGuiExt.ColorPicker4("NavNodeL##configColors", config.ColorNavNodeL);
                config.ColorNavNodeA = ImGuiExt.ColorPicker4("NavNodeA##configColors", config.ColorNavNodeA);
                config.ColorNavNodeG = ImGuiExt.ColorPicker4("NavNodeG##configColors", config.ColorNavNodeG);
                config.ColorNavNodeT = ImGuiExt.ColorPicker4("NavNodeT##configColors", config.ColorNavNodeT);
                config.ColorNavNodeS = ImGuiExt.ColorPicker4("NavNodeS##configColors", config.ColorNavNodeS);
                config.ColorNavPath = ImGuiExt.ColorPicker4("NavPath##configColors", config.ColorNavPath);
                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Team##configColors"))
            {
                for (int i = 0; i < config.ColorVolumeTeam.Length; i++)
                {
                    config.ColorVolumeTeam[i] = ImGuiExt.ColorPicker4($"Team {i} volume##configColors", config.ColorVolumeTeam[i]);
                }
                ImGui.Separator();
                for (int i = 0; i < config.ColorCollisionTeam.Length; i++)
                {
                    config.ColorCollisionTeam[i] = ImGuiExt.ColorPicker4($"Team {i + 1} collision##configColors", config.ColorCollisionTeam[i]);
                }
                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Gamemodes##configColors"))
            {
                config.ColorHordePath = ImGuiExt.ColorPicker4("Horde path##configColors", config.ColorHordePath);
                ImGui.TreePop();
            }

            ImGui.TreePop();
        }

        ImGui.End();
    }


}