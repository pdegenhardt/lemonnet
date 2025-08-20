cmake_minimum_required(VERSION 3.10)
project(LemonWrapper)

set(CMAKE_CXX_STANDARD 11)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Add LEMON include directory
include_directories(${CMAKE_CURRENT_SOURCE_DIR}/lemon-1.3.1)

# Create the shared library
add_library(lemon_wrapper SHARED
    lemon_wrapper.cpp
    lemon_wrapper.h
)

# Define the export macro
target_compile_definitions(lemon_wrapper PRIVATE LEMON_WRAPPER_EXPORTS)

# Set properties for Windows
if(WIN32)
    set_target_properties(lemon_wrapper PROPERTIES
        WINDOWS_EXPORT_ALL_SYMBOLS ON
    )
endif()

# Installation rules
install(TARGETS lemon_wrapper
    LIBRARY DESTINATION lib
    RUNTIME DESTINATION bin
)

install(FILES lemon_wrapper.h
    DESTINATION include
)