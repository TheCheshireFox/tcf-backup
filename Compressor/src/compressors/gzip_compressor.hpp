#pragma once

#include <zlib.h>

#include "../compressor.hpp"
#include "zlib_compatible_compressor.hpp"

struct gzip_options_t : zlib_compatible_compressor_options_t<int, int, z_stream>
{
    gzip_options_t() : zlib_compatible_compressor_options_t({ Z_FINISH, Z_NO_FLUSH }) {}
};

class gzip_compressor_t : public zlib_compatible_compressor_t<gzip_options_t>
{
private:
    ENUM_TO_STR_METHOD(gz_code_to_str,
        ENUM_TO_STR(Z_OK)
        ENUM_TO_STR(Z_STREAM_END)
        ENUM_TO_STR(Z_NEED_DICT)
        ENUM_TO_STR(Z_ERRNO)
        ENUM_TO_STR(Z_STREAM_ERROR)
        ENUM_TO_STR(Z_DATA_ERROR)
        ENUM_TO_STR(Z_MEM_ERROR)
        ENUM_TO_STR(Z_BUF_ERROR)
        ENUM_TO_STR(Z_VERSION_ERROR)
        ENUM_TO_STR_DEFAULT(MKSTR("Unknown code: " << arg).c_str())
    );

    void set_error_by_ret_code(int code)
    {
        set_last_error(gz_code_to_str(code));
    }

protected:
    bool do_init(z_stream* stream)
    {
        stream->zalloc = Z_NULL;
        stream->zfree = Z_NULL;
        stream->opaque = Z_NULL;

        auto ret = deflateInit2(stream, Z_DEFAULT_COMPRESSION, Z_DEFLATED, 15 | 16, 8, Z_DEFAULT_STRATEGY);
        if (ret != Z_OK)
        {
            set_error_by_ret_code(ret);
            return false;
        }

        return true;
    }

    void do_cleanup(z_stream* stream) { deflateEnd(stream); }

    int deflate_once(z_stream* stream, int flush_flag) { return ::deflate(stream, flush_flag); }

    compress_status_t check_deflate(z_stream* stream, int ret)
    {
        switch (ret)
        {
        case Z_STREAM_END:
            return compress_status_t::COMPLETE;
        case Z_OK:
            return stream->avail_in == 0
                ? compress_status_t::COMPLETE
                : compress_status_t::MORE;
        case Z_BUF_ERROR:
            return stream->avail_in == 0
                ? compress_status_t::COMPLETE
                : stream->avail_out == 0
                    ? compress_status_t::MORE
                    : compress_status_t::ERROR;
        default:
            set_error_by_ret_code(ret);
            return compress_status_t::ERROR;
        }
    }

    compress_status_t check_flush(z_stream* stream, int ret)
    {
        return check_deflate(stream, ret);
    }
};