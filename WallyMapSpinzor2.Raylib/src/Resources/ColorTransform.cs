using System.Linq;
using System.Text;
using SwfLib.Data;

namespace WallyMapSpinzor2.Raylib;

public class ColorTransform
{
    public short RMult { get; set; } = 256;
    public short GMult { get; set; } = 256;
    public short BMult { get; set; } = 256;
    public short AMult { get; set; } = 256;
    public short RAdd { get; set; } = 0;
    public short GAdd { get; set; } = 0;
    public short BAdd { get; set; } = 0;
    public short AAdd { get; set; } = 0;

    public ColorTransform() { }

    public ColorTransform(ColorTransformRGB transformRGB)
    {
        RMult = transformRGB.HasMultTerms ? transformRGB.RedMultTerm : (short)256;
        RAdd = transformRGB.HasAddTerms ? transformRGB.RedAddTerm : (short)0;
        GMult = transformRGB.HasMultTerms ? transformRGB.GreenMultTerm : (short)256;
        GAdd = transformRGB.HasAddTerms ? transformRGB.GreenAddTerm : (short)0;
        BMult = transformRGB.HasMultTerms ? transformRGB.BlueMultTerm : (short)256;
        BAdd = transformRGB.HasAddTerms ? transformRGB.BlueAddTerm : (short)0;
        AMult = 256;
        AAdd = 0;
    }

    public ColorTransform(ColorTransformRGBA transformRGBA)
    {
        RMult = transformRGBA.HasMultTerms ? transformRGBA.RedMultTerm : (short)256;
        RAdd = transformRGBA.HasAddTerms ? transformRGBA.RedAddTerm : (short)0;
        GMult = transformRGBA.HasMultTerms ? transformRGBA.GreenMultTerm : (short)256;
        GAdd = transformRGBA.HasAddTerms ? transformRGBA.GreenAddTerm : (short)0;
        BMult = transformRGBA.HasMultTerms ? transformRGBA.BlueMultTerm : (short)256;
        BAdd = transformRGBA.HasAddTerms ? transformRGBA.BlueAddTerm : (short)0;
        AMult = transformRGBA.HasMultTerms ? transformRGBA.AlphaMultTerm : (short)256;
        AAdd = transformRGBA.HasAddTerms ? transformRGBA.AlphaAddTerm : (short)0;
    }

    public string GetShaderOperation(string varName)
    {
        return @$"
        {varName}.r *= {RMult / 256.0};
        {varName}.r += {RAdd / 255.0};
        if({varName}.r < 0) {varName}.r = 0;
        if({varName}.r > 1) {varName}.r = 1;

        {varName}.g *= {GMult / 256.0};
        {varName}.g += {GAdd / 255.0};
        if({varName}.g < 0) {varName}.g = 0;
        if({varName}.g > 1) {varName}.g = 1;

        {varName}.b *= {BMult / 256.0};
        {varName}.b += {BAdd / 255.0};
        if({varName}.b < 0) {varName}.b = 0;
        if({varName}.b > 1) {varName}.b = 1;

        {varName}.a *= {AMult / 256.0};
        {varName}.a += {AAdd / 255.0};
        if({varName}.a < 0) {varName}.a = 0;
        if({varName}.a > 1) {varName}.a = 1;
        ";
    }

    public static string CreateShader(ColorTransform[] transforms)
    {
        StringBuilder sb = new(
            @"
            #version 330

            in vec2 fragTexCoord;
            in vec4 fragColor;

            uniform sampler2D texture0;
            uniform vec4 colDiffuse;

            out vec4 finalColor;

            void main()
            {
                vec4 texColor = texture(texture0, fragTexCoord);
            "
        );
        foreach (ColorTransform transform in transforms.Reverse())
        {
            sb.Append(transform.GetShaderOperation("texColor"));
        }
        sb.Append(
            @"
                finalColor = texColor*colDiffuse;
            }"
        );
        return sb.ToString();
    }
}