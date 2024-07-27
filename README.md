# WallyMapSpinzor2.Raylib

A map editor for Brawlhalla, using raylib for rendering. **Currently under development.**

## Installation and running from source

make sure you have _git_ installed.

download the code:

`git clone --recurse-submodules https://github.com/moffel1020/WallyMapSpinzor2.Raylib.git`

run the project (inside the WallyMapSpinzor2.Raylib folder created by git clone):

`dotnet run --project WallyMapSpinzor2.Raylib`

_an exe will be provided in the future_

## Requirements

- .NET 8 SDK

The program allows overriding any relevant brawlhalla file with a custom one, but for ease of use, **it is recommended to install Brawlhalla**.

## Submodules

- WallyMapSpinzor2 - C# serialization library for Brawlhalla .xml maps + rendering API (can be implemented in any rendering library - see the old WallyMapSpinzor2.MonoGame repo).

- WallyAnmSpinzor - C# library for parsing Brawlhalla .anm files.

- BrawlhallaSwz - C# library for encrypting and decrypting Brawlhalla .swz files.

- SwiffCheese - C# library for exporting flash vector graphics.

- AbcDisassembler - C# library for parsing actionscript bytecode. Used to find the swz decryption key.

## Package Dependencies

This list includes the dependencies from submodules.

- SixLabors.ImageSharp (3.1.5) + SixLabors.ImageSharp.Drawing (2.1.4) - Image manipulation library. Used for flash vector graphics exporting.

- NativeFileDialogSharp (0.5.0) - C# bindings for nativefiledialog, a C library for opening the platform's default file explorer dialog.

- Raylib-cs (6.1.1) - C# bindings for Raylib, a C rendering library.

- ImGui.NET (1.90.1.1) - C# bindings for ImGui, a C++ ui library.

- rlImgui-cs (2.0.3) - C# library for bridging between Raylib-cs and ImGui.NET.

- SwfLib (1.0.5) - C# library for parsing .swf files.

- OneOf (3.0.271) - C# tagged union library.
