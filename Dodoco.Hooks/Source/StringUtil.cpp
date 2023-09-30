#include <stringapiset.h>
#include "StringUtil.hpp"

std::string Dodoco::Native::StringUtil::WideToANSI(const std::wstring& wstr) {

    int count = WideCharToMultiByte(CP_ACP, 0, wstr.c_str(), wstr.length(), nullptr, 0, nullptr, nullptr);
    std::string str(count, 0);
    WideCharToMultiByte(CP_ACP, 0, wstr.c_str(), -1, &str[0], count, nullptr, nullptr);
    return str;

}

std::wstring Dodoco::Native::StringUtil::AnsiToWide(const std::string& str) {

    int count = MultiByteToWideChar(CP_ACP, 0, str.c_str(), str.length(), NULL, 0);
    std::wstring wstr(count, 0);
    MultiByteToWideChar(CP_ACP, 0, str.c_str(), str.length(), &wstr[0], count);
    return wstr;

}

std::string Dodoco::Native::StringUtil::WideToUtf8(const std::wstring& wstr) {

    int count = WideCharToMultiByte(CP_UTF8, 0, wstr.c_str(), wstr.length(), NULL, 0, NULL, NULL);
    std::string str(count, 0);
    WideCharToMultiByte(CP_UTF8, 0, wstr.c_str(), -1, &str[0], count, NULL, NULL);
    return str;

}

std::wstring Dodoco::Native::StringUtil::Utf8ToWide(const std::string& str) {

    int count = MultiByteToWideChar(CP_UTF8, 0, str.c_str(), str.length(), NULL, 0);
    std::wstring wstr(count, 0);
    MultiByteToWideChar(CP_UTF8, 0, str.c_str(), str.length(), &wstr[0], count);
    return wstr;

}