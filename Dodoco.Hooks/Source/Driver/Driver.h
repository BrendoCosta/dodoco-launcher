#ifndef DRIVER_H_INCLUDED
#define DRIVER_H_INCLUDED

#define NTDDI_VERSION NTDDI_WIN10

#include <ddk/wdm.h>
#include <ddk/ntddk.h>
#include <ddk/ntifs.h>
#include <ddk/ntstrsafe.h>

#ifndef DRIVER_DEVICE_PATH
    #define DRIVER_DEVICE_PATH L"\\Device\\HoYoProtect"
#endif // DRIVER_DEVICE_PATH

#ifndef DRIVER_SYMLINK_PATH
    #define DRIVER_SYMLINK_PATH L"\\??\\HoYoProtect"
#endif // DRIVER_SYMLINK_PATH

#endif // DRIVER_H_INCLUDED