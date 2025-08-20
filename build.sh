#!/bin/bash

echo "Building LEMON wrapper library for Linux..."

# Change to the native source directory
cd src/LemonNet.Native

# Set the include path to the LEMON library
LEMON_INCLUDE="-Ilemon-1.3.1"

# Compile the wrapper
g++ -fPIC -shared -O2 -std=c++11 \
    $LEMON_INCLUDE \
    lemon_wrapper.cpp \
    lemon-1.3.1/lemon/bits/windows.cc \
    -o ../../lemon_wrapper.so \
    -DLEMON_WRAPPER_EXPORTS

if [ $? -eq 0 ]; then
    echo "Build successful! Created lemon_wrapper.so"
    echo ""
    echo "To use in your C# application:"
    echo "1. Make sure lemon_wrapper.so is in the same directory as your executable"
    echo "2. Or add its directory to LD_LIBRARY_PATH"
else
    echo "Build failed!"
    exit 1
fi