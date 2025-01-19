# WallyMapEditor
<p>
  <a href="https://discord.com/invite/pxpe7FGwfQ">
    <img src="https://img.shields.io/discord/1287853332853035150?logo=discord&logoColor=white&label=Discord&color=7289da" />
 </a>
</p>

A map editor for Brawlhalla. **Currently under development.**

## Download
Download the latest release [here](https://github.com/moffel1020/WallyMapEditor/releases/latest)

If the program doesn't launch on windows, you may need to install the [latest Visual C++ Redistributable](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170)

## Building from source

make sure you have _git_ installed.

download the code:

`git clone --recurse-submodules https://github.com/moffel1020/WallyMapEditor.git`

run the project (inside the WallyMapEditor folder created by git clone):

`dotnet run --project WallyMapEditor`

## Requirements

- .NET 8 SDK
- On Windows: latest Visual C++ Redistributable

The program allows overriding any relevant brawlhalla file with a custom one, but for ease of use, **it is recommended to install Brawlhalla**.

## Submodules

- WallyMapSpinzor2 - C# serialization library for Brawlhalla .xml maps + rendering API (can be implemented in any rendering library - see the old WallyMapSpinzor2.MonoGame repo).

- WallyAnmSpinzor - C# library for parsing Brawlhalla .anm files.

- BrawlhallaSwz - C# library for encrypting and decrypting Brawlhalla .swz files.

- SwiffCheese - C# library for converting flash vector graphics into svg.

- AbcDisassembler - C# library for parsing actionscript bytecode. Used to find the swz decryption key.

## Package Dependencies

This list includes the dependencies from submodules.

- Svg.Skia (2.0.0.4) - Library based on SkiaSharp (2.88.8) for Svg rendering.

- NativeFileDialogSharp (0.5.0) - C# bindings for nativefiledialog, a C library for opening the platform's default file explorer dialog.

- Raylib-cs (6.1.1) - C# bindings for Raylib, a C rendering library.

- ImGui.NET (1.90.9.1) - C# bindings for ImGui, a C++ ui library.

- rlImgui-cs (2.1.0) - C# library for bridging between Raylib-cs and ImGui.NET.

- SwfLib (1.0.5) - C# library for parsing .swf files.

- OneOf (3.0.271) - C# tagged union library.

- Sep (0.5.2) - C# csv parsing library.
