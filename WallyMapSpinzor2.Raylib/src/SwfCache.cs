using System;
using System.Collections.Generic;
using System.IO;

using SwfLib;
using SwfLib.Data;
using SwfLib.Tags;
using SwfLib.Tags.ControlTags;
using SwfLib.Tags.ShapeTags;

using SwiffCheese.Wrappers;

namespace WallyMapSpinzor2.Raylib;

public class SwfCache
{
    public SwfFile? Swf { get; set; } = null!;
    public Dictionary<string, ushort> SymbolClass { get; set; } = null!;
    public Dictionary<ushort, DefineSpriteTag> SpriteTags { get; set; } = null!;
    public Dictionary<ushort, DefineShapeXTag> ShapeTags { get; set; } = null!;

    private SwfCache() { }

    public static SwfCache CreateFrom(Stream stream)
    {
        SwfCache swf = new()
        {
            Swf = SwfFile.ReadFrom(stream)
        };

        SymbolClassTag? symbolClass = null;

        //find symbol class
        foreach (SwfTagBase tag in swf.Swf.Tags)
        {
            if (tag is SymbolClassTag symbolClassTag)
            {
                symbolClass = symbolClassTag;
                break;
            }
        }

        if (symbolClass is null)
        {
            throw new Exception("No symbol class in swf");
        }

        swf.SymbolClass = new();
        foreach (SwfSymbolReference reference in symbolClass.References)
        {
            swf.SymbolClass[reference.SymbolName] = reference.SymbolID;
        }

        swf.SpriteTags = new();
        foreach (SwfTagBase tag in swf.Swf.Tags)
        {
            if (tag is DefineSpriteTag st)
            {
                swf.SpriteTags[st.SpriteID] = st;
            }
        }

        //find matching shape tags
        swf.ShapeTags = new();
        foreach (SwfTagBase tag in swf.Swf.Tags)
        {
            //skip DefineShape4 because we don't support it
            if (tag is ShapeBaseTag shape && shape is not DefineShape4Tag)
            {
                swf.ShapeTags[shape.ShapeID] = new DefineShapeXTag(shape);
            }
        }

        return swf;
    }
}