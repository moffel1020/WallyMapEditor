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
}