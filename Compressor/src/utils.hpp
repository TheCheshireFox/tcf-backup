#pragma once

#include <string>
#include <sstream>

#define ENUM_TO_STR_METHOD(method, ...)\
std::string method(auto arg)\
{\
    switch (arg)\
    {\
        __VA_ARGS__\
    }\
}\

#define ENUM_TO_STR(value) case value: return #value;
#define CENUM_TO_STR(type, value) case type::value: return #value;
#define ENUM_TO_STR_DEFAULT(value) default: return value;

#define MKSTR(x) (std::stringstream() << x).str()

template<typename... VALS_T>
bool is_in(const auto& val, const VALS_T&... test_vals)
{
    for (const auto& test_val : { test_vals... })
    {
        if (val == test_val)
        {
            return true;
        }
    }

    return false;
}