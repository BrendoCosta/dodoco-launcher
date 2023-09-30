set(COMMON_DRIVER_SOURCE_FILES "Driver.c")
set(COMMON_DRIVER_COMPILE_OPTIONS BEFORE PRIVATE -I /usr/share/mingw-w64/include/ddk)
set(COMMON_DRIVER_LINK_FLAGS -Wl,--subsystem,native -Wl,--image-base,0x10000 -Wl,--file-alignment,0x1000 -Wl,--section-alignment,0x1000 -Wl,--entry,DriverEntry@8 -Wl,--stack,0x40000 -Wl,--dynamicbase -Wl,--nxcompat -nostartfiles -nostdlib -lntoskrnl -lhal)

add_executable(HoYoKProtect WIN32 ${COMMON_DRIVER_SOURCE_FILES})
target_compile_options(HoYoKProtect ${COMMON_DRIVER_COMPILE_OPTIONS})
target_compile_definitions(HoYoKProtect PRIVATE DRIVER_DEVICE_PATH=L"\\\\Device\\\\HoYoProtect" DRIVER_SYMLINK_PATH=L"\\\\??\\\\HoYoProtect")
target_link_libraries(HoYoKProtect ${COMMON_DRIVER_LINK_FLAGS})
set_target_properties(HoYoKProtect PROPERTIES SUFFIX ".sys")

add_executable(mhyprot3 WIN32 ${COMMON_DRIVER_SOURCE_FILES})
target_compile_options(mhyprot3 ${COMMON_DRIVER_COMPILE_OPTIONS})
target_compile_definitions(mhyprot3 PRIVATE DRIVER_DEVICE_PATH=L"\\\\Device\\\\mhyprot2" DRIVER_SYMLINK_PATH=L"\\\\??\\\\mhyprot2")
target_link_libraries(mhyprot3 ${COMMON_DRIVER_LINK_FLAGS})
set_target_properties(mhyprot3 PROPERTIES SUFFIX ".Sys")

set_target_properties(HoYoKProtect mhyprot3 PROPERTIES PREFIX "")