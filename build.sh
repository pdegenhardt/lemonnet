#!/bin/bash

echo "Building LEMON wrapper library for Linux..."

# Change to the native source directory
cd src/LemonNet.Native

# Set the include path to the LEMON library
LEMON_INCLUDE="-Ilemon-1.3.1"

# Create the runtime directory if it doesn't exist
mkdir -p ../LemonNet/runtimes/linux-x64/native

# Compile the wrapper
g++ -fPIC -shared -O2 -std=c++11 \
    $LEMON_INCLUDE \
    lemon_wrapper.cpp \
    lemon-1.3.1/lemon/bits/windows.cc \
    -o ../LemonNet/runtimes/linux-x64/native/lemon_wrapper.so \
    -DLEMON_WRAPPER_EXPORTS

if [ $? -eq 0 ]; then
    echo "Build successful! Created lemon_wrapper.so in src/LemonNet/runtimes/linux-x64/native/"
    echo ""
    echo "The library has been placed in the correct NuGet runtime location."
else
    echo "Build failed!"
    exit 1
fi