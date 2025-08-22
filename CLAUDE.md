# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Overview

This is a C# wrapper for the LEMON (Library for Efficient Modeling and Optimization in Networks) graph library's Edmonds-Karp maximum flow algorithm. The project bridges C++ template-heavy code to C# through a C ABI wrapper layer.

## Project Structure and Architecture

### Three-Layer Architecture
1. **LEMON C++ Library** (`src/LemonNet.Native/lemon-1.3.1/`) - Original template-based graph library
2. **C Wrapper Layer** (`src/LemonNet.Native/lemon_wrapper.cpp/h`) - Exposes C++ templates through C ABI for P/Invoke
3. **C# Managed Layer** (`src/LemonNet/LemonMaxFlow.cs`) - Type-safe .NET API using P/Invoke

### Key Architectural Decisions
- **x64 only** - No 32-bit support to simplify platform management
- **P/Invoke over C++/CLI** - For cross-platform compatibility with .NET Core/5+
- **C wrapper required** - LEMON uses heavy C++ templating that cannot be directly P/Invoked
- **Project references ensure build order** - Native project builds before C# projects automatically

## Building the Solution

### Visual Studio (Primary Method)
```cmd
# Open solution in Visual Studio 2022+
# Select Debug|x64 or Release|x64
# Build → Rebuild Solution
# F5 to run tests
```

### Command Line Build
```cmd
# Requires VS Developer Command Prompt
dotnet restore LemonNet.sln
msbuild LemonNet.sln /p:Configuration=Release /p:Platform=x64
```

### Creating NuGet Package
```cmd
# From VS Developer Command Prompt
# 1. Restore packages
dotnet restore LemonNet.sln

# 2. Build entire solution (including native C++ project)
msbuild LemonNet.sln /p:Configuration=Release /p:Platform=x64

# 3. Create NuGet package
msbuild src/LemonNet/LemonNet.csproj /t:Pack /p:Configuration=Release
```

Note: `dotnet pack` cannot be used directly because it doesn't support C++ project references. Use MSBuild's Pack target instead.

### Native Library Only (if needed)
```cmd
# From VS Developer Command Prompt
cd src\LemonNet.Native
msbuild LemonWrapper.Native.vcxproj /p:Configuration=Release /p:Platform=x64
```

## Running Tests

### Visual Studio
- Set `LemonNetTest` as startup project
- Press F5 (debug) or Ctrl+F5 (run without debugging)

### Command Line
```cmd
cd tests\LemonNet.Tests\bin\Debug\net9.0
LemonNetTest.exe
```

## Critical Implementation Details

### Static Member Initialization (lemon_wrapper.cpp)
LEMON requires explicit instantiation of static template members:
```cpp
namespace lemon {
    float Tolerance<float>::def_epsilon = static_cast<float>(1e-4);
    double Tolerance<double>::def_epsilon = 1e-10;
    long double Tolerance<long double>::def_epsilon = 1e-14;
    const Invalid INVALID = Invalid();
}
```

### Windows-Specific Requirements
- Must include `lemon-1.3.1/lemon/bits/windows.cc` for Windows threading primitives
- Define `WIN32`, `_WIN32`, `_WINDOWS` preprocessor macros
- `LEMON_WRAPPER_EXPORTS` must be defined when building the DLL

### Native Library Location Flow
1. Windows: Native build outputs to `src/LemonNet.Native/bin/x64/[Debug|Release]/lemon_wrapper.dll`
2. Linux: build.sh outputs to `src/LemonNet/bin/[Debug|Release]/net9.0/lemon_wrapper.so`
3. Both libraries are co-located with LemonNet.dll in the same output folder

### Platform Configuration
- Solution platform: `x64`
- C# projects: Build as `AnyCPU` but with `<PlatformTarget>x64</PlatformTarget>`
- Native project: Build as `x64`
- This ensures no `BadImageFormatException` errors

## Common Issues and Solutions

### DllNotFoundException
- Ensure native project built successfully
- Check that `lemon_wrapper.dll` (Windows) or `lemon_wrapper.so` (Linux) exists in the output folder alongside `LemonNet.dll`
- Verify DLL is x64: `file lemon_wrapper.dll` should show "PE32+ executable"

### BadImageFormatException
- Platform mismatch between C# and native DLL
- Ensure building with x64 configuration
- Clean and rebuild entire solution

### Unresolved External Symbols
- Missing static member definitions (see Static Member Initialization above)
- Missing Windows-specific source files (`windows.cc`)
- Incorrect preprocessor definitions

## Project Dependencies

### Build Requirements
- Visual Studio 2022+ with C++ workload
- .NET 9.0 SDK
- Windows SDK
- C++14 or later compiler support

### Runtime Requirements
- .NET 9.0 runtime
- Windows x64 OS
- Visual C++ redistributables (typically installed with VS)

## Key Files to Understand

### For P/Invoke Interface
- `src/LemonNet.Native/lemon_wrapper.h` - C API exposed to C#
- `src/LemonNet/LemonMaxFlow.cs` - P/Invoke declarations and C# wrapper

### For Build Configuration
- `LemonNet.sln` - Solution configuration (x64 only)
- `src/LemonNet.Native/LemonWrapper.Native.vcxproj` - Native build settings
- `src/LemonNet/LemonNet.csproj` - Manages native DLL copying

### For LEMON Integration
- `src/LemonNet.Native/lemon_wrapper.cpp` - Template instantiation and C++ to C bridging
- `src/LemonNet.Native/lemon-1.3.1/lemon/edmonds_karp.h` - Algorithm being wrapped
- `src/LemonNet.Native/lemon-1.3.1/lemon/smart_graph.h` - Graph implementation used