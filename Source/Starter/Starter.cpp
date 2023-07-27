#include <iostream>
#include <windows.h>
#include <shlwapi.h>
#include <argparse/argparse.hpp>
#include "../DllInjector.hpp"
#include "../Log.hpp"
#include "../StringUtil.hpp"
#include "../Process.hpp"

int main(int argc, char* argv[]) {

    argparse::ArgumentParser program("Starter");

    program.add_argument("gamepath")
        .help("Path to game's executable file (\"GenshinImpact.exe\" or \"YuanShen.exe\")");

    try {

        program.parse_args(argc, argv);

        std::wstring gameExecutablePath = Dodoco::Native::StringUtil::Utf8ToWide(program.get<std::string>("gamepath"));
        std::wstring dllPath = L"Payload.dll";

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

    } catch (const std::runtime_error& err) {

        std::cerr << err.what() << std::endl;
        std::cerr << program;
        Dodoco::Native::Process::CloseProcess(GetCurrentProcess(), EXIT_FAILURE);
        return EXIT_FAILURE;
        
    }

}
