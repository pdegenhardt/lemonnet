#!/bin/bash

echo "Building LEMON wrapper library for Linux..."

# Change to the native source directory
cd src/LemonNet.Native

# Set the include path to the LEMON library
LEMON_INCLUDE="-Ilemon-1.3.1"

# Determine configuration (default to Debug if not specified)
CONFIGURATION=${1:-Debug}

# Create the output directory if it doesn't exist
mkdir -p ../LemonNet/bin/$CONFIGURATION/net9.0

# Compile the wrapper
g++ -fPIC -shared -O2 -std=c++11 \
    $LEMON_INCLUDE \
    lemon_wrapper.cpp \
    lemon-1.3.1/lemon/bits/windows.cc \
    -o ../LemonNet/bin/$CONFIGURATION/net9.0/lemon_wrapper.so \
    -DLEMON_WRAPPER_EXPORTS

if [ $? -eq 0 ]; then
    echo "Build successful! Created lemon_wrapper.so in src/LemonNet/bin/$CONFIGURATION/net9.0/"
    echo ""
    echo "The library has been placed directly in the output folder alongside LemonNet.dll"
else
    echo "Build failed!"
    exit 1
fi