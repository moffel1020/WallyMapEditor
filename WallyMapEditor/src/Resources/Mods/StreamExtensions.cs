using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace WallyMapEditor.Mod;

internal static class StreamExtensions
{
    public static byte[] GetBytes(this Stream stream, Span<byte> buf)
    {
        uint length = stream.GetU32(buf);
        byte[] bytesBuf = new byte[length];
        stream.ReadExactly(bytesBuf);
        return bytesBuf;
    }
    public static void PutBytes(this Stream stream, Span<byte> buf, byte[] value)
    {
        stream.PutU32(buf, (uint)value.Length);
        stream.Write(value);
    }

    public static bool GetB(this Stream stream) => stream.GetU8() != 0;
    public static void PutB(this Stream stream, bool value) => stream.PutU8((byte)(value ? 1 : 0));

    public static string GetStr(this Stream stream, Span<byte> buf)
    {
        int length = stream.GetU16(buf);
        Span<byte> strBuf = length >= 1024 ? new byte[length] : stackalloc byte[length];
        stream.ReadExactly(strBuf);
        return Encoding.UTF8.GetString(strBuf);
    }
    public static void PutStr(this Stream stream, Span<byte> buf, string value)
    {
        byte[] strBuf = Encoding.UTF8.GetBytes(value);
        if (strBuf.Length > ushort.MaxValue)
            throw new ArgumentException("String is too long");
        stream.PutU16(buf, (ushort)strBuf.Length);
        stream.Write(buf);
    }

    public static string GetLongStr(this Stream stream, Span<byte> buf)
    {
        uint length = stream.GetU32(buf);
        Span<byte> strBuf = length >= 1024 ? new byte[length] : stackalloc byte[(int)length];
        stream.ReadExactly(strBuf);
        return Encoding.UTF8.GetString(strBuf);
    }
    public static void PutLongStr(this Stream stream, Span<byte> buf, string value)
    {
        byte[] strBuf = Encoding.UTF8.GetBytes(value);
        stream.PutU32(buf, (uint)strBuf.Length);
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

    public static ushort GetU16(this Stream stream, Span<byte> buf)
    {
        stream.ReadExactly(buf[..2]);
        return BinaryPrimitives.ReadUInt16LittleEndian(buf);
    }
    public static void PutU16(this Stream stream, Span<byte> buf, ushort value)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(buf[..2], value);
        stream.Write(buf[..2]);
    }

    public static short GetI16(this Stream stream, Span<byte> buf)
    {
        stream.ReadExactly(buf[..2]);
        return BinaryPrimitives.ReadInt16LittleEndian(buf);
    }
    public static void PutI16(this Stream stream, Span<byte> buf, short value)
    {
        BinaryPrimitives.WriteInt16LittleEndian(buf[..2], value);
        stream.Write(buf[..2]);
    }

    public static uint GetU32(this Stream stream, Span<byte> buf)
    {
        stream.ReadExactly(buf[..4]);
        return BinaryPrimitives.ReadUInt32LittleEndian(buf);
    }
    public static void PutU32(this Stream stream, Span<byte> buf, uint value)
    {
        BinaryPrimitives.WriteUInt32LittleEndian(buf[..4], value);
        stream.Write(buf[..4]);
    }

    public static int GetI32(this Stream stream, Span<byte> buf)
    {
        stream.ReadExactly(buf[..4]);
        return BinaryPrimitives.ReadInt32LittleEndian(buf);
    }
    public static void PutI32(this Stream stream, Span<byte> buf, int value)
    {
        BinaryPrimitives.WriteInt32LittleEndian(buf[..4], value);
        stream.Write(buf[..4]);
    }

    public static ulong GetU64(this Stream stream, Span<byte> buf)
    {
        stream.ReadExactly(buf[..8]);
        return BinaryPrimitives.ReadUInt64LittleEndian(buf);
    }
    public static void PutU64(this Stream stream, Span<byte> buf, ulong value)
    {
        BinaryPrimitives.WriteUInt64LittleEndian(buf[..8], value);
        stream.Write(buf[..8]);
    }

    public static long GetI64(this Stream stream, Span<byte> buf)
    {
        stream.ReadExactly(buf[..8]);
        return BinaryPrimitives.ReadInt64LittleEndian(buf);
    }
    public static void PutI64(this Stream stream, Span<byte> buf, long value)
    {
        BinaryPrimitives.WriteInt64LittleEndian(buf[..8], value);
        stream.Write(buf[..8]);
    }

    public static UInt128 GetU128(this Stream stream, Span<byte> buf)
    {
        stream.ReadExactly(buf[..16]);
        return BinaryPrimitives.ReadUInt128LittleEndian(buf);
    }
    public static void PutU128(this Stream stream, Span<byte> buf, UInt128 value)
    {
        BinaryPrimitives.WriteUInt128LittleEndian(buf[..16], value);
        stream.Write(buf[..16]);
    }

    public static Int128 GetI128(this Stream stream, Span<byte> buf)
    {
        stream.ReadExactly(buf[..16]);
        return BinaryPrimitives.ReadInt128LittleEndian(buf);
    }
    public static void PutI128(this Stream stream, Span<byte> buf, Int128 value)
    {
        BinaryPrimitives.WriteInt128LittleEndian(buf[..16], value);
        stream.Write(buf[..16]);
    }

    public static Half GetF16(this Stream stream, Span<byte> buf)
    {
        stream.ReadExactly(buf[..2]);
        return BinaryPrimitives.ReadHalfLittleEndian(buf);
    }
    public static void PutF16(this Stream stream, Span<byte> buf, Half value)
    {
        BinaryPrimitives.WriteHalfLittleEndian(buf[..2], value);
        stream.Write(buf[..2]);
    }

    public static float GetF32(this Stream stream, Span<byte> buf)
    {
        stream.ReadExactly(buf[..4]);
        return BinaryPrimitives.ReadSingleLittleEndian(buf);
    }
    public static void PutF32(this Stream stream, Span<byte> buf, float value)
    {
        BinaryPrimitives.WriteSingleLittleEndian(buf[..4], value);
        stream.Write(buf[..4]);
    }

    public static double GetF64(this Stream stream, Span<byte> buf)
    {
        stream.ReadExactly(buf[..8]);
        return BinaryPrimitives.ReadDoubleLittleEndian(buf);
    }
    public static void PutF64(this Stream stream, Span<byte> buf, double value)
    {
        BinaryPrimitives.WriteDoubleLittleEndian(buf[..8], value);
        stream.Write(buf[..8]);
    }
}