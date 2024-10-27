using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace WallyMapEditor.Mod;

internal static class StreamExtensions
{
    public static byte[] GetBytes(this Stream stream)
    {
        uint length = stream.GetU32();
        byte[] bytesBuf = new byte[length];
        stream.ReadExactly(bytesBuf);
        return bytesBuf;
    }
    public static void PutBytes(this Stream stream, byte[] value)
    {
        stream.PutU32((uint)value.Length);
        stream.Write(value);
    }

    public static bool GetBool(this Stream stream) => stream.GetU8() != 0;
    public static void PutBool(this Stream stream, bool value) => stream.PutU8((byte)(value ? 1 : 0));

    public static string GetStr(this Stream stream)
    {
        int length = stream.GetU16();
        Span<byte> buf = length >= 1024 ? new byte[length] : stackalloc byte[length];
        stream.ReadExactly(buf);
        return Encoding.UTF8.GetString(buf);
    }
    public static void PutStr(this Stream stream, string value)
    {
        byte[] buf = Encoding.UTF8.GetBytes(value);
        if (buf.Length > ushort.MaxValue)
            throw new ArgumentException("String is too long");
        stream.PutU16((ushort)buf.Length);
        stream.Write(buf);
    }

    public static string GetLongStr(this Stream stream)
    {
        uint length = stream.GetU32();
        Span<byte> buf = length >= 1024 ? new byte[length] : stackalloc byte[(int)length];
        stream.ReadExactly(buf);
        return Encoding.UTF8.GetString(buf);
    }
    public static void PutLongStr(this Stream stream, string value)
    {
        byte[] buf = Encoding.UTF8.GetBytes(value);
        stream.PutU32((uint)buf.Length);
        stream.Write(buf);
    }

    public static byte GetU8(this Stream stream)
    {
        int @byte = stream.ReadByte();
        if (@byte == -1) throw new EndOfStreamException();
        return (byte)@byte;
    }
    public static void PutU8(this Stream stream, byte value) => stream.WriteByte(value);

    public static sbyte GetI8(this Stream stream) => (sbyte)stream.GetU8();
    public static void PutI8(this Stream stream, sbyte value) => stream.PutU8((byte)value);

    public static ushort GetU16(this Stream stream)
    {
        Span<byte> buf = stackalloc byte[2];
        stream.ReadExactly(buf);
        return BinaryPrimitives.ReadUInt16LittleEndian(buf);
    }
    public static void PutU16(this Stream stream, ushort value)
    {
        Span<byte> buf = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16LittleEndian(buf, value);
        stream.Write(buf);
    }

    public static short GetI16(this Stream stream)
    {
        Span<byte> buf = stackalloc byte[2];
        stream.ReadExactly(buf);
        return BinaryPrimitives.ReadInt16LittleEndian(buf);
    }
    public static void PutI16(this Stream stream, short value)
    {
        Span<byte> buf = stackalloc byte[2];
        BinaryPrimitives.WriteInt16LittleEndian(buf, value);
        stream.Write(buf);
    }

    public static uint GetU32(this Stream stream)
    {
        Span<byte> buf = stackalloc byte[4];
        stream.ReadExactly(buf);
        return BinaryPrimitives.ReadUInt32LittleEndian(buf);
    }
    public static void PutU32(this Stream stream, uint value)
    {
        Span<byte> buf = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(buf, value);
        stream.Write(buf);
    }

    public static int GetI32(this Stream stream)
    {
        Span<byte> buf = stackalloc byte[4];
        stream.ReadExactly(buf);
        return BinaryPrimitives.ReadInt32LittleEndian(buf);
    }
    public static void PutI32(this Stream stream, int value)
    {
        Span<byte> buf = stackalloc byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(buf, value);
        stream.Write(buf);
    }

    public static ulong GetU64(this Stream stream)
    {
        Span<byte> buf = stackalloc byte[8];
        stream.ReadExactly(buf);
        return BinaryPrimitives.ReadUInt64LittleEndian(buf);
    }
    public static void PutU64(this Stream stream, ulong value)
    {
        Span<byte> buf = stackalloc byte[8];
        BinaryPrimitives.WriteUInt64LittleEndian(buf, value);
        stream.Write(buf);
    }

    public static long GetI64(this Stream stream)
    {
        Span<byte> buf = stackalloc byte[8];
        stream.ReadExactly(buf);
        return BinaryPrimitives.ReadInt64LittleEndian(buf);
    }
    public static void PutI64(this Stream stream, long value)
    {
        Span<byte> buf = stackalloc byte[8];
        BinaryPrimitives.WriteInt64LittleEndian(buf, value);
        stream.Write(buf);
    }

    public static UInt128 GetU128(this Stream stream)
    {
        Span<byte> buf = stackalloc byte[16];
        stream.ReadExactly(buf);
        return BinaryPrimitives.ReadUInt128LittleEndian(buf);
    }
    public static void PutU128(this Stream stream, UInt128 value)
    {
        Span<byte> buf = stackalloc byte[16];
        BinaryPrimitives.WriteUInt128LittleEndian(buf, value);
        stream.Write(buf);
    }

    public static Int128 GetI128(this Stream stream)
    {
        Span<byte> buf = stackalloc byte[16];
        stream.ReadExactly(buf);
        return BinaryPrimitives.ReadInt128LittleEndian(buf);
    }
    public static void PutI128(this Stream stream, Int128 value)
    {
        Span<byte> buf = stackalloc byte[16];
        BinaryPrimitives.WriteInt128LittleEndian(buf, value);
        stream.Write(buf);
    }

    public static Half GetF16(this Stream stream)
    {
        Span<byte> buf = stackalloc byte[2];
        stream.ReadExactly(buf);
        return BinaryPrimitives.ReadHalfLittleEndian(buf);
    }
    public static void PutF16(this Stream stream, Half value)
    {
        Span<byte> buf = stackalloc byte[2];
        BinaryPrimitives.WriteHalfLittleEndian(buf, value);
        stream.Write(buf);
    }

    public static float GetF32(this Stream stream)
    {
        Span<byte> buf = stackalloc byte[4];
        stream.ReadExactly(buf);
        return BinaryPrimitives.ReadSingleLittleEndian(buf);
    }
    public static void PutF32(this Stream stream, float value)
    {
        Span<byte> buf = stackalloc byte[4];
        BinaryPrimitives.WriteSingleLittleEndian(buf, value);
        stream.Write(buf);
    }

    public static double GetF64(this Stream stream)
    {
        Span<byte> buf = stackalloc byte[8];
        stream.ReadExactly(buf);
        return BinaryPrimitives.ReadDoubleLittleEndian(buf);
    }
    public static void PutF64(this Stream stream, double value)
    {
        Span<byte> buf = stackalloc byte[8];
        BinaryPrimitives.WriteDoubleLittleEndian(buf, value);
        stream.Write(buf);
    }
}