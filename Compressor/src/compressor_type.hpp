#pragma once

#include <ostream>

#include "utils.hpp"

enum class compressor_type_t
{
    GZIP = 0,
    BZIP2,
    XZ
};

ENUM_TO_STR_METHOD(compressor_type_to_str,
    CENUM_TO_STR(compressor_type_t, GZIP)
    CENUM_TO_STR(compressor_type_t, BZIP2)
    CENUM_TO_STR(compressor_type_t, XZ)
    ENUM_TO_STR_DEFAULT(MKSTR("Unknown compressor type: " << (int)arg).c_str())
)

std::ostream& operator<<(std::ostream& os, const compressor_type_t& type)
{
    return os << compressor_type_to_str(type);
}