# ATRACTool Reloaded

![Downloads](https://img.shields.io/github/downloads/XyLe-GBP/ATRACTool-Reloaded/total.svg)
[![GitHub (pre-)release](https://img.shields.io/github/release/XyLe-GBP/ATRACTool-Reloaded/all.svg)](https://github.com/XyLe-GBP/ATRACTool-Reloaded/releases)

Utility tool to convert Sony's ATRAC3/ATRAC3plus/ATRAC9(.AT3/.AT9) to any supported format sound.  

Or convert any supported sound to Sony's ATRAC3/ATRAC3plus/ATRAC9 or Walkman format.  

**This application is a GUI application created with the aim of making the command line tools at3tool.exe and at9tool.exe intuitive to operate.**  

**Download:**
[Release build](https://github.com/XyLe-GBP/ATRACTool-Reloaded/releases)

This application has been published and can be run without installing the .NET runtime.

※If for some reason you want to use the portable version, you will need to install the runtime.

version 1.35 or later  
[.NET Desktop Runtime 9.0](https://dotnet.microsoft.com/download/dotnet/9.0)  
version 1.30 or later  
[.NET Desktop Runtime 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)  
version 1.28 or later  
[.NET Desktop Runtime 7.0](https://dotnet.microsoft.com/download/dotnet/7.0)  
version 1.27 or earlier  
[.NET Desktop Runtime 6.0](https://dotnet.microsoft.com/download/dotnet/6.0)  

**The SCEI ATRAC3plus Codec TOOL (at3tool.exe) uses <code>MSVCR80.DLL</code>.**  

If you encounter any errors, please install the following redistributable packages.

 [Microsoft Visual C++ 2005 SP1 Redistributable Package (x86)](http://www.microsoft.com/ja-jp/download/details.aspx?id=5638)

 [Microsoft Visual C++ 2005 SP1 Redistributable Package (x64)](http://www.microsoft.com/ja-jp/download/details.aspx?id=18471)

**The SCEI ATRAC9 Codec TOOL (at9tool.exe) uses <code>MSVCR90.DLL</code>.**  

If you encounter any errors, please install the following redistributable packages.

 [Microsoft Visual C++ 2008 Redistributable Package (x86)](http://www.microsoft.com/ja-jp/download/details.aspx?id=29)

 [Microsoft Visual C++ 2008 Redistributable Package (x64)](http://www.microsoft.com/ja-jp/download/details.aspx?id=15336)

## Details

ATRAC3 and ATRAC3plus are mainly used for PSP and PS3.  
while ATRAC9 is used for PSVita and PS4.  

**Supported file extensions:**
- .m4a (AAC, Apple Lossless)
- .aac
- .aiff
- .alac
- .flac
- .mp3
- .wma
- .opus
- .ogg (Vorbis)
- .wav (PCM)
- .at3 (ATRAC3, ATRAC3+)
- .at9 (ATRAC9)

**For Walkman:**
- .oma (ATRAC3, ATRAC3+, ATRAC Advanced Lossless)
- .omg (ATRAC3, ATRAC3+)
- .kdr

**Notes on converting audio to ATRAC**

In both ATRAC3 and ATRAC9, the concept of audio channels exists.
For example, if you target a stereo WAVE file for conversion to a bitrate that the tool only supports mono, the conversion will fail.

In the case of ATRAC9, three different sampling frequencies can be specified.  
When specifying 12kHz or 24kHz, 12kHz means only 12kHz WAVE files.  

In the case of 24kHz, only 24kHz WAVE files can be specified.  
If there is no particular reason, it is recommended to convert at 48kHz.

**How to Set Loop Point Information for ATRAC for Consoles (Sony PlayStation Series)**

You can easily configure loop point information on the application's GUI.  
Additionally, more advanced settings are possible via the configuration window. (For experienced users)  
When configuring via the configuration window, you must adhere to the specification methods for at3tool.exe and at9tool.exe.  
If not adhered to, the application will return an error.

**About Walkman Features**

Walkman-specific features depend on the OpenMG Library.  
To convert audio files to the Walkman format, Sony Media Library Earth must be installed on your PC.  
The Walkman format does not support setting loop point information.  

Walkman features are currently still in development and do not support all functions.  
Unexpected bugs and issues may exist.  

**Supported Language**

- English
- Japanese
- Chinese (Simplified)

※Upon request, we can also support other languages.  

**System**

This application does not support x86 (32 bit).  
(If you need x86 for some reason, please build it from the C# source code)

## License

MIT
