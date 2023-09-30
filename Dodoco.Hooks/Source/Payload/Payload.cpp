#include <exception>
#include <stdexcept>
#include <string>
#include <windows.h>
#include <psapi.h>
#include <tlhelp32.h>
#include <ntdef.h>
#include "Hooks.hpp"
#include "../Log.hpp"
#include "../Process.hpp"
#include <MinHook.h>

/*
 * Author: Kyle Halladay
 * http://kylehalladay.com/blog/2020/11/13/Hooking-By-Example.html
*/
HMODULE FindModuleInProcess(HANDLE process, const char* name) {

	char* lowerCaseName = _strdup(name);
	_strlwr_s(lowerCaseName, strlen(name) + 1);

	HMODULE remoteProcessModules[1024];
	DWORD numBytesWrittenInModuleArray = 0;
	BOOL success = EnumProcessModules(process, remoteProcessModules, sizeof(HMODULE) * 1024, &numBytesWrittenInModuleArray);

	if (!success) {
        
        Dodoco::Native::Log(L"Error enumerating modules from target process (error " + std::to_wstring(GetLastError()) + L")");
        return nullptr;

	}

	DWORD numRemoteModules = numBytesWrittenInModuleArray / sizeof(HMODULE);
	CHAR remoteProcessName[256];
	GetModuleFileNameEx(process, NULL, remoteProcessName, 256);
	_strlwr_s(remoteProcessName, 256);

	MODULEINFO remoteProcessModuleInfo;
	HMODULE remoteProcessModule = 0;

	for (DWORD i = 0; i < numRemoteModules; ++i) {

		CHAR moduleName[256];
		GetModuleFileNameEx(process, remoteProcessModules[i], moduleName, 256);
		_strlwr_s(moduleName, 256);
		char* lastSlash = strrchr(moduleName, '\\');
		if (!lastSlash) lastSlash = strrchr(moduleName, '/');

		char* dllName = lastSlash + 1;

		if (strcmp(dllName, lowerCaseName) == 0) {

			remoteProcessModule = remoteProcessModules[i];

			success = GetModuleInformation(process, remoteProcessModules[i], &remoteProcessModuleInfo, sizeof(MODULEINFO));
			free(lowerCaseName);
			return remoteProcessModule;

		}

	}

	free(lowerCaseName);
	return 0;
		
}

bool InstallHook(std::string targetModuleName, std::string targetFunctionName, void* hookFunctionAddress, void* backupFunctionAddress) {

    Dodoco::Native::Log(
        std::string("InstallHook(")
        .append(targetModuleName).append(", ")
        .append(targetFunctionName).append(", ")
        .append(std::to_string((uint64_t) hookFunctionAddress)).append(", ")
        .append(std::to_string((uint64_t) backupFunctionAddress)).append(")")
    );

    if (!hookFunctionAddress) {

        Dodoco::Native::Log("Invalid hook function address");
        return false;

    }
    
    if (!backupFunctionAddress) {

        Dodoco::Native::Log("Invalid backup function address");
        return false;

    }

    HMODULE targetModuleAddress = FindModuleInProcess(GetCurrentProcess(), targetModuleName.c_str());
    MH_STATUS minHookStatus = MH_STATUS::MH_UNKNOWN;

    if (!targetModuleAddress) {

        Dodoco::Native::Log("Can't get the target module's address");
        return false;

    }

    void* targetFunctionAddress = (void*) GetProcAddress(targetModuleAddress, targetFunctionName.c_str());

    if (!targetFunctionAddress) {

        Dodoco::Native::Log("Can't get the target function's address");
        return false;

    }

    minHookStatus = MH_CreateHook(targetFunctionAddress, hookFunctionAddress, reinterpret_cast<LPVOID*>(backupFunctionAddress));
    if (minHookStatus != MH_OK) {

        Dodoco::Native::Log("Failed to create the hook for the function \"" + targetFunctionName + "\" (status " + std::to_string(minHookStatus) + ")");
        return false;

    }

    minHookStatus = MH_EnableHook(targetFunctionAddress);
    if (minHookStatus != MH_OK) {

        Dodoco::Native::Log("Failed to enable the hook for the function \"" + targetFunctionName + "\" (status " + std::to_string(minHookStatus) + ")");
        return false;

    }

    Dodoco::Native::Log("Successfully installed the hook for the function \"" + targetFunctionName + "\"");
    return true;

}

bool Initialize(void) {

    if (MH_Initialize() != MH_OK) {

        Dodoco::Native::Log("Failed to initialize MinHook");
        return false;

    }

    if (!InstallHook("advapi32.dll", "CreateServiceW", (void*) Dodoco::Native::Hooks::CreateServiceW_Hook, &Dodoco::Native::Hooks::CreateServiceW_Backup)
		|| !InstallHook("advapi32.dll", "OpenServiceW", (void*) Dodoco::Native::Hooks::OpenServiceW_Hook, &Dodoco::Native::Hooks::OpenServiceW_Backup)
		) {

        Dodoco::Native::Log("Failed to install hooks");
        return false;
        
    }

    return true;

}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved) {

    if (ul_reason_for_call == DLL_PROCESS_ATTACH) {

        if (!Initialize()) {

			Dodoco::Native::Process::CloseProcess(GetCurrentProcess(), EXIT_FAILURE);

        }

    }

    return TRUE;

}
