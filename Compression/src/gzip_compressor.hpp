#pragma once

#include <zlib.h>

#include "compressor.hpp"

class gzip_compressor_t : public compressor_t
{
private:
    constexpr static int BUFFER_SIZE = 16384;

private:
    z_stream _stream;

    unsigned char _buffer[BUFFER_SIZE];

protected:
    bool init()
    {
        _stream.zalloc = Z_NULL;
        _stream.zfree = Z_NULL;
        _stream.opaque = Z_NULL;

        auto ret = deflateInit2(&_stream, Z_DEFAULT_COMPRESSION, Z_DEFLATED, 15 | 16, 8, Z_DEFAULT_STRATEGY);
        if (ret != Z_OK)
        {
            return false;
        }

        return true;
    }

    void do_cleanup()
    {
        deflateEnd(&_stream);
    }

    bool compress(char* data, int len, int fd)
    {
        _stream.avail_in = len;
        _stream.next_in = (unsigned char*)data;

        try
        {
            deflate(fd);
            flush(fd);

            if (_stream.avail_in != 0)
            {
                throw std::runtime_error("Not all data was compressed");
            }
        }
        catch (const std::exception& exc)
        {
            set_last_error(exc);
            return false;
        }

        return true;
    }

private:
    void deflate(int fd)
    {
        do
        {
            do_deflate(fd, Z_NO_FLUSH);
        }  while (_stream.avail_out == 0);
    }

    void flush(int fd)
    {
        while (do_deflate(fd, Z_FINISH) != Z_STREAM_END)
        {

        }
    }

    int do_deflate(int fd, int flush_flag)
    {
        _stream.avail_out = BUFFER_SIZE;
        _stream.next_out = _buffer;

        auto ret = ::deflate(&_stream, flush_flag);
        if (ret == Z_STREAM_ERROR)
        {
            throw std::runtime_error("Unable to compress (flush) data. Error code: " + std::to_string(ret));
        }

        write_all(fd, (char*)_buffer, BUFFER_SIZE - _stream.avail_out);

        return ret;
    }
};