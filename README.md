# ATRACTool Reloaded

![Downloads](https://img.shields.io/github/downloads/XyLe-GBP/ATRACTool-Reloaded/total.svg)
[![GitHub (pre-)release](https://img.shields.io/github/release/XyLe-GBP/ATRACTool-Reloaded/all.svg)](https://github.com/XyLe-GBP/ATRACTool-Reloaded/releases)

Utility tool to convert Sony's ATRAC3/ATRAC3plus/ATRAC9(.AT3/.AT9) to Wave(.WAV) sound.  

Or convert Wave sound to Sony's ATRAC3/ATRAC3plus/ATRAC9.  

User interface functions are provided for SCEI ATRAC3plus Codec Tool (at3tool.exe) and SCEI ATRAC9 Codec Tool (at9tool.exe).

**Download:**
[Release build](https://github.com/XyLe-GBP/ATRACTool-Reloaded/releases)

This application has been published and can be run without installing the .NET runtime.

※If for some reason you want to use the portable version, you will need to install the runtime.

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

**Notes on converting audio to ATRAC**

In both ATRAC3 and ATRAC9, the concept of audio channels exists.
For example, if you target a stereo WAVE file for conversion to a bitrate that the tool only supports mono, the conversion will fail.

In the case of ATRAC9, three different sampling frequencies can be specified.  
When specifying 12kHz or 24kHz, 12kHz means only 12kHz WAVE files.  

In the case of 24kHz, only 24kHz WAVE files can be specified.  
If there is no particular reason, it is recommended to convert at 48kHz.

**How to set a loop point for ATRAC**

When setting the loop point for ATRAC9, the following conditions must be met before conversion can be performed.

> specify the loop end point is E samples from the beginning  
            S and E must satisfy under condition  
            (-fs:12000Hz)  
            0 <= S < S + 3071 <= E < number of samples in file1(12000Hz PCM)  
            (-fs:24000Hz)  
            0 <= S < S + 3071 <= E < number of samples in file1(24000Hz PCM)  
            (-fs:48000Hz)  
            0 <= S < S +  511 <= E < number of samples in file1( 8000Hz PCM)  
            0 <= S < S +  767 <= E < number of samples in file1(12000Hz PCM)  
            0 <= S < S + 1023 <= E < number of samples in file1(16000Hz PCM)  
            0 <= S < S + 1535 <= E < number of samples in file1(24000Hz PCM)  
            0 <= S < S + 2047 <= E < number of samples in file1(32000Hz PCM)  
            0 <= S < S + 2821 <= E < number of samples in file1(44100Hz PCM)  
            0 <= S < S + 3071 <= E < number of samples in file1(48000Hz PCM)  

**Supported Language**

- English
- Japanese
- Chinese (under development)

**System**

This application does not support x86 (32 bit).  
(If you need x86 for some reason, please build it from the C# source code)

## License

MIT
