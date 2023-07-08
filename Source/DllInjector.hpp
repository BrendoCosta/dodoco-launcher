#ifndef INJECTOR_H_INCLUDED
#define INJECTOR_H_INCLUDED

#include <windows.h>
#include <string>

namespace Dodoco::Native {

    class DllInjector {

        public:
        
            static bool Inject(HANDLE processHandle, const std::wstring dllPath);

    };

}

#endif // INJECTOR_H_INCLUDED