#ifndef PROCESS_H_INCLUDED
#define PROCESS_H_INCLUDED

#include <windows.h>

namespace Dodoco::Native::Process {

    void CloseProcess(HANDLE processHandle, int exitCode);

}

#endif // PROCESS_H_INCLUDED