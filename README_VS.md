# Visual Studio Setup for LEMON C# Wrapper

## Prerequisites

- Visual Studio 2022 or later with .NET 9.0 SDK
- C++ development workload installed in Visual Studio
- Windows SDK

## Building and Debugging

### Step 1: Build the Native Library

Open a **Visual Studio Developer Command Prompt** and run:

```cmd
build-vs.bat
```

This creates both x64 and x86 versions of the native library in their respective folders.

### Step 2: Open in Visual Studio

1. Double-click `LemonNet.sln` to open the solution in Visual Studio
2. The solution will automatically load the test project

### Step 3: Debug the Application

1. Set breakpoints in `TestProgram.cs` or `LemonMaxFlow.cs`
2. Select Debug configuration and your preferred platform (AnyCPU, x64, or x86)
3. Press **F5** to start debugging

### Debug Features Available

- **Step Through Code**: F10 (Step Over), F11 (Step Into)
- **Inspect Variables**: Hover over variables or use Locals/Watch windows
- **Call Stack**: View the complete call stack
- **Immediate Window**: Execute C# code during debugging
- **Exception Settings**: Configure which exceptions to break on

### Platform Configuration

The project supports multiple platforms:
- **AnyCPU**: Will run as x64 on 64-bit Windows, x86 on 32-bit
- **x64**: Forces 64-bit execution (requires x64 lemon_wrapper.dll)
- **x86**: Forces 32-bit execution (requires x86 lemon_wrapper.dll)

### Troubleshooting

If you get a DllNotFoundException:
1. Ensure you've run `build-vs.bat` from a VS Developer Command Prompt
2. Check that the appropriate DLL exists:
   - For x64: `x64\lemon_wrapper.dll`
   - For x86: `x86\lemon_wrapper.dll`
   - For AnyCPU: `lemon_wrapper.dll` in project root
3. Clean and rebuild the solution

### Native Debugging (Optional)

To debug into the C++ wrapper code:
1. Select the "LemonNetTest (with Native Debugging)" launch profile
2. Enable mixed-mode debugging in project properties
3. Set breakpoints in `lemon_wrapper.cpp`
4. Press F5 to debug both managed and native code

## Project Structure

```
LemonNet/
├── LemonNet.sln                  # Visual Studio solution
├── LemonNetTest.csproj          # C# project file
├── Properties/
│   └── launchSettings.json      # Debug profiles
├── LemonMaxFlow.cs              # C# wrapper class
├── TestProgram.cs               # Test application
├── lemon_wrapper.h/cpp          # Native C++ wrapper
├── build-vs.bat                 # VS build script
├── x64/
│   └── lemon_wrapper.dll       # 64-bit native library
└── x86/
    └── lemon_wrapper.dll       # 32-bit native library
```