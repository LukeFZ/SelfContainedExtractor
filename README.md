# SelfContainedExtractor
*Utility to extract assemblies (and other resources) from self-contained .NET 5+ executables*

## Requirements

 .NET 7.x [x64](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-7.0.0-rc.2-windows-x64-installer)/[x86](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-7.0.0-rc.2-windows-x86-installer) installed

## Usage

1. Download the latest release from the [Releases](https://github.com/LukeFZ/SelfContainedExtractor/releases) tab.
2. Extract the downloaded archive.
3. Launch the executable from the command-line.  
`Usage: DotNetSelfContainedExtractor.exe <self-contained app> <output directory path> [bundle file offset (0x for hex)]`  
**Note: The bundle file offset is optional, if omitted the program will attempt to automatically detect the required offset.**

## Explanation

The code for this extractor has been derived from the [.NET corehost source](https://github.com/dotnet/runtime/tree/0e6fa6287022ef1d660c377daf3ca27a9694eea5/src/native/corehost/bundle) and ported to C#.  
It first reads the header of the contained bundle to get the amount of contained files, then reads an entry for each file detailing offset, size and name of the contained file.  
After all files have been read (and if needed, zlib-decompressed) it then dumps every file to the specified output folder.
