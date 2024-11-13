namespace WallyMapEditor;

public static class Strings
{
    public const string UI_DISPLAY_NAME_TOOLTIP = "The name of the map as shown ingame";
    public const string UI_TEST_LEVEL_TOOLTIP = "If true, the map can only be played while test features are on";
    public const string UI_LEVEL_THUMBNAIL_TOOLTIP = "The map's image in the map selection screen.";
    public const string UI_EXTRA_SF_TOOLTIP = "Amount of frames to offset moving platform movement.";
    public const string UI_KILL_BOUNDS_TOOLTIP = "The blastzones are defined as offsets from the camera bounds.";
    public const string UI_SOFT_TOP_TOOLTIP = "If false, players can die off the top of the map while not stunned. Default is true.";
    public const string UI_HARD_LEFT_TOOLTIP = "If true, players can't die off the left blastzone while not stunned. Used in Showdown.";
    public const string UI_HARD_RIGHT_TOOLTIP = "If true, players can't die off the right blastzone while not stunned. Used in Showdown.";
    public const string UI_CAMERA_BOUNDS_TOOLTIP = "The maximum area of the map that the camera can show.";
    public const string UI_STREAMER_BG_MUSIC_TOOLTIP = "The alternative music track to use when streamer mode is on.";
    public const string UI_SIDEKICK_BOUNDS_TOOLTIP = "Area for sidekicks to fly in.\nSidekicks are called spawn bots internally.";
    public const string UI_IS_CLIMB_MAP_TOOLTIP = "Affects how bots recover to the stage.";
    public const string UI_STRICT_RECOVER_TOOLTIP = "Affects how bots recover to the stage.";
    public const string UI_MIDGROUND_SECTION_TOOLTIP = "This does nothing ingame.";
    public const string UI_AVOID_TEAM_COLOR_TOOLTIP = "Prevent picking this team color.";
    public const string UI_TEAM_COLOR_ORDER_TOOLTIP = "Defines the priority order of team colors.";
    public const string UI_SHADOW_TINT_TOOLTIP = "Defines the color of shadows.";
    public const string UI_ANIMATED_MIDGROUND_TOOLTIP = "If true, the animated background will be on the midground layer.";
    public const string UI_FORCE_DRAW_TOOLTIP = "If true, the animated background will be shown even if animated backgrounds are turned off.";
    public const string UI_EASING_TOOLTIP = "These properties allow smoothing out the movement between keyframes.";
    public const string UI_EASE_IN_TOOLTIP = "If true, movement will start slow and then speed up.";
    public const string UI_EASE_OUT_TOOLTIP = "If true, movement will slow down near the next keyframe.";
    public const string UI_EASE_POWER_TOOLTIP = "Controls how aggressive the speed curve is.";
    public const string UI_CENTER_TOOLTIP = "Defines a position to do an elliptical orbit around. Must have the same X or Y as one of the keyframes or it won't work correctly.";
    public const string UI_RESPAWN_OFF_TOOLTIP = "If true, DynamicRespawns animated by the moving platform won't get used for respawning while interpolating from this keyframe.";
    public const string UI_BG_ANIMATED_ASSET_TOOLTIP = "An alternative background to use when animated backgrounds are enabled";
    public const string UI_COLLISION_FLAG_TOOLTIP = "Affects bouncing sound. Sand is used in Vollybrawl. Might not work.";
    public const string UI_COLLISION_COLOR_FLAG_TOOLTIP = "Unused.";
    public const string UI_COLLISION_TAUNT_EVENT_TOOLTIP = "Used for battlepass missions like taunting on an island.";
    public const string UI_COLLISION_ANCHOR_TOOLTIP = "Turns the collision into a quadratic bezier.";
    public const string UI_COLLISION_NORMAL_TOOLTIP = "Overrides the default collision normal. Useful when moving platforms move through collision.";
    public const string UI_PLATFORM_ASSET_SWAP_TOOLTIP = "Set if the platform always shows up, only shows up when animated backgrounds are off, or only shows up when animated backgrounds are on.";
    public const string UI_HORDE_GROUP_COUNT_TOOLTIP = "The number of demons that spawn.";
    public const string UI_HORDE_GROUP_DELAY_TOOLTIP = "The delay until this group spawns.";
    public const string UI_HORDE_GROUP_STAGGER_TOOLTIP = "The delay between demons spawning from the group.";
    public const string UI_HORDE_GROUP_DIR_TOOLTIP = "Controls where the demons will come from.";
    public const string UI_HORDE_GROUP_PATH_TOOLTIP = "Controls what door the demons will go to.";
    public const string UI_HORDE_GROUP_PATH_NUMERIC_TOOLTIP = "Controls the path the demons will take.";
    public const string UI_HORDE_GROUP_SHARED_TOOLTIP = "If true, Dir will be relative to previous group and not previous demon.";
    public const string UI_HORDE_GROUP_SHARED_PATH_TOOLTIP = "If true, all demons will take the same path, instead of randomizing.";
}