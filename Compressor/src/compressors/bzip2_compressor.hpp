#pragma once

#include <bzlib.h>

#include "../utils.hpp"
#include "../compressor.hpp"
#include "zlib_compatible_compressor.hpp"

struct bzip2_options_t : zlib_compatible_compressor_options_t<int, int, bz_stream>
{
    bzip2_options_t() : zlib_compatible_compressor_options_t({ BZ_FINISH, BZ_RUN }) {}
};

class bzip2_compressor_t : public zlib_compatible_compressor_t<bzip2_options_t>
{
private:
    ENUM_TO_STR_METHOD(bz_code_to_str,
        ENUM_TO_STR(BZ_OK)
        ENUM_TO_STR(BZ_RUN_OK)
        ENUM_TO_STR(BZ_FLUSH_OK)
        ENUM_TO_STR(BZ_FINISH_OK)
        ENUM_TO_STR(BZ_STREAM_END)
        ENUM_TO_STR(BZ_SEQUENCE_ERROR)
        ENUM_TO_STR(BZ_PARAM_ERROR)
        ENUM_TO_STR(BZ_MEM_ERROR)
        ENUM_TO_STR(BZ_DATA_ERROR)
        ENUM_TO_STR(BZ_DATA_ERROR_MAGIC)
        ENUM_TO_STR(BZ_IO_ERROR)
        ENUM_TO_STR(BZ_UNEXPECTED_EOF)
        ENUM_TO_STR(BZ_OUTBUFF_FULL)
        ENUM_TO_STR(BZ_CONFIG_ERROR)
        ENUM_TO_STR_DEFAULT(MKSTR("Unknown code: " << arg).c_str())
    );

    void set_error_by_ret_code(int code)
    {
        set_last_error(bz_code_to_str(code));
    }

protected:
    bool do_init(bz_stream* stream)
    {
        auto ret = BZ2_bzCompressInit(stream, 9, 0, 30);
        if (ret != BZ_OK)
        {
            set_error_by_ret_code(ret);
            return false;
        }

        return true;
    }

    void do_cleanup(bz_stream* stream) { BZ2_bzCompressEnd(stream); }

    int deflate_once(bz_stream* stream, int flush_flag) { return BZ2_bzCompress(stream, flush_flag); }

    compress_status_t check_deflate(bz_stream* stream, int ret)
    {
        switch (ret)
        {
        case BZ_STREAM_END:
            return compress_status_t::COMPLETE;
        case BZ_RUN_OK:
            return stream->avail_in > 0
                ? compress_status_t::MORE
                : compress_status_t::COMPLETE;
        default:
            set_error_by_ret_code(ret);
            return compress_status_t::ERROR;
        }
    }

    compress_status_t check_flush(bz_stream* stream, int ret)
    {
        switch (ret)
        {
        case BZ_STREAM_END:
            return compress_status_t::COMPLETE;
        case BZ_FINISH_OK:
            return compress_status_t::MORE;
        default:
            set_error_by_ret_code(ret);
            return compress_status_t::ERROR;
        }
    }
};