# Build Instructions

The filter code is written in [Haxe](https://haxe.org/) and depends on several C++ libraries. Install Haxe before proceeding.

1. Install `hxcpp` with `haxelib install hxcpp`.

2. Download and unpack [zlib](https://zlib.net/) or [cloudflare-zlib](https://github.com/cloudflare/zlib). Open x64 Native Tools Command Prompt for Visual Studio and locate the source directory. Build with `nmake /f win32\Makefile.msc`. Create a `bin` directory here and copy the generated `zlib.lib` in there.

3. Run `haxe -main MainFilterNew.hx -cpp bin -D HXCPP_GC_BIG_BLOCKS -D HXCPP_GC_MOVING -D HXCPP_M64 -D HXCPP_OPTIMIZE_LINK -D HXCPP_SINGLE_THREADED_APP -D HXCPP_VISIT_ALLOCS -D analyzer-optimize -dce full`. The executable should be placed under `bin\MainFilterNew.exe`.

4. Optionally run `mt -manifest MainFilterNew.manifest -outputresource:MainFilterNew.exe;#1` to enforce UTF-8 encoding on Windows 10 or higher.
