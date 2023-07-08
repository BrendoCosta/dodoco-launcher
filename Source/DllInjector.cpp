#include "DllInjector.hpp"
#include "Log.hpp"

bool Dodoco::Native::DllInjector::Inject(HANDLE processHandle, const std::wstring dllPath) {

    std::size_t dllPathByteSize = (dllPath.size() + 1) * sizeof(wchar_t);
    
    void* dllBaseMemoryAddress = VirtualAllocEx(
		processHandle,
		nullptr,            // Let the system decide where to allocate the memory
		dllPathByteSize,
		MEM_COMMIT,         // Commit the virtual memory
		PAGE_READWRITE      // Access for committed page
    );

    if (!dllBaseMemoryAddress) {

        Dodoco::Native::Log("Failed to write to allocate memory in target process");
        return false;

    }

    if (!WriteProcessMemory(processHandle, dllBaseMemoryAddress, (LPCVOID) (dllPath.c_str()), dllPathByteSize, nullptr)) {

        Dodoco::Native::Log("Failed to write to target process' memory");
        return false;

    }
    
    PTHREAD_START_ROUTINE loadLibraryWThreadRoutine = (PTHREAD_START_ROUTINE) GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryW");
    
    if (!loadLibraryWThreadRoutine) {

        Dodoco::Native::Log("Failed to get LoadLibraryW thread routine");
        return false;

    }

    HANDLE remoteThreadHandle = CreateRemoteThread(
		processHandle,
		nullptr,                    // Default security attributes
		0,                          // Thread's stack size
		loadLibraryWThreadRoutine,
		dllBaseMemoryAddress,
		0,                          // Thread should run immediately after creation
		nullptr                     // Don't return the thread identifier
    );

    if (!remoteThreadHandle) {

        Dodoco::Native::Log("Failed to create the remote thread in target process");
        return false;

    }

    // Wait until remote thread end

	WaitForSingleObject(remoteThreadHandle, INFINITE);

    // Free allocated resources

    if (!VirtualFreeEx(processHandle, dllBaseMemoryAddress, 0, MEM_RELEASE)) {

        Dodoco::Native::Log("Failed to release the allocated memory from target process");
        return false;

    }
    
    CloseHandle(remoteThreadHandle);

    return true;

}
