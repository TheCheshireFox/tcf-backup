#pragma once

#include <lzma.h>

#include "../compressor.hpp"
#include "zlib_compatible_compressor.hpp"

struct xz_options_t : zlib_compatible_compressor_options_t<lzma_action, lzma_ret, lzma_stream>
{
    xz_options_t() : zlib_compatible_compressor_options_t({ LZMA_FINISH, LZMA_RUN }) {}
};

class xz_compressor_t : public zlib_compatible_compressor_t<xz_options_t>
{
private:
    void set_error_by_ret_code(lzma_ret code)
    {
        switch (code)
        {
        case LZMA_OK:
            set_last_error("OK");
            break;
        case LZMA_STREAM_END:
            set_last_error("EOF");
            break;
        case LZMA_NO_CHECK:
            set_last_error("Input stream has no integrity check");
            break;
        case LZMA_UNSUPPORTED_CHECK:
            set_last_error("Cannot calculate the integrity check");
            break;
        case LZMA_GET_CHECK:
            set_last_error("Integrity check type is now available");
            break;
        case LZMA_MEM_ERROR:
            set_last_error("Cannot allocate memory");
            break;
        case LZMA_MEMLIMIT_ERROR:
            set_last_error("Memory usage limit was reached");
            break;
        case LZMA_FORMAT_ERROR:
            set_last_error("File format not recognized");
            break;
        case LZMA_OPTIONS_ERROR:
            set_last_error("Invalid or unsupported options");
            break;
        case LZMA_DATA_ERROR:
            set_last_error("Data is corrupt");
            break;
        case LZMA_BUF_ERROR:
            set_last_error("No progress is possible");
            break;
        case LZMA_PROG_ERROR:
            set_last_error("Programming error");
            break;
        }
    }

protected:
    bool do_init(lzma_stream* stream)
    {
        auto preset = LZMA_PRESET_DEFAULT;

        auto ret = lzma_easy_encoder(stream, preset, LZMA_CHECK_CRC64);
        if (ret != LZMA_OK)
        {
            set_error_by_ret_code(ret);
        }

        return ret == LZMA_OK;
    }

    void do_cleanup(lzma_stream* stream) { lzma_end(stream); }

    lzma_ret deflate_once(lzma_stream* stream, lzma_action flush_flag) { return lzma_code(stream, flush_flag); }

    compress_status_t check_deflate(lzma_stream* stream, lzma_ret ret)
    {
        switch (ret)
        {
        case LZMA_STREAM_END:
            return compress_status_t::COMPLETE;
        case LZMA_OK:
            return compress_status_t::MORE;
        case LZMA_BUF_ERROR:
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

    compress_status_t check_flush(lzma_stream* stream, lzma_ret ret)
    {
        return check_deflate(stream, ret);
    }
};