using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public sealed class OverlayConfig : ISerializable, IDeserializable<OverlayConfig>
{
    public required float RadiusCollisionPoint { get; set; }
    public required float RadiusCollisionAnchor { get; set; }
    public required float RadiusCollisionSnapPoint { get; set; }
    public required float RadiusItemSpawnCorner { get; set; }
    public required float RadiusNavNodePosition { get; set; }
    public required float RadiusAssetCorner { get; set; }
    public required float RadiusParentAssetPosition { get; set; }
    public required float RadiusVolumeCorner { get; set; }
    public required float RadiusCameraBoundsCorner { get; set; }
    public required float RadiusSpawnBotBoundsCorner { get; set; }
    public required float RadiusDynamicPosition { get; set; }
    public required float RadiusMovingPlatformPosition { get; set; }
    public required float RadiusKeyFramePosition { get; set; }
    public required float RadiusAnmCenter { get; set; }
    public required float RadiusKeyFrameCenter { get; set; }
    public required float RadiusFireOffset { get; set; }
    public required float RadiusPathPoint { get; set; }

    public required double SizeOffsetRespawnBox { get; set; }
    public required double SizeOffsetNavNodeBox { get; set; }

    public required double LineWidthParentAssetScale { get; set; }
    public required double LengthParentAssetScale { get; set; }
    public required double SensitivityParentAssetScale { get; set; }

    public required double LengthFireDirectionArrow { get; set; }
    public required double OffsetFireDirectionArrowBack { get; set; }
    public required double OffsetFireDirectionArrowSide { get; set; }

    public required int FontSizeKeyFrameNum { get; set; }
    public required int FontSizeAnmCenter { get; set; }
    public required int FontSizeKeyFrameCenter { get; set; }
    public required int FontSizePathPointNum { get; set; }

    public required RlColor ColorCollisionPoint { get; set; }
    public required RlColor UsingColorCollisionPoint { get; set; }
    public required RlColor ColorCollisionAnchor { get; set; }
    public required RlColor UsingColorCollisionAnchor { get; set; }
    public required RlColor ColorCollisionSnapPoint { get; set; }

    public required RlColor ColorItemSpawnBox { get; set; }
    public required RlColor UsingColorItemSpawnBox { get; set; }

    public required RlColor ColorRespawnBox { get; set; }
    public required RlColor UsingColorRespawnBox { get; set; }

    public required RlColor ColorNavNodeBox { get; set; }
    public required RlColor UsingColorNavNodeBox { get; set; }

    public required RlColor ColorAssetBox { get; set; }
    public required RlColor UsingColorAssetBox { get; set; }
    public required RlColor ColorAssetRotationLine { get; set; }

    public required RlColor ColorParentAssetPosition { get; set; }
    public required RlColor UsingColorParentAssetPosition { get; set; }
    public required RlColor ColorParentAssetRotationLine { get; set; }
    public required RlColor ColorParentAssetScale { get; set; }
    public required RlColor UsingColorParentAssetScale { get; set; }

    public required RlColor ColorVolumeBox { get; set; }
    public required RlColor UsingColorVolumeBox { get; set; }

    public required RlColor ColorCameraBoundsBox { get; set; }
    public required RlColor UsingColorCameraBoundsBox { get; set; }

    public required RlColor ColorSpawnBotBoundsBox { get; set; }
    public required RlColor UsingColorSpawnBotBoundsBox { get; set; }

    public required RlColor ColorDynamicPosition { get; set; }
    public required RlColor UsingColorDynamicPosition { get; set; }

    public required RlColor ColorMovingPlatformPosition { get; set; }
    public required RlColor UsingColorMovingPlatformPosition { get; set; }

    public required RlColor ColorKeyFramePosition { get; set; }
    public required RlColor UsingColorKeyFramePosition { get; set; }
    public required RlColor ColorAnmCenter { get; set; }
    public required RlColor UsingColorAnmCenter { get; set; }
    public required RlColor ColorKeyFrameCenter { get; set; }
    public required RlColor UsingColorKeyFrameCenter { get; set; }
    public required RlColor TextColorKeyFrameNum { get; set; }
    public required RlColor TextColorAnmCenter { get; set; }
    public required RlColor TextColorKeyFrameCenter { get; set; }

    public required RlColor ColorFireOffset { get; set; }
    public required RlColor UsingColorFireOffset { get; set; }
    public required RlColor ColorFireOffsetLine { get; set; }
    public required RlColor ColorFireDirectionArrow { get; set; }

    public required RlColor ColorPathPoint { get; set; }
    public required RlColor UsingColorPathPoint { get; set; }
    public required RlColor TextColorPathPointNum { get; set; }

    public OverlayConfig() { }
    [SetsRequiredMembers]
    private OverlayConfig(XElement e)
    {
        OverlayConfig @default = Default;

        int getInt(string name, int @default) => e.GetIntElement(name, @default);
        uint getUInt(string name, uint @default) => e.GetUIntElement(name, @default);
        double getDouble(string name, double @default) => e.GetDoubleElement(name, @default);
        float getFloat(string name, float @default) => Utils.ParseFloatOrNull(e.GetElementOrNull(name)) ?? @default;
        RlColor getColor(string name, RlColor @default) => WmeUtils.HexToRlColor(getUInt(name, WmeUtils.RlColorToHex(@default)));

        RadiusCollisionPoint = getFloat(nameof(RadiusCollisionPoint), @default.RadiusCollisionPoint);
        RadiusCollisionAnchor = getFloat(nameof(RadiusCollisionAnchor), @default.RadiusCollisionAnchor);
        RadiusCollisionSnapPoint = getFloat(nameof(RadiusCollisionSnapPoint), @default.RadiusCollisionSnapPoint);
        RadiusItemSpawnCorner = getFloat(nameof(RadiusItemSpawnCorner), @default.RadiusItemSpawnCorner);
        RadiusAssetCorner = getFloat(nameof(RadiusAssetCorner), @default.RadiusAssetCorner);
        RadiusParentAssetPosition = getFloat(nameof(RadiusParentAssetPosition), @default.RadiusParentAssetPosition);
        RadiusVolumeCorner = getFloat(nameof(RadiusVolumeCorner), @default.RadiusVolumeCorner);
        RadiusNavNodePosition = getFloat(nameof(RadiusNavNodePosition), @default.RadiusNavNodePosition);
        RadiusCameraBoundsCorner = getFloat(nameof(RadiusCameraBoundsCorner), @default.RadiusCameraBoundsCorner);
        RadiusSpawnBotBoundsCorner = getFloat(nameof(RadiusSpawnBotBoundsCorner), @default.RadiusSpawnBotBoundsCorner);
        RadiusDynamicPosition = getFloat(nameof(RadiusDynamicPosition), @default.RadiusDynamicPosition);
        RadiusMovingPlatformPosition = getFloat(nameof(RadiusMovingPlatformPosition), @default.RadiusMovingPlatformPosition);
        RadiusKeyFramePosition = getFloat(nameof(RadiusKeyFramePosition), @default.RadiusKeyFramePosition);
        RadiusAnmCenter = getFloat(nameof(RadiusAnmCenter), @default.RadiusAnmCenter);
        RadiusKeyFrameCenter = getFloat(nameof(RadiusKeyFrameCenter), @default.RadiusKeyFrameCenter);
        RadiusFireOffset = getFloat(nameof(RadiusFireOffset), @default.RadiusFireOffset);
        RadiusPathPoint = getFloat(nameof(RadiusPathPoint), @default.RadiusPathPoint);

        SizeOffsetRespawnBox = getDouble(nameof(SizeOffsetRespawnBox), @default.SizeOffsetRespawnBox);
        SizeOffsetNavNodeBox = getDouble(nameof(SizeOffsetNavNodeBox), @default.SizeOffsetNavNodeBox);
        LineWidthParentAssetScale = getDouble(nameof(LineWidthParentAssetScale), @default.LineWidthParentAssetScale);
        LengthParentAssetScale = getDouble(nameof(LengthParentAssetScale), @default.LengthParentAssetScale);
        SensitivityParentAssetScale = getDouble(nameof(SensitivityParentAssetScale), @default.SensitivityParentAssetScale);
        LengthFireDirectionArrow = getDouble(nameof(LengthFireDirectionArrow), @default.LengthFireDirectionArrow);
        OffsetFireDirectionArrowBack = getDouble(nameof(OffsetFireDirectionArrowBack), @default.OffsetFireDirectionArrowBack);
        OffsetFireDirectionArrowSide = getDouble(nameof(OffsetFireDirectionArrowSide), @default.OffsetFireDirectionArrowSide);
        FontSizeKeyFrameNum = getInt(nameof(FontSizeKeyFrameNum), @default.FontSizeKeyFrameNum);
        FontSizeAnmCenter = getInt(nameof(FontSizeAnmCenter), @default.FontSizeAnmCenter);
        FontSizeKeyFrameCenter = getInt(nameof(FontSizeKeyFrameCenter), @default.FontSizeKeyFrameCenter);
        FontSizePathPointNum = getInt(nameof(FontSizePathPointNum), @default.FontSizePathPointNum);

        ColorCollisionPoint = getColor(nameof(ColorCollisionPoint), @default.ColorCollisionPoint);
        UsingColorCollisionPoint = getColor(nameof(UsingColorCollisionPoint), @default.UsingColorCollisionPoint);
        ColorCollisionAnchor = getColor(nameof(ColorCollisionAnchor), @default.ColorCollisionAnchor);
        UsingColorCollisionAnchor = getColor(nameof(UsingColorCollisionAnchor), @default.UsingColorCollisionAnchor);
        ColorCollisionSnapPoint = getColor(nameof(ColorCollisionSnapPoint), @default.ColorCollisionSnapPoint);
        ColorItemSpawnBox = getColor(nameof(ColorItemSpawnBox), @default.ColorItemSpawnBox);
        UsingColorItemSpawnBox = getColor(nameof(UsingColorItemSpawnBox), @default.UsingColorItemSpawnBox);
        ColorRespawnBox = getColor(nameof(ColorRespawnBox), @default.ColorRespawnBox);
        UsingColorRespawnBox = getColor(nameof(UsingColorRespawnBox), @default.UsingColorRespawnBox);
        ColorNavNodeBox = getColor(nameof(ColorNavNodeBox), @default.ColorNavNodeBox);
        UsingColorNavNodeBox = getColor(nameof(UsingColorNavNodeBox), @default.UsingColorNavNodeBox);
        ColorAssetBox = getColor(nameof(ColorAssetBox), @default.ColorAssetBox);
        UsingColorAssetBox = getColor(nameof(UsingColorAssetBox), @default.UsingColorAssetBox);
        ColorAssetRotationLine = getColor(nameof(ColorAssetRotationLine), @default.ColorAssetRotationLine);
        ColorParentAssetPosition = getColor(nameof(ColorParentAssetPosition), @default.ColorParentAssetPosition);
        UsingColorParentAssetPosition = getColor(nameof(UsingColorParentAssetPosition), @default.UsingColorParentAssetPosition);
        ColorParentAssetRotationLine = getColor(nameof(ColorParentAssetRotationLine), @default.ColorParentAssetRotationLine);
        ColorParentAssetScale = getColor(nameof(ColorParentAssetScale), @default.ColorParentAssetScale);
        UsingColorParentAssetScale = getColor(nameof(UsingColorParentAssetScale), @default.UsingColorParentAssetScale);
        ColorVolumeBox = getColor(nameof(ColorVolumeBox), @default.ColorVolumeBox);
        UsingColorVolumeBox = getColor(nameof(UsingColorVolumeBox), @default.UsingColorVolumeBox);
        ColorCameraBoundsBox = getColor(nameof(ColorCameraBoundsBox), @default.ColorCameraBoundsBox);
        UsingColorCameraBoundsBox = getColor(nameof(UsingColorCameraBoundsBox), @default.UsingColorCameraBoundsBox);
        ColorSpawnBotBoundsBox = getColor(nameof(ColorSpawnBotBoundsBox), @default.ColorSpawnBotBoundsBox);
        UsingColorSpawnBotBoundsBox = getColor(nameof(UsingColorSpawnBotBoundsBox), @default.UsingColorSpawnBotBoundsBox);
        ColorDynamicPosition = getColor(nameof(ColorDynamicPosition), @default.ColorDynamicPosition);
        UsingColorDynamicPosition = getColor(nameof(UsingColorDynamicPosition), @default.UsingColorDynamicPosition);
        ColorMovingPlatformPosition = getColor(nameof(ColorMovingPlatformPosition), @default.ColorMovingPlatformPosition);
        UsingColorMovingPlatformPosition = getColor(nameof(UsingColorMovingPlatformPosition), @default.UsingColorMovingPlatformPosition);
        ColorKeyFramePosition = getColor(nameof(ColorKeyFramePosition), @default.ColorKeyFramePosition);
        UsingColorKeyFramePosition = getColor(nameof(UsingColorKeyFramePosition), @default.UsingColorKeyFramePosition);
        ColorAnmCenter = getColor(nameof(ColorAnmCenter), @default.ColorAnmCenter);
        UsingColorAnmCenter = getColor(nameof(UsingColorAnmCenter), @default.UsingColorAnmCenter);
        ColorKeyFrameCenter = getColor(nameof(ColorKeyFrameCenter), @default.ColorKeyFrameCenter);
        UsingColorKeyFrameCenter = getColor(nameof(UsingColorKeyFrameCenter), @default.UsingColorKeyFrameCenter);
        TextColorKeyFrameNum = getColor(nameof(TextColorKeyFrameNum), @default.TextColorKeyFrameNum);
        TextColorAnmCenter = getColor(nameof(TextColorAnmCenter), @default.TextColorAnmCenter);
        TextColorKeyFrameCenter = getColor(nameof(TextColorKeyFrameCenter), @default.TextColorKeyFrameCenter);
        ColorFireOffset = getColor(nameof(ColorFireOffset), @default.ColorFireOffset);
        UsingColorFireOffset = getColor(nameof(UsingColorFireOffset), @default.UsingColorFireOffset);
        ColorFireOffsetLine = getColor(nameof(ColorFireOffsetLine), @default.ColorFireOffsetLine);
        ColorFireDirectionArrow = getColor(nameof(ColorFireDirectionArrow), @default.ColorFireDirectionArrow);
        ColorPathPoint = getColor(nameof(ColorPathPoint), @default.ColorPathPoint);
        UsingColorPathPoint = getColor(nameof(UsingColorPathPoint), @default.UsingColorPathPoint);
        TextColorPathPointNum = getColor(nameof(TextColorPathPointNum), @default.TextColorPathPointNum);
    }
    public static OverlayConfig Deserialize(XElement e) => new(e);

    public void Serialize(XElement e)
    {
        void addValue(string name, object? value) => e.AddChild(name, value);
        void addColor(string name, RlColor value) => addValue(name, "0x" + WmeUtils.RlColorToHex(value));

        addValue(nameof(RadiusCollisionPoint), RadiusCollisionPoint);
        addValue(nameof(RadiusCollisionAnchor), RadiusCollisionAnchor);
        addValue(nameof(RadiusCollisionSnapPoint), RadiusCollisionSnapPoint);
        addValue(nameof(RadiusItemSpawnCorner), RadiusItemSpawnCorner);
        addValue(nameof(RadiusAssetCorner), RadiusAssetCorner);
        addValue(nameof(RadiusParentAssetPosition), RadiusParentAssetPosition);
        addValue(nameof(RadiusVolumeCorner), RadiusVolumeCorner);
        addValue(nameof(RadiusNavNodePosition), RadiusNavNodePosition);
        addValue(nameof(RadiusCameraBoundsCorner), RadiusCameraBoundsCorner);
        addValue(nameof(RadiusSpawnBotBoundsCorner), RadiusSpawnBotBoundsCorner);
        addValue(nameof(RadiusDynamicPosition), RadiusDynamicPosition);
        addValue(nameof(RadiusMovingPlatformPosition), RadiusMovingPlatformPosition);
        addValue(nameof(RadiusKeyFramePosition), RadiusKeyFramePosition);
        addValue(nameof(RadiusAnmCenter), RadiusAnmCenter);
        addValue(nameof(RadiusKeyFrameCenter), RadiusKeyFrameCenter);
        addValue(nameof(RadiusFireOffset), RadiusFireOffset);
        addValue(nameof(RadiusPathPoint), RadiusPathPoint);
        addValue(nameof(SizeOffsetRespawnBox), SizeOffsetRespawnBox);
        addValue(nameof(SizeOffsetNavNodeBox), SizeOffsetNavNodeBox);
        addValue(nameof(LineWidthParentAssetScale), LineWidthParentAssetScale);
        addValue(nameof(LengthParentAssetScale), LengthParentAssetScale);
        addValue(nameof(SensitivityParentAssetScale), SensitivityParentAssetScale);
        addValue(nameof(LengthFireDirectionArrow), LengthFireDirectionArrow);
        addValue(nameof(OffsetFireDirectionArrowBack), OffsetFireDirectionArrowBack);
        addValue(nameof(OffsetFireDirectionArrowSide), OffsetFireDirectionArrowSide);
        addValue(nameof(FontSizeKeyFrameNum), FontSizeKeyFrameNum);
        addValue(nameof(FontSizeAnmCenter), FontSizeAnmCenter);
        addValue(nameof(FontSizeKeyFrameCenter), FontSizeKeyFrameCenter);
        addValue(nameof(FontSizePathPointNum), FontSizePathPointNum);
        addColor(nameof(ColorCollisionPoint), ColorCollisionPoint);
        addColor(nameof(UsingColorCollisionPoint), UsingColorCollisionPoint);
        addColor(nameof(ColorCollisionAnchor), ColorCollisionAnchor);
        addColor(nameof(UsingColorCollisionAnchor), UsingColorCollisionAnchor);
        addColor(nameof(ColorCollisionSnapPoint), ColorCollisionSnapPoint);
        addColor(nameof(ColorItemSpawnBox), ColorItemSpawnBox);
        addColor(nameof(UsingColorItemSpawnBox), UsingColorItemSpawnBox);
        addColor(nameof(ColorRespawnBox), ColorRespawnBox);
        addColor(nameof(UsingColorRespawnBox), UsingColorRespawnBox);
        addColor(nameof(ColorNavNodeBox), ColorNavNodeBox);
        addColor(nameof(UsingColorNavNodeBox), UsingColorNavNodeBox);
        addColor(nameof(ColorAssetBox), ColorAssetBox);
        addColor(nameof(UsingColorAssetBox), UsingColorAssetBox);
        addColor(nameof(ColorAssetRotationLine), ColorAssetRotationLine);
        addColor(nameof(ColorParentAssetPosition), ColorParentAssetPosition);
        addColor(nameof(UsingColorParentAssetPosition), UsingColorParentAssetPosition);
        addColor(nameof(ColorParentAssetScale), ColorParentAssetScale);
        addColor(nameof(UsingColorParentAssetScale), UsingColorParentAssetScale);
        addColor(nameof(ColorParentAssetRotationLine), ColorParentAssetRotationLine);
        addColor(nameof(ColorVolumeBox), ColorVolumeBox);
        addColor(nameof(UsingColorVolumeBox), UsingColorVolumeBox);
        addColor(nameof(ColorCameraBoundsBox), ColorCameraBoundsBox);
        addColor(nameof(UsingColorCameraBoundsBox), UsingColorCameraBoundsBox);
        addColor(nameof(ColorSpawnBotBoundsBox), ColorSpawnBotBoundsBox);
        addColor(nameof(UsingColorSpawnBotBoundsBox), UsingColorSpawnBotBoundsBox);
        addColor(nameof(ColorDynamicPosition), ColorDynamicPosition);
        addColor(nameof(UsingColorDynamicPosition), UsingColorDynamicPosition);
        addColor(nameof(ColorMovingPlatformPosition), ColorMovingPlatformPosition);
        addColor(nameof(UsingColorMovingPlatformPosition), UsingColorMovingPlatformPosition);
        addColor(nameof(ColorKeyFramePosition), ColorKeyFramePosition);
        addColor(nameof(UsingColorKeyFramePosition), UsingColorKeyFramePosition);
        addColor(nameof(ColorAnmCenter), ColorAnmCenter);
        addColor(nameof(UsingColorAnmCenter), UsingColorAnmCenter);
        addColor(nameof(ColorKeyFrameCenter), ColorKeyFrameCenter);
        addColor(nameof(UsingColorKeyFrameCenter), UsingColorKeyFrameCenter);
        addColor(nameof(TextColorKeyFrameNum), TextColorKeyFrameNum);
        addColor(nameof(TextColorAnmCenter), TextColorAnmCenter);
        addColor(nameof(TextColorKeyFrameCenter), TextColorKeyFrameCenter);
        addColor(nameof(ColorFireOffset), ColorFireOffset);
        addColor(nameof(UsingColorFireOffset), UsingColorFireOffset);
        addColor(nameof(ColorFireOffsetLine), ColorFireOffsetLine);
        addColor(nameof(ColorFireDirectionArrow), ColorFireDirectionArrow);
        addColor(nameof(ColorPathPoint), ColorPathPoint);
        addColor(nameof(UsingColorPathPoint), UsingColorPathPoint);
        addColor(nameof(TextColorPathPointNum), TextColorPathPointNum);
    }

    private const float DEFAULT_RADIUS = 30;
    private const float DEFAULT_MID_RADIUS = 70;
    private const float DEFAULT_LARGE_RADIUS = 100;
    private const float DEFAULT_SIZE_OFFSET = 40;
    private const int DEFAULT_FONT_SIZE = 32;
    private const byte OPACITY = 190;
    private static readonly RlColor TransparentGray = RlColor.Gray with { A = OPACITY };
    private static readonly RlColor TransparentWhite = RlColor.White with { A = OPACITY };
    private static readonly RlColor TransparentDarkGreen = RlColor.DarkGreen with { A = OPACITY };
    private static readonly RlColor TransparentGreen = RlColor.Green with { A = OPACITY };
    private static readonly RlColor TransparentRed = RlColor.Red with { A = OPACITY };
    private static readonly RlColor TransparentPink = RlColor.Pink with { A = OPACITY };
    private static readonly RlColor TransparentYellow = RlColor.Yellow with { A = OPACITY };
    private static readonly RlColor TransparentOrange = RlColor.Orange with { A = OPACITY };

    public static OverlayConfig Default => new()
    {
        RadiusCollisionPoint = DEFAULT_RADIUS,
        RadiusCollisionAnchor = DEFAULT_RADIUS,
        RadiusCollisionSnapPoint = DEFAULT_RADIUS,
        RadiusItemSpawnCorner = DEFAULT_RADIUS,
        RadiusAssetCorner = DEFAULT_RADIUS,
        RadiusParentAssetPosition = DEFAULT_MID_RADIUS,
        RadiusVolumeCorner = DEFAULT_RADIUS,
        RadiusNavNodePosition = DEFAULT_RADIUS,
        RadiusCameraBoundsCorner = DEFAULT_LARGE_RADIUS,
        RadiusSpawnBotBoundsCorner = DEFAULT_LARGE_RADIUS,
        RadiusDynamicPosition = DEFAULT_MID_RADIUS,
        RadiusMovingPlatformPosition = DEFAULT_MID_RADIUS,
        RadiusKeyFramePosition = DEFAULT_RADIUS,
        RadiusAnmCenter = DEFAULT_RADIUS,
        RadiusKeyFrameCenter = DEFAULT_RADIUS,
        RadiusFireOffset = DEFAULT_RADIUS,
        RadiusPathPoint = DEFAULT_RADIUS,
        SizeOffsetNavNodeBox = DEFAULT_SIZE_OFFSET,
        SizeOffsetRespawnBox = DEFAULT_SIZE_OFFSET,
        LineWidthParentAssetScale = 20,
        LengthParentAssetScale = 250,
        SensitivityParentAssetScale = 0.1,
        LengthFireDirectionArrow = 70,
        OffsetFireDirectionArrowSide = 9,
        OffsetFireDirectionArrowBack = 14,
        FontSizeKeyFrameNum = DEFAULT_FONT_SIZE,
        FontSizeAnmCenter = DEFAULT_FONT_SIZE,
        FontSizeKeyFrameCenter = DEFAULT_FONT_SIZE,
        FontSizePathPointNum = DEFAULT_FONT_SIZE,
        ColorCollisionPoint = TransparentGray,
        UsingColorCollisionPoint = TransparentWhite,
        ColorCollisionAnchor = TransparentDarkGreen,
        UsingColorCollisionAnchor = TransparentGreen,
        ColorCollisionSnapPoint = TransparentWhite,
        ColorItemSpawnBox = TransparentGray,
        UsingColorItemSpawnBox = TransparentWhite,
        ColorRespawnBox = TransparentGray,
        UsingColorRespawnBox = TransparentWhite,
        ColorNavNodeBox = TransparentGray,
        UsingColorNavNodeBox = TransparentWhite,
        ColorAssetBox = TransparentGray,
        UsingColorAssetBox = TransparentWhite,
        ColorAssetRotationLine = RlColor.White,
        ColorParentAssetPosition = TransparentRed,
        UsingColorParentAssetPosition = TransparentPink,
        ColorParentAssetRotationLine = RlColor.White,
        ColorParentAssetScale = RlColor.Yellow,
        UsingColorParentAssetScale = RlColor.Orange,
        ColorVolumeBox = TransparentGray,
        UsingColorVolumeBox = TransparentWhite,
        ColorCameraBoundsBox = TransparentGray,
        UsingColorCameraBoundsBox = TransparentWhite,
        ColorSpawnBotBoundsBox = TransparentGray,
        UsingColorSpawnBotBoundsBox = TransparentWhite,
        ColorDynamicPosition = TransparentRed,
        UsingColorDynamicPosition = TransparentPink,
        ColorMovingPlatformPosition = TransparentRed,
        UsingColorMovingPlatformPosition = TransparentPink,
        ColorKeyFramePosition = TransparentYellow,
        UsingColorKeyFramePosition = TransparentOrange,
        ColorAnmCenter = TransparentDarkGreen,
        UsingColorAnmCenter = TransparentGreen,
        ColorKeyFrameCenter = TransparentDarkGreen,
        UsingColorKeyFrameCenter = TransparentGreen,
        TextColorKeyFrameNum = RlColor.White,
        TextColorAnmCenter = RlColor.White,
        TextColorKeyFrameCenter = RlColor.White,
        ColorFireOffset = TransparentOrange,
        UsingColorFireOffset = TransparentRed,
        ColorFireOffsetLine = TransparentYellow,
        ColorFireDirectionArrow = TransparentPink,
        ColorPathPoint = TransparentPink,
        UsingColorPathPoint = TransparentRed,
        TextColorPathPointNum = RlColor.White,
    };
}