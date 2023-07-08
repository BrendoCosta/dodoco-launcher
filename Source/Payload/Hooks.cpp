#include <shlwapi.h>
#include "Hooks.hpp"
#include "../Log.hpp"
#include "../Process.hpp"

std::wstring GetHookDriverFullPath(std::wstring serviceName) {

    std::wstring driverToLoadFilename;
    WCHAR driverToLoadFullPath[MAX_PATH] = TEXT(L"");

    if (serviceName.compare(L"HoYoProtect") == 0) {

        Dodoco::Native::Log("Using HoYoProtect");
        driverToLoadFilename = L"HoYoKProtect.sys";

    } else if (serviceName.compare(L"mhyprot2") == 0 || serviceName.compare(L"mhyprot3") == 0) {

        Dodoco::Native::Log("Using mhyprot2 / mhyprot3");
        driverToLoadFilename = L"mhyprot3.Sys";

    } else {

        Dodoco::Native::Log(L"Unknown service \"" + serviceName + L"\"");
        Dodoco::Native::Process::CloseProcess(GetCurrentProcess(), EXIT_FAILURE);
        return nullptr;

    }

    if (!GetFullPathNameW(driverToLoadFilename.c_str(), MAX_PATH, driverToLoadFullPath, nullptr)) {

        Dodoco::Native::Log("\"GetFullPathNameW\" error (" + std::to_string(GetLastError()) + ")");
        Dodoco::Native::Process::CloseProcess(GetCurrentProcess(), EXIT_FAILURE);
        return nullptr;

    }

    return std::wstring(driverToLoadFullPath);

}

SC_HANDLE Dodoco::Native::Hooks::CreateServiceW_Hook(
    SC_HANDLE   hSCManager,
    LPCWSTR     lpServiceName,
    LPCWSTR     lpDisplayName,
    DWORD       dwDesiredAccess,
    DWORD       dwServiceType,
    DWORD       dwStartType,
    DWORD       dwErrorControl,
    LPCWSTR     lpBinaryPathName,
    LPCWSTR     lpLoadOrderGroup,
    LPDWORD     lpdwTagId,
    LPCWSTR     lpDependencies,
    LPCWSTR     lpServiceStartName,
    LPCWSTR     lpPassword
) {

    std::wstring serviceName(lpServiceName);

    if (serviceName.compare(L"HoYoProtect") == 0
        || serviceName.compare(L"mhyprot2") == 0
        || serviceName.compare(L"mhyprot3") == 0) {

        std::wstring driverToLoadFullPath(GetHookDriverFullPath(serviceName));

        if (driverToLoadFullPath.empty()) {

            Dodoco::Native::Log("\"GetHookDriverFullPath\" error (" + std::to_string(GetLastError()) + ")");
            Dodoco::Native::Process::CloseProcess(GetCurrentProcess(), EXIT_FAILURE);
            return nullptr;

        }

        return Dodoco::Native::Hooks::CreateServiceW_Backup(hSCManager, lpServiceName, lpDisplayName, dwDesiredAccess, dwServiceType, dwStartType, dwErrorControl, driverToLoadFullPath.c_str(), lpLoadOrderGroup, lpdwTagId, lpDependencies, lpServiceStartName, lpPassword);
        
    } else {

        return Dodoco::Native::Hooks::CreateServiceW_Backup(hSCManager, lpServiceName, lpDisplayName, dwDesiredAccess, dwServiceType, dwStartType, dwErrorControl, lpBinaryPathName, lpLoadOrderGroup, lpdwTagId, lpDependencies, lpServiceStartName, lpPassword);

    }

}

SC_HANDLE Dodoco::Native::Hooks::OpenServiceW_Hook(
    SC_HANDLE   hSCManager,
    LPCWSTR     lpServiceName,
    DWORD       dwDesiredAccess
) {

    std::wstring serviceName(lpServiceName);

    if (serviceName.compare(L"HoYoProtect") == 0
        || serviceName.compare(L"mhyprot2") == 0
        || serviceName.compare(L"mhyprot3") == 0) {

        SC_HANDLE schSCManagerHandle;
        SC_HANDLE originalServiceHandle;

        schSCManagerHandle = OpenSCManager( 
            nullptr,                // Local computer
            nullptr,                // ServicesActive database 
            SC_MANAGER_ALL_ACCESS   // Full access rights 
        );

        if (!schSCManagerHandle) {

            Dodoco::Native::Log("\"OpenSCManager\" error (" + std::to_string(GetLastError()) + ")");
            return nullptr;

        }

        originalServiceHandle = Dodoco::Native::Hooks::OpenServiceW_Backup(hSCManager, lpServiceName, SERVICE_ALL_ACCESS);

        if (!originalServiceHandle) {

            Dodoco::Native::Log("\"OpenServiceW_Backup\" error (" + std::to_string(GetLastError()) + ")");
            return nullptr;

        }

        std::wstring driverToLoadFullPath(GetHookDriverFullPath(serviceName));

        if (driverToLoadFullPath.empty()) {

            Dodoco::Native::Log("\"GetHookDriverFullPath\" error (" + std::to_string(GetLastError()) + ")");
            Dodoco::Native::Process::CloseProcess(GetCurrentProcess(), EXIT_FAILURE);
            return nullptr;

        }

        if (!ChangeServiceConfigW(
            originalServiceHandle,
            SERVICE_KERNEL_DRIVER,
            SERVICE_DEMAND_START,
            SERVICE_ERROR_NORMAL,
            driverToLoadFullPath.c_str(),
            nullptr,
            nullptr,
            nullptr,
            nullptr,
            nullptr,
            nullptr
        )) {

            Dodoco::Native::Log("\"ChangeServiceConfigW\" error (" + std::to_string(GetLastError()) + ")");
            return nullptr;

        }

        return originalServiceHandle;
        
    } else {

        return Dodoco::Native::Hooks::OpenServiceW_Backup(hSCManager, lpServiceName, dwDesiredAccess);

    }

}
