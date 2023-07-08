#ifndef HOOKS_H_INCLUDED
#define HOOKS_H_INCLUDED

#include <windows.h>

namespace Dodoco::Native::Hooks {

    /*
     * Function pointers
    */

    using CreateServiceW_Ptr = SC_HANDLE (WINAPI*)(
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
    );

    using OpenServiceW_Ptr = SC_HANDLE (WINAPI*)(
        SC_HANDLE   hSCManager,
        LPCWSTR     lpServiceName,
        DWORD       dwDesiredAccess
    );

    inline CreateServiceW_Ptr CreateServiceW_Backup;
    inline OpenServiceW_Ptr OpenServiceW_Backup;

    /*
     * Hooks declaration
    */

    SC_HANDLE CreateServiceW_Hook(
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
    );

    SC_HANDLE OpenServiceW_Hook(
        SC_HANDLE   hSCManager,
        LPCWSTR     lpServiceName,
        DWORD       dwDesiredAccess
    );
}

#endif // HOOKS_H_INCLUDED