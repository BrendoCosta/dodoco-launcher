#include <iostream>
#include <windows.h>
#include "Log.hpp"

void Dodoco::Native::Log(std::string message) {

    std::cout << message << std::endl;

    std::string msg = message.append("\r\n");

    HANDLE logFileHandle = CreateFileW(
        L"log",
        FILE_APPEND_DATA,
        0,
        nullptr,
        OPEN_ALWAYS,
        FILE_ATTRIBUTE_NORMAL,
        nullptr
    );

    if (!logFileHandle)
        return;

    WriteFile(logFileHandle, msg.c_str(), msg.size() * sizeof(char), nullptr, nullptr);
    CloseHandle(logFileHandle);

}

void Dodoco::Native::Log(std::wstring message) {

    return Dodoco::Native::Log(std::string(message.begin(), message.end()));

}