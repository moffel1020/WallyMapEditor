using System.Xml.Linq;

namespace WallyMapSpinzor2.Raylib;

public class OverlayConfig : ISerializable, IDeserializable
{
    public required float RadiusCollisionPoint { get; set; }
    public required float RadiusCollisionAnchor { get; set; }
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

    public required double SizeOffsetRespawnBox { get; set; }
    public required double SizeOffsetNavNodeBox { get; set; }

    public required int FontSizeKeyFrameNum { get; set; }

    public required RlColor ColorCollisionPoint { get; set; }
    public required RlColor UsingColorCollisionPoint { get; set; }
    public required RlColor ColorCollisionAnchor { get; set; }
    public required RlColor UsingColorCollisionAnchor { get; set; }

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
    public required RlColor TextColorKeyFrameNum { get; set; }

    public void Deserialize(XElement e)
    {
        OverlayConfig @default = Default;
        RadiusCollisionPoint = Utils.ParseFloatOrNull(e.GetElementValue(nameof(RadiusCollisionPoint))) ?? @default.RadiusCollisionPoint;
        RadiusCollisionAnchor = Utils.ParseFloatOrNull(e.GetElementValue(nameof(RadiusCollisionAnchor))) ?? @default.RadiusCollisionAnchor;
        RadiusItemSpawnCorner = Utils.ParseFloatOrNull(e.GetElementValue(nameof(RadiusItemSpawnCorner))) ?? @default.RadiusItemSpawnCorner;
        RadiusAssetCorner = Utils.ParseFloatOrNull(e.GetElementValue(nameof(RadiusAssetCorner))) ?? @default.RadiusAssetCorner;
        RadiusParentAssetPosition = Utils.ParseFloatOrNull(e.GetElementValue(nameof(RadiusParentAssetPosition))) ?? @default.RadiusParentAssetPosition;
        RadiusVolumeCorner = Utils.ParseFloatOrNull(e.GetElementValue(nameof(RadiusVolumeCorner))) ?? @default.RadiusVolumeCorner;
        RadiusNavNodePosition = Utils.ParseFloatOrNull(e.GetElementValue(nameof(RadiusNavNodePosition))) ?? @default.RadiusNavNodePosition;
        RadiusCameraBoundsCorner = Utils.ParseFloatOrNull(e.GetElementValue(nameof(RadiusCameraBoundsCorner))) ?? @default.RadiusCameraBoundsCorner;
        RadiusSpawnBotBoundsCorner = Utils.ParseFloatOrNull(e.GetElementValue(nameof(RadiusSpawnBotBoundsCorner))) ?? @default.RadiusSpawnBotBoundsCorner;
        RadiusDynamicPosition = Utils.ParseFloatOrNull(e.GetElementValue(nameof(RadiusDynamicPosition))) ?? @default.RadiusDynamicPosition;
        RadiusMovingPlatformPosition = Utils.ParseFloatOrNull(e.GetElementValue(nameof(RadiusMovingPlatformPosition))) ?? @default.RadiusMovingPlatformPosition;
        RadiusKeyFramePosition = Utils.ParseFloatOrNull(e.GetElementValue(nameof(RadiusKeyFramePosition))) ?? @default.RadiusKeyFramePosition;
        SizeOffsetRespawnBox = Utils.ParseDoubleOrNull(e.GetElementValue(nameof(SizeOffsetRespawnBox))) ?? @default.SizeOffsetRespawnBox;
        SizeOffsetNavNodeBox = Utils.ParseDoubleOrNull(e.GetElementValue(nameof(SizeOffsetNavNodeBox))) ?? @default.SizeOffsetNavNodeBox;
        FontSizeKeyFrameNum = Utils.ParseIntOrNull(e.GetElementValue(nameof(FontSizeKeyFrameNum))) ?? @default.FontSizeKeyFrameNum;
        ColorCollisionPoint = Wms2RlUtils.ParseRlColorOrNull(e.GetElementValue(nameof(ColorCollisionPoint))) ?? @default.ColorCollisionPoint;
        UsingColorCollisionPoint = Wms2RlUtils.ParseRlColorOrNull(e.GetElementValue(nameof(UsingColorCollisionPoint))) ?? @default.UsingColorCollisionPoint;
        ColorCollisionAnchor = Wms2RlUtils.ParseRlColorOrNull(e.GetElementValue(nameof(ColorCollisionAnchor))) ?? @default.ColorCollisionAnchor;
        UsingColorCollisionAnchor = Wms2RlUtils.ParseRlColorOrNull(e.GetElementValue(nameof(UsingColorCollisionAnchor))) ?? @default.UsingColorCollisionAnchor;
        ColorItemSpawnBox = Wms2RlUtils.ParseRlColorOrNull(e.GetElementValue(nameof(ColorItemSpawnBox))) ?? @default.ColorItemSpawnBox;
        UsingColorItemSpawnBox = Wms2RlUtils.ParseRlColorOrNull(e.GetElementValue(nameof(UsingColorItemSpawnBox))) ?? @default.UsingColorItemSpawnBox;
        ColorRespawnBox = Wms2RlUtils.ParseRlColorOrNull(e.GetElementValue(nameof(ColorRespawnBox))) ?? @default.ColorRespawnBox;
        UsingColorRespawnBox = Wms2RlUtils.ParseRlColorOrNull(e.GetElementValue(nameof(UsingColorRespawnBox))) ?? @default.UsingColorRespawnBox;
        ColorNavNodeBox = Wms2RlUtils.ParseRlColorOrNull(e.GetElementValue(nameof(ColorNavNodeBox))) ?? @default.ColorNavNodeBox;
        UsingColorNavNodeBox = Wms2RlUtils.ParseRlColorOrNull(e.GetElementValue(nameof(UsingColorNavNodeBox))) ?? @default.UsingColorNavNodeBox;
        ColorAssetBox = Wms2RlUtils.ParseRlColorOrNull(e.GetElementValue(nameof(ColorAssetBox))) ?? @default.ColorAssetBox;
        UsingColorAssetBox = Wms2RlUtils.ParseRlColorOrNull(e.GetElementValue(nameof(UsingColorAssetBox))) ?? @default.UsingColorAssetBox;
        ColorAssetRotationLine = Wms2RlUtils.ParseRlColorOrNull(e.GetElementValue(nameof(ColorAssetRotationLine))) ?? @default.ColorAssetRotationLine;
        ColorParentAssetPosition = Wms2RlUtils.ParseRlColorOrNull(e.GetElementValue(nameof(ColorParentAssetPosition))) ?? @default.ColorParentAssetPosition;
        UsingColorParentAssetPosition = Wms2RlUtils.ParseRlColorOrNull(e.GetElementValue(nameof(UsingColorParentAssetPosition))) ?? @default.UsingColorParentAssetPosition;
        ColorParentAssetRotationLine = Wms2RlUtils.ParseRlColorOrNull(e.GetElementValue(nameof(ColorParentAssetRotationLine))) ?? @default.ColorParentAssetRotationLine;
        ColorVolumeBox = Wms2RlUtils.ParseRlColorOrNull(e.GetElementValue(nameof(ColorVolumeBox))) ?? @default.ColorVolumeBox;
        UsingColorVolumeBox = Wms2RlUtils.ParseRlColorOrNull(e.GetElementValue(nameof(UsingColorVolumeBox))) ?? @default.UsingColorVolumeBox;
        ColorCameraBoundsBox = Wms2RlUtils.ParseRlColorOrNull(e.GetElementValue(nameof(ColorCameraBoundsBox))) ?? @default.ColorCameraBoundsBox;
        UsingColorCameraBoundsBox = Wms2RlUtils.ParseRlColorOrNull(e.GetElementValue(nameof(UsingColorCameraBoundsBox))) ?? @default.UsingColorCameraBoundsBox;
        ColorSpawnBotBoundsBox = Wms2RlUtils.ParseRlColorOrNull(e.GetElementValue(nameof(ColorSpawnBotBoundsBox))) ?? @default.ColorSpawnBotBoundsBox;
        UsingColorSpawnBotBoundsBox = Wms2RlUtils.ParseRlColorOrNull(e.GetElementValue(nameof(UsingColorSpawnBotBoundsBox))) ?? @default.UsingColorSpawnBotBoundsBox;
        ColorDynamicPosition = Wms2RlUtils.ParseRlColorOrNull(e.GetElementValue(nameof(ColorDynamicPosition))) ?? @default.ColorDynamicPosition;
        UsingColorDynamicPosition = Wms2RlUtils.ParseRlColorOrNull(e.GetElementValue(nameof(UsingColorDynamicPosition))) ?? @default.UsingColorDynamicPosition;
        ColorMovingPlatformPosition = Wms2RlUtils.ParseRlColorOrNull(e.GetElementValue(nameof(ColorMovingPlatformPosition))) ?? @default.ColorMovingPlatformPosition;
        UsingColorMovingPlatformPosition = Wms2RlUtils.ParseRlColorOrNull(e.GetElementValue(nameof(UsingColorMovingPlatformPosition))) ?? @default.UsingColorMovingPlatformPosition;
        ColorKeyFramePosition = Wms2RlUtils.ParseRlColorOrNull(e.GetElementValue(nameof(ColorKeyFramePosition))) ?? @default.ColorKeyFramePosition;
        UsingColorKeyFramePosition = Wms2RlUtils.ParseRlColorOrNull(e.GetElementValue(nameof(UsingColorKeyFramePosition))) ?? @default.UsingColorKeyFramePosition;
        TextColorKeyFrameNum = Wms2RlUtils.ParseRlColorOrNull(e.GetElementValue(nameof(TextColorKeyFrameNum))) ?? @default.TextColorKeyFrameNum;
    }

    public void Serialize(XElement e)
    {
        e.AddChild(nameof(RadiusCollisionPoint), RadiusCollisionPoint);
        e.AddChild(nameof(RadiusCollisionAnchor), RadiusCollisionAnchor);
        e.AddChild(nameof(RadiusItemSpawnCorner), RadiusItemSpawnCorner);
        e.AddChild(nameof(RadiusAssetCorner), RadiusAssetCorner);
        e.AddChild(nameof(RadiusParentAssetPosition), RadiusParentAssetPosition);
        e.AddChild(nameof(RadiusVolumeCorner), RadiusVolumeCorner);
        e.AddChild(nameof(RadiusNavNodePosition), RadiusNavNodePosition);
        e.AddChild(nameof(RadiusCameraBoundsCorner), RadiusCameraBoundsCorner);
        e.AddChild(nameof(RadiusSpawnBotBoundsCorner), RadiusSpawnBotBoundsCorner);
        e.AddChild(nameof(RadiusDynamicPosition), RadiusDynamicPosition);
        e.AddChild(nameof(RadiusMovingPlatformPosition), RadiusMovingPlatformPosition);
        e.AddChild(nameof(RadiusKeyFramePosition), RadiusKeyFramePosition);
        e.AddChild(nameof(SizeOffsetRespawnBox), SizeOffsetRespawnBox);
        e.AddChild(nameof(SizeOffsetNavNodeBox), SizeOffsetNavNodeBox);
        e.AddChild(nameof(FontSizeKeyFrameNum), FontSizeKeyFrameNum);
        e.AddChild(nameof(ColorCollisionPoint), "0x" + Wms2RlUtils.RlColorToHex(ColorCollisionPoint));
        e.AddChild(nameof(UsingColorCollisionPoint), "0x" + Wms2RlUtils.RlColorToHex(UsingColorCollisionPoint));
        e.AddChild(nameof(ColorCollisionAnchor), "0x" + Wms2RlUtils.RlColorToHex(ColorCollisionAnchor));
        e.AddChild(nameof(UsingColorCollisionAnchor), "0x" + Wms2RlUtils.RlColorToHex(UsingColorCollisionAnchor));
        e.AddChild(nameof(ColorItemSpawnBox), "0x" + Wms2RlUtils.RlColorToHex(ColorItemSpawnBox));
        e.AddChild(nameof(UsingColorItemSpawnBox), "0x" + Wms2RlUtils.RlColorToHex(UsingColorItemSpawnBox));
        e.AddChild(nameof(ColorRespawnBox), "0x" + Wms2RlUtils.RlColorToHex(ColorRespawnBox));
        e.AddChild(nameof(UsingColorRespawnBox), "0x" + Wms2RlUtils.RlColorToHex(UsingColorRespawnBox));
        e.AddChild(nameof(ColorNavNodeBox), "0x" + Wms2RlUtils.RlColorToHex(ColorNavNodeBox));
        e.AddChild(nameof(UsingColorNavNodeBox), "0x" + Wms2RlUtils.RlColorToHex(UsingColorNavNodeBox));
        e.AddChild(nameof(ColorAssetBox), "0x" + Wms2RlUtils.RlColorToHex(ColorAssetBox));
        e.AddChild(nameof(UsingColorAssetBox), "0x" + Wms2RlUtils.RlColorToHex(UsingColorAssetBox));
        e.AddChild(nameof(ColorAssetRotationLine), "0x" + Wms2RlUtils.RlColorToHex(ColorAssetRotationLine));
        e.AddChild(nameof(ColorParentAssetPosition), "0x" + Wms2RlUtils.RlColorToHex(ColorParentAssetPosition));
        e.AddChild(nameof(UsingColorParentAssetPosition), "0x" + Wms2RlUtils.RlColorToHex(UsingColorParentAssetPosition));
        e.AddChild(nameof(ColorParentAssetRotationLine), "0x" + Wms2RlUtils.RlColorToHex(ColorParentAssetRotationLine));
        e.AddChild(nameof(ColorVolumeBox), "0x" + Wms2RlUtils.RlColorToHex(ColorVolumeBox));
        e.AddChild(nameof(UsingColorVolumeBox), "0x" + Wms2RlUtils.RlColorToHex(UsingColorVolumeBox));
        e.AddChild(nameof(ColorCameraBoundsBox), "0x" + Wms2RlUtils.RlColorToHex(ColorCameraBoundsBox));
        e.AddChild(nameof(UsingColorCameraBoundsBox), "0x" + Wms2RlUtils.RlColorToHex(UsingColorCameraBoundsBox));
        e.AddChild(nameof(ColorSpawnBotBoundsBox), "0x" + Wms2RlUtils.RlColorToHex(ColorSpawnBotBoundsBox));
        e.AddChild(nameof(UsingColorSpawnBotBoundsBox), "0x" + Wms2RlUtils.RlColorToHex(UsingColorSpawnBotBoundsBox));
        e.AddChild(nameof(ColorDynamicPosition), "0x" + Wms2RlUtils.RlColorToHex(ColorDynamicPosition));
        e.AddChild(nameof(UsingColorDynamicPosition), "0x" + Wms2RlUtils.RlColorToHex(UsingColorDynamicPosition));
        e.AddChild(nameof(ColorMovingPlatformPosition), "0x" + Wms2RlUtils.RlColorToHex(ColorMovingPlatformPosition));
        e.AddChild(nameof(UsingColorMovingPlatformPosition), "0x" + Wms2RlUtils.RlColorToHex(UsingColorMovingPlatformPosition));
        e.AddChild(nameof(ColorKeyFramePosition), "0x" + Wms2RlUtils.RlColorToHex(ColorKeyFramePosition));
        e.AddChild(nameof(UsingColorKeyFramePosition), "0x" + Wms2RlUtils.RlColorToHex(UsingColorKeyFramePosition));
        e.AddChild(nameof(TextColorKeyFrameNum), "0x" + Wms2RlUtils.RlColorToHex(TextColorKeyFrameNum));
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
    private static readonly RlColor OpaqueWhite = RlColor.White;

    public static OverlayConfig Default => new()
    {
        RadiusCollisionPoint = DEFAULT_RADIUS,
        RadiusCollisionAnchor = DEFAULT_RADIUS,
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
        SizeOffsetNavNodeBox = DEFAULT_SIZE_OFFSET,
        SizeOffsetRespawnBox = DEFAULT_SIZE_OFFSET,
        FontSizeKeyFrameNum = DEFAULT_FONT_SIZE,
        ColorCollisionPoint = TransparentGray,
        UsingColorCollisionPoint = TransparentWhite,
        ColorCollisionAnchor = TransparentDarkGreen,
        UsingColorCollisionAnchor = TransparentGreen,
        ColorItemSpawnBox = TransparentGray,
        UsingColorItemSpawnBox = TransparentWhite,
        ColorRespawnBox = TransparentGray,
        UsingColorRespawnBox = TransparentWhite,
        ColorNavNodeBox = TransparentGray,
        UsingColorNavNodeBox = TransparentWhite,
        ColorAssetBox = TransparentGray,
        UsingColorAssetBox = TransparentWhite,
        ColorAssetRotationLine = OpaqueWhite,
        ColorParentAssetPosition = TransparentRed,
        UsingColorParentAssetPosition = TransparentPink,
        ColorParentAssetRotationLine = OpaqueWhite,
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
        TextColorKeyFrameNum = OpaqueWhite,
    };
}