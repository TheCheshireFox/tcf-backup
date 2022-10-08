#pragma once

#include <lzma.h>

#include "compressor.hpp"

class xz_compressor_t : public compressor_t
{
private:
    constexpr static int BUFFER_SIZE = 16384;

private:
    lzma_stream _stream;

    unsigned char _buffer[BUFFER_SIZE];

private:
    template<typename... Codes>
    bool check_ret_code(lzma_ret code, Codes... good_codes)
    {
        for (const auto good_code: {good_codes...})
        {
            if (code == good_code)
            {
                return true;
            }
        }

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

        return false;
    }

private:
    lzma_ret do_compress(int fd, lzma_action action)
    {
        _stream.avail_out = BUFFER_SIZE;
        _stream.next_out = _buffer;

        auto ret = lzma_code(&_stream, action);
        if (ret != LZMA_STREAM_END && ret != LZMA_OK)
        {
            return ret;
        }

        write_all(fd, (char*)_buffer, BUFFER_SIZE - _stream.avail_out);

        return ret;
    }

protected:
    bool init()
    {
        auto preset = LZMA_PRESET_DEFAULT;
        
        auto ret = lzma_easy_encoder(&_stream, preset, LZMA_CHECK_CRC64);

        return check_ret_code(ret, LZMA_OK);
    }

    void do_cleanup()
    {
        lzma_end(&_stream);
    }

    bool compress(char* data, int len, int fd)
    {
        _stream.avail_in = len;
        _stream.next_in = (uint8_t*)data;

        while (true)
        {
            if (_stream.avail_in == 0)
            {
                auto ret = do_compress(fd, LZMA_FINISH);
                return check_ret_code(ret, LZMA_OK, LZMA_STREAM_END);
            }

            auto ret = do_compress(fd, LZMA_RUN);
            if (ret != LZMA_STREAM_END && !check_ret_code(ret, LZMA_OK))
            {
                return false;
            }
        }

        return true;
    }
};