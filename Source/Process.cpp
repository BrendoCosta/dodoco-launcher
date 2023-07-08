#include "Process.hpp"

void Dodoco::Native::Process::CloseProcess(HANDLE processHandle, int exitCode) {

    if (exitCode == EXIT_FAILURE) {

        MessageBoxW(nullptr, L"Dodoco Hooks error. See the log for more information.", L"Dodoco Hooks", MB_OK);

    }

    TerminateProcess(processHandle, exitCode);

}