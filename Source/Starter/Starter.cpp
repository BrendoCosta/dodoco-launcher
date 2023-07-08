#include <windows.h>
#include <shlwapi.h>
#include "../DllInjector.hpp"
#include "../Log.hpp"
#include "../StringUtil.hpp"
#include "../Process.hpp"

int main(int argc, char* argv[]) {

    if (argc != 3) {

        Dodoco::Native::Log("Wrong number of arguments!");
        Dodoco::Native::Log("Usage: [GAME EXECUTABLE PATH] [DLL TO INJECT]");
        return EXIT_FAILURE;

    }

    std::wstring gameExecutablePath = Dodoco::Native::StringUtil::Utf8ToWide(std::string(argv[1]));
    std::wstring dllPath = Dodoco::Native::StringUtil::Utf8ToWide(std::string(argv[2]));

    if (!PathFileExistsW(gameExecutablePath.c_str())) {

        Dodoco::Native::Log(L"Can't find the game's executable file \"" + gameExecutablePath + L"\"");
        return EXIT_FAILURE;

    }

    if (!PathFileExistsW(dllPath.c_str())) {

        Dodoco::Native::Log(L"Can't find the dll file \"" + dllPath + L"\"");
        return EXIT_FAILURE;

    }

    Dodoco::Native::Log("Creating game's process...");

    STARTUPINFOW startupInfo = {};
    PROCESS_INFORMATION procInfo = {};

    if (!CreateProcessW(std::wstring(gameExecutablePath).c_str(), nullptr, nullptr, nullptr, FALSE, CREATE_SUSPENDED, nullptr, nullptr, &startupInfo, &procInfo)) {
        
        Dodoco::Native::Log("Failed to create the process");
        return EXIT_FAILURE;

    }

    Dodoco::Native::Log("Successfully created game's process");
    Dodoco::Native::Log("Injecting dll in game's process...");

    if (!Dodoco::Native::DllInjector::Inject(procInfo.hProcess, dllPath)) {

        Dodoco::Native::Log("Failed to inject the dll in game's process");
        Dodoco::Native::Process::CloseProcess(procInfo.hProcess, 9);
        Dodoco::Native::Process::CloseProcess(GetCurrentProcess(), EXIT_FAILURE);

    }

    Dodoco::Native::Log("Successfully injected the dll in game's process");
    Dodoco::Native::Log("Resuming game's process...");

    ResumeThread(procInfo.hThread);
    CloseHandle(procInfo.hProcess);

    return EXIT_SUCCESS;

}
