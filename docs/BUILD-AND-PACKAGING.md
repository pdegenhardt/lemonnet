# Build and Native Binary Packaging Strategy

## Overview

LemonNet uses a simplified approach to managing native binaries across Windows and Linux platforms. Instead of using the complex `runtimes/{rid}/native` folder structure, we place both platform-specific native libraries directly alongside the managed assemblies.

## Native Binary Locations

### Build Output
Both platforms output their native libraries to the same relative location:
- **Windows**: `src/LemonNet/bin/{Configuration}/net9.0/lemon_wrapper.dll`
- **Linux**: `src/LemonNet/bin/{Configuration}/net9.0/lemon_wrapper.so`

### Why This Approach?

1. **Simplicity**: No complex runtime identifier (RID) folder structures to manage
2. **Consistency**: Same relative path for both platforms makes build scripts simpler
3. **Automatic Discovery**: .NET runtime automatically finds native libraries in the same directory as the managed assembly
4. **No LD_LIBRARY_PATH Required**: Co-locating native libraries eliminates the need for environment variable configuration on Linux

## Build Process

### Windows Build
1. Visual Studio project (`LemonWrapper.Native.vcxproj`) builds the native DLL to `src/LemonNet.Native/bin/x64/{Configuration}/`
2. Post-build event copies `lemon_wrapper.dll` to `src/LemonNet/bin/{Configuration}/net9.0/`
3. The C# project references the native project to ensure correct build order

### Linux Build
1. `build.sh` script compiles the native library using g++
2. Outputs `lemon_wrapper.so` directly to `src/LemonNet/bin/{Configuration}/net9.0/`
3. No additional copy step required

### CI/CD Pipeline (GitHub Actions)

The workflow handles both platforms:

1. **Linux Job**:
   - Runs on `ubuntu-latest`
   - Executes `build.sh` to create `lemon_wrapper.so`
   - Uploads artifact from `src/LemonNet/bin/Release/net9.0/`

2. **Windows Job**:
   - Runs on `windows-latest`
   - Uses MSBuild to compile native DLL
   - Copies DLL to `src/LemonNet/bin/Release/net9.0/`
   - Uploads artifact from the output directory

3. **Test and Publish Job**:
   - Downloads both native libraries to `src/LemonNet/bin/Release/net9.0/`
   - Runs tests with both libraries present
   - Creates NuGet package containing both native libraries

## NuGet Package Structure

The published NuGet package includes both native libraries in the same location:
```
LemonNet.nupkg
├── lib/
│   └── net9.0/
│       ├── LemonNet.dll
│       ├── lemon_wrapper.dll  (Windows native)
│       └── lemon_wrapper.so   (Linux native)
```

When a consumer installs the package, the appropriate native library is automatically available based on their platform.

## Platform Detection

The P/Invoke in `LemonMaxFlow.cs` uses a platform-agnostic approach:
```csharp
[DllImport("lemon_wrapper", CallingConvention = CallingConvention.Cdecl)]
```

The .NET runtime automatically:
- Looks for `lemon_wrapper.dll` on Windows
- Looks for `lemon_wrapper.so` on Linux/macOS
- Searches in the same directory as the calling assembly first

## Developer Setup

### Windows Development
1. Open `LemonNet.sln` in Visual Studio 2022+
2. Build solution (F6) - native DLL is automatically built and copied
3. Run tests (Ctrl+R, A)

### Linux Development
1. Run `./build.sh` to build native library
2. Run `dotnet build` to build managed code
3. Run `dotnet test` to execute tests

### WSL Development (Windows Subsystem for Linux)
1. The Visual Studio project attempts to build Linux binaries via WSL
2. If WSL is not available, the build continues (non-fatal)
3. For cross-platform testing on Windows, install WSL2 and run `build.sh`

## Troubleshooting

### Common Issues

1. **DllNotFoundException**
   - Verify native library exists in output directory
   - Check that architecture matches (x64 only supported)
   - Ensure Visual C++ redistributables are installed (Windows)

2. **BadImageFormatException**
   - Platform mismatch (32-bit vs 64-bit)
   - Ensure all projects target x64

3. **Duplicate Output Paths**
   - If you see nested `bin/Debug/net9.0/bin/Debug/net9.0/`, check the `.csproj` file
   - Use `$(OutputPath)` MSBuild variable instead of hardcoded paths

## Design Decisions

### Why Not Use Runtime Identifiers (RIDs)?

While .NET supports RID-specific folders (`runtimes/win-x64/native/`), we chose simplicity:
- Single output location for all platforms
- Easier debugging and troubleshooting
- Simpler build scripts and CI/CD pipelines
- Package consumers don't need to understand RID resolution

### Why x64 Only?

- LEMON library is optimized for 64-bit architectures
- Simplifies build matrix (no need for x86, ARM variants)
- Modern servers and development machines are predominantly x64
- Reduces package size and complexity

### Why Bundle Both Native Libraries?

- Single NuGet package works on both Windows and Linux
- No need for platform-specific packages
- Minimal size overhead (~200KB per platform)
- Simplifies deployment for cross-platform applications

## Future Considerations

If we need to support additional platforms or architectures:
1. Consider adopting RID-specific folders for more complex scenarios
2. Implement conditional package references based on target platform
3. Create platform-specific NuGet packages if size becomes a concern

For now, the simple approach of co-locating native binaries provides the best balance of simplicity and functionality.