using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using AbcDisassembler;
using AbcDisassembler.Instructions;
using AbcDisassembler.Multinames;
using AbcDisassembler.Swf.Tags;
using AbcDisassembler.Swf;

namespace WallyMapEditor;

public static partial class WmeUtils
{
    private static List<int> FindGetlexPositions(CPoolInfo cpool, string lexName, List<Instruction> code)
    {
        List<int> result = [];
        for (int i = 0; i < code.Count; ++i)
        {
            Instruction instruction = code[i];
            if (instruction.Name == "getlex" &&
                instruction.Args[0].Value is INamedMultiname named &&
                cpool.Strings[(int)named.Name] == lexName)
            {
                result.Add(i);
            }
        }
        return result;
    }

    private static int FindCallpropvoidPos(CPoolInfo cpool, string methodName, ReadOnlySpan<Instruction> code)
    {
        for (int i = 0; i < code.Length; ++i)
        {
            Instruction instruction = code[i];
            if (instruction.Name == "callpropvoid" &&
                instruction.Args[0].Value is INamedMultiname named &&
                cpool.Strings[(int)named.Name] == methodName)
            {
                return i;
            }
        }
        return -1;
    }

    private static uint? FindLastPushuintArg(ReadOnlySpan<Instruction> code)
    {
        for (int i = code.Length - 1; i >= 0; ++i)
        {
            Instruction instruction = code[i];
            if (instruction.Name == "pushuint")
                return (uint)instruction.Args[0].Value;
        }
        return null;
    }

    public static uint? FindDecryptionKey(AbcFile abc)
    {
        foreach (MethodBodyInfo mb in abc.MethodBodies)
        {
            ReadOnlySpan<Instruction> instructions = CollectionsMarshal.AsSpan(mb.Code);

            List<int> getlexPos = FindGetlexPositions(abc.ConstantPool, "ANE_RawData", mb.Code);
            for (int i = 0; i < getlexPos.Count; i++)
            {
                ReadOnlySpan<Instruction> relevantCode = getlexPos[i] == getlexPos[^1]
                    ? instructions[getlexPos[i]..]
                    : instructions[getlexPos[i]..getlexPos[i + 1]];

                int callpropvoidPos = FindCallpropvoidPos(abc.ConstantPool, "Init", relevantCode);

                if (callpropvoidPos != -1)
                    return FindLastPushuintArg(instructions[0..callpropvoidPos]);
            }
        }

        return null;
    }

    public static DoAbcTag? GetDoAbcTag(string swfPath)
    {
        using FileStream file = new(swfPath, FileMode.Open, FileAccess.Read);
        foreach (ITag t in SwfFile.ReadTags(file))
        {
            if (t is DoAbcTag abcTag)
                return abcTag;
        }

        return null;
    }

    public static uint? FindDecryptionKeyFromPath(string bhairPath)
    {
        DoAbcTag? tag = GetDoAbcTag(bhairPath);
        if (tag is null) return null;
        return FindDecryptionKey(tag.AbcFile);
    }
}