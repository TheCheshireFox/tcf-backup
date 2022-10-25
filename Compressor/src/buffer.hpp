#pragma once

#include <array>

struct buffer_t
{
    char* data;
    int count;

    bool is_valid() const
    {
        return data != nullptr && count > 0;
    }
};