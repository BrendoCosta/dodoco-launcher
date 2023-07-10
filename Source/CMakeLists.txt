cmake_minimum_required(VERSION 3.18)
project(dodoco-hooks LANGUAGES C CXX)
set(CMAKE_CXX_FLAGS "-std=c++20 -static-libgcc -static-libstdc++ -Wall -pedantic")
set(CMAKE_C_FLAGS "-Wall -pedantic")

set(CMAKE_SIZEOF_VOID_P 8)
SET(CMAKE_SYSTEM_PROCESSOR AMD64)
SET(CMAKE_CROSSCOMPILING 1)

set(COMMON_SOURCE_FILES
    "DllInjector.cpp"
    "Log.cpp"
    "Process.cpp"
    "StringUtil.cpp"
)
add_library(DodocoCommon STATIC ${COMMON_SOURCE_FILES})

add_subdirectory(Library/minhook)
    include_directories(Library/minhook/include)
    link_directories(Library/minhook)
add_subdirectory(Library/argparse)
    include_directories(Library/argparse/include)

add_subdirectory(Payload)
add_subdirectory(Driver)
add_subdirectory(Starter)