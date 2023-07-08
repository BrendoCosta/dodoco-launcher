#ifndef LOGGER_H_INCLUDED
#define LOGGER_H_INCLUDED

#include <string>

namespace Dodoco::Native {

    void Log(std::string message);
    void Log(std::wstring message);

}

#endif // LOGGER_H_INCLUDED