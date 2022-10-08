#pragma once

#include <ostream>

enum class compressor_type_t
{
    GZIP = 0,
    BZIP2,
    XZ
};

#define COMPRESSOR_TYPE_OS(type)\
case compressor_type_t::type: \
    return os << #type;

std::ostream& operator<<(std::ostream& os, const compressor_type_t& type)
{
    switch (type)
    {
        COMPRESSOR_TYPE_OS(GZIP)
        COMPRESSOR_TYPE_OS(BZIP2)
        COMPRESSOR_TYPE_OS(XZ)
        default:
            return os << "Unknown compressor type (" << (int)type << ")";
    }
}