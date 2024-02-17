using ImGuiNET;

namespace WallyMapSpinzor2.Raylib;

public class RenderConfigWindow
{
    private bool _open = false;
    public bool Open { get => _open; set => _open = value; }

    public void Show(RenderConfig Config)
    {
        ImGui.Begin("Render Config", ref _open);

        ImGui.SeparatorText("Bounds##config");
        Config.ShowCameraBounds = ImGuiExt.Checkbox("Camera bounds##config", Config.ShowCameraBounds);
        Config.ShowKillBounds = ImGuiExt.Checkbox("Kill bounds##config", Config.ShowKillBounds);
        Config.ShowSpawnBotBounds = ImGuiExt.Checkbox("Spawn bot bounds##config", Config.ShowSpawnBotBounds);

        ImGui.SeparatorText("Collisions##config");
        Config.ShowCollision = ImGuiExt.Checkbox("Collisions##config", Config.ShowCollision);
        Config.ShowCollisionNormalOverride = ImGuiExt.Checkbox("Normal overrides##config", Config.ShowCollisionNormalOverride);

        ImGui.SeparatorText("Spawns##config");
        Config.ShowRespawn = ImGuiExt.Checkbox("Respawns##config", Config.ShowRespawn);
        Config.ShowItemSpawn = ImGuiExt.Checkbox("Item spawns##config", Config.ShowItemSpawn);
        Config.ShowZombieSpawns = ImGuiExt.Checkbox("Zombie spawns##config", Config.ShowZombieSpawns);

        ImGui.SeparatorText("Assets##config");
        Config.ShowAssets = ImGuiExt.Checkbox("Assets##config", Config.ShowAssets);
        Config.ShowBackground = ImGuiExt.Checkbox("Backgrounds##config", Config.ShowBackground);
        Config.AnimatedBackgrounds = ImGuiExt.Checkbox("Animate backgrounds##config", Config.AnimatedBackgrounds);

        ImGui.SeparatorText("NavNodes##config");
        Config.ShowNavNode = ImGuiExt.Checkbox("NavNodes##config", Config.ShowNavNode);

        ImGui.SeparatorText("Volumes##config");
        Config.ShowGoal = ImGuiExt.Checkbox("Goals##config", Config.ShowGoal);
        Config.ShowNoDodgeZone = ImGuiExt.Checkbox("No dodge zones##config", Config.ShowNoDodgeZone);
        Config.ShowVolume = ImGuiExt.Checkbox("Plain volumes##config", Config.ShowVolume);

        ImGui.SeparatorText("Others##config");
        Config.NoSkulls = ImGuiExt.Checkbox("NoSkulls##config", Config.NoSkulls);
        Config.ScoringType = ImGuiExt.StringEnumCombo("Scoring Type##config", typeof(ScoringTypeEnum), Config.ScoringType, true);
        Config.Theme = ImGuiExt.StringEnumCombo("Theme##config", typeof(ThemeEnum), Config.Theme, true);
        Config.Hotkey = ImGuiExt.StringEnumCombo("Hotkey##config", typeof(HotkeyEnum), Config.Hotkey, false);

        ImGui.Separator();
        if (ImGui.TreeNode("Gamemode Config##config"))
        {
            Config.ShowPickedPlatform = ImGuiExt.Checkbox("Highlight platform king platform##config", Config.ShowPickedPlatform);
            if (!Config.ShowPickedPlatform) ImGui.BeginDisabled();
            Config.PickedPlatform = ImGuiExt.SliderInt("Platform king platform index##config", Config.PickedPlatform, minValue: 0, maxValue: 9);
            if (!Config.ShowPickedPlatform) ImGui.EndDisabled();
            ImGui.Separator();
            Config.BlueScore = ImGuiExt.SliderInt("Blue team score##config", Config.BlueScore, minValue: 0, maxValue: 99);
            Config.RedScore = ImGuiExt.SliderInt("Red team score##config", Config.RedScore, minValue: 0, maxValue: 99);
            ImGui.Separator();
            Config.ShowRingRopes = ImGuiExt.Checkbox("Show brawldown ropes##config", Config.ShowRingRopes);
            ImGui.Separator();
            Config.ShowHordeDoors = ImGuiExt.Checkbox("Show horde doors##config", Config.ShowHordeDoors);
            if (!Config.ShowHordeDoors) ImGui.BeginDisabled();
            // we're gonna assume for now we won't be dealing with more than 2
            Config.DamageHordeDoors[0] = ImGuiExt.SliderInt("Door 1 damage##config", Config.DamageHordeDoors[0], minValue: 0, maxValue: 24);
            Config.DamageHordeDoors[1] = ImGuiExt.SliderInt("Door 2 damage##config", Config.DamageHordeDoors[1], minValue: 0, maxValue: 24);
            if (!Config.ShowHordeDoors) ImGui.EndDisabled();
            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Size##config"))
        {
            Config.RadiusRespawn = ImGuiExt.DragFloat("Respawn radius##config", Config.RadiusRespawn, minValue: 0);
            Config.RadiusZombieSpawn = ImGuiExt.DragFloat("Zombie spawn radius##config", Config.RadiusZombieSpawn, minValue: 0);
            Config.RadiusNavNode = ImGuiExt.DragFloat("NavNode radius##config", Config.RadiusNavNode, minValue: 0);
            Config.LengthCollisionNormal = ImGuiExt.DragFloat("Collision normal length##config", Config.LengthCollisionNormal, minValue: 0);
            Config.OffsetNavLineArrowSide = ImGuiExt.DragFloat("Offset navnode arrow side##config", Config.OffsetNavLineArrowSide, minValue: 0);
            Config.OffsetNavLineArrowBack = ImGuiExt.DragFloat("Offset navnode arrow back##config", Config.OffsetNavLineArrowBack);
            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Colors##Colors"))
        {
            if (ImGui.TreeNode("Bounds##configColors"))
            {
                Config.ColorCameraBounds = ImGuiExt.ColorPicker4("Camera bounds##configColors", Config.ColorCameraBounds);
                Config.ColorKillBounds = ImGuiExt.ColorPicker4("Kill bounds##configColors", Config.ColorKillBounds);
                Config.ColorSpawnBotBounds = ImGuiExt.ColorPicker4("Spawn bot bounds##configColors", Config.ColorSpawnBotBounds);
                ImGui.TreePop();
            }
            if (ImGui.TreeNode("Collisions##configColors"))
            {
                Config.ColorHardCollision = ImGuiExt.ColorPicker4("Hard##configColors", Config.ColorHardCollision);
                Config.ColorSoftCollision = ImGuiExt.ColorPicker4("Soft##configColors", Config.ColorSoftCollision);
                Config.ColorGameModeHardCollision = ImGuiExt.ColorPicker4("Gamemode hard##configColors", Config.ColorGameModeHardCollision);
                Config.ColorBouncyHardCollision = ImGuiExt.ColorPicker4("Bouncy hard##configColors", Config.ColorBouncyHardCollision);
                Config.ColorBouncySoftCollision = ImGuiExt.ColorPicker4("Bouncy soft##configColors", Config.ColorBouncySoftCollision);
                Config.ColorBouncyNoSlideCollision = ImGuiExt.ColorPicker4("No slide##configColors", Config.ColorNoSlideCollision);
                Config.ColorTriggerCollision = ImGuiExt.ColorPicker4("Rrigger##configColors", Config.ColorTriggerCollision);
                Config.ColorStickyCollision = ImGuiExt.ColorPicker4("Sticky##configColors", Config.ColorStickyCollision);
                Config.ColorItemIgnoreCollision = ImGuiExt.ColorPicker4("Item ignore##configColors", Config.ColorItemIgnoreCollision);
                Config.ColorPressurePlateCollision = ImGuiExt.ColorPicker4("Pressure plate##configColors", Config.ColorPressurePlateCollision);
                Config.ColorSoftPressurePlateCollision = ImGuiExt.ColorPicker4("Soft pressure plate##configColors", Config.ColorSoftPressurePlateCollision);
                Config.ColorCollisionNormal = ImGuiExt.ColorPicker4("Collision normal##configColors", Config.ColorCollisionNormal);
                ImGui.TreePop();
            }
            if (ImGui.TreeNode("Respawns##configColors"))
            {
                Config.ColorRespawn = ImGuiExt.ColorPicker4("Respawn##configColors", Config.ColorRespawn);
                Config.ColorInitialRespawn = ImGuiExt.ColorPicker4("Initial respawn##configColors", Config.ColorInitialRespawn);
                Config.ColorExpandedInitRespawn = ImGuiExt.ColorPicker4("Expanded init respawn##configColors", Config.ColorExpandedInitRespawn);
                Config.ColorZombieSpawns = ImGuiExt.ColorPicker4("Zombie spawns##configColors", Config.ColorZombieSpawns);
                ImGui.TreePop();
            }
            if (ImGui.TreeNode("Item spawns##configColors"))
            {
                Config.ColorItemSpawn = ImGuiExt.ColorPicker4("Item spawn##configColors", Config.ColorItemSpawn);
                Config.ColorItemInitSpawn = ImGuiExt.ColorPicker4("Item init spawn##configColors", Config.ColorItemInitSpawn);
                Config.ColorItemSet = ImGuiExt.ColorPicker4("Item set##configColors", Config.ColorItemSet);
                Config.ColorTeamItemInitSpawn = ImGuiExt.ColorPicker4("Team item init spawn##configColors", Config.ColorTeamItemInitSpawn);
                ImGui.TreePop();
            }

            if (ImGui.TreeNode("NavNodes##configColors"))
            {
                Config.ColorNavNode = ImGuiExt.ColorPicker4("NavNode##configColors", Config.ColorNavNode);
                Config.ColorNavNodeW = ImGuiExt.ColorPicker4("NavNodeW##configColors", Config.ColorNavNodeW);
                Config.ColorNavNodeL = ImGuiExt.ColorPicker4("NavNodeL##configColors", Config.ColorNavNodeL);
                Config.ColorNavNodeA = ImGuiExt.ColorPicker4("NavNodeA##configColors", Config.ColorNavNodeA);
                Config.ColorNavNodeG = ImGuiExt.ColorPicker4("NavNodeG##configColors", Config.ColorNavNodeG);
                Config.ColorNavNodeT = ImGuiExt.ColorPicker4("NavNodeT##configColors", Config.ColorNavNodeT);
                Config.ColorNavNodeS = ImGuiExt.ColorPicker4("NavNodeS##configColors", Config.ColorNavNodeS);
                Config.ColorNavPath = ImGuiExt.ColorPicker4("NavPath##configColors", Config.ColorNavPath);
                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Team##configColors"))
            {
                for (int i = 0; i < Config.ColorVolumeTeam.Length; i++)
                {
                    Config.ColorVolumeTeam[i] = ImGuiExt.ColorPicker4($"Team {i} volume##configColors", Config.ColorVolumeTeam[i]);
                }
                ImGui.Separator();
                for (int i = 0; i < Config.ColorCollisionTeam.Length; i++)
                {
                    Config.ColorCollisionTeam[i] = ImGuiExt.ColorPicker4($"Team {i + 1} collision##configColors", Config.ColorCollisionTeam[i]);
                }
            }
            ImGui.TreePop();
        }

        ImGui.End();
    }
}