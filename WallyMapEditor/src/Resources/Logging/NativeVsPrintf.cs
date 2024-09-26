using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WallyMapEditor;

internal partial class NativeVsPrintf {

    private const string MSVCRT = "msvcrt";

    [LibraryImport(MSVCRT, EntryPoint = "vsprintf")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial int NativeVsPrintf_Windows(nint buffer, nint format, nint args);

    [LibraryImport(MSVCRT, EntryPoint = "vsnprintf")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial int NativeVsnPrintf_Windows(nint buffer, nuint size, nint format, nint args);

    public static string GetString(nint format, nint args)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return VsPrintf_Windows(format, args);
        
        throw new PlatformNotSupportedException("Logging with va_list not supported on this platform");
    }

    private static string VsPrintf_Windows(nint format, nint args)
    {
        nint buffer = nint.Zero;

        try
        {
            int byteSize = NativeVsnPrintf_Windows(nint.Zero, nuint.Zero, format, args) + 1;

            if (byteSize <= 1)
                return string.Empty;

            buffer = Marshal.AllocHGlobal(byteSize);
            _ = NativeVsPrintf_Windows(buffer, format, args);

            return Marshal.PtrToStringUTF8(buffer)!;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

}