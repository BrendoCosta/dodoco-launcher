#ifndef STRINGUTIL_H_INCLUDED
#define STRINGUTIL_H_INCLUDED

#include <string>

namespace Dodoco::Native::StringUtil {

    std::string WideToANSI(const std::wstring& wstr);
    std::wstring AnsiToWide(const std::string& str);
    std::string WideToUtf8(const std::wstring& wstr);
    std::wstring Utf8ToWide(const std::string& str);

}

#endif // STRINGUTIL_H_INCLUDED