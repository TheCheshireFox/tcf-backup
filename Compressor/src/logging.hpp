#pragma once

#include <string_view>

#include "utils.hpp"

enum class log_level_t
{
    DEBUG,
    INFO,
    WARN,
    ERROR
};

#define LOG_METHOD(method_name, lvl) void method_name(const std::string_view&& message)\
{\
    if (details::g_log_method != nullptr && details::g_log_level <= lvl)\
    {\
        details::g_log_method(lvl, message.data());\
    }\
}

using log_method_callback_t = void (*)(log_level_t log_level, const char* message);

namespace logging
{
    namespace details
    {
        log_level_t g_log_level = log_level_t::INFO;
        log_method_callback_t g_log_method = nullptr;

        void set(log_level_t level, log_method_callback_t method)
        {
            g_log_level = level;
            g_log_method = method;
        }
    }

    LOG_METHOD(debug, log_level_t::DEBUG);
    LOG_METHOD(info, log_level_t::INFO);
    LOG_METHOD(warning, log_level_t::WARN);
    LOG_METHOD(error, log_level_t::ERROR);
}