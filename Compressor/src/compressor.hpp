#pragma once

#include <thread>
#include <string_view>

#include <poll.h>
#include <fcntl.h>
#include <sys/eventfd.h>

#include "logging.hpp"
#include "buffer.hpp"

enum class compress_status_t
{
    COMPLETE,
    MORE,
    ERROR
};

class compressor_t
{
private:
    std::string _last_error;

protected:
    void set_last_error(const std::exception& exc)
    {
        set_last_error(exc.what());
    }

    void set_last_error(std::string_view&& message)
    {
        logging::error(_last_error = message);
    }

protected:
    virtual bool init_internal() = 0;
    virtual compress_status_t compress_internal(const buffer_t& in, buffer_t& out) = 0;
    virtual compress_status_t cleanup_internal(buffer_t& out) = 0;

public:
    const char* get_last_error() const
    {
        return _last_error.c_str();
    }

    compress_status_t compress(char* src, int src_size, char* dst, int dst_size, int* comressed_size)
    {
        buffer_t out{ dst, dst_size };
        auto ret = compress_internal({ src, src_size }, out);

        *comressed_size = out.count;
        return ret;
    }

    bool init()
    {
        return init_internal();
    }

    compress_status_t cleanup(char* dst, int dst_size, int* comressed_size)
    {
        buffer_t out{ dst, dst_size };
        auto ret = cleanup_internal(out);

        *comressed_size = out.count;
        return ret;
    }

    virtual ~compressor_t()
    {

    }
};