#pragma once

#include <functional>

#include <zlib.h>

#include "../compressor.hpp"

template<typename FLUSH_FLAG_T, typename RET_TYPE_T, typename STREAM_T>
struct zlib_compatible_compressor_options_t
{
    typedef FLUSH_FLAG_T flush_flag_t;
    typedef RET_TYPE_T return_type_t;
    typedef STREAM_T stream_t;

    FLUSH_FLAG_T flush;
    FLUSH_FLAG_T no_flush;
};

template<typename OPTIONS_T>
class zlib_compatible_compressor_t : public compressor_t
{
private:
    typedef typename OPTIONS_T::flush_flag_t FLUSH_FLAG_T;
    typedef typename OPTIONS_T::return_type_t RET_TYPE_T;
    typedef typename OPTIONS_T::stream_t STREAM_T;

private:
    STREAM_T _stream;
    OPTIONS_T _options;

private:
    compress_status_t deflate(buffer_t& out)
    {
        return check_deflate(&_stream, do_deflate(out, _options.no_flush));
    }

    compress_status_t flush(buffer_t& out)
    {
        return check_flush(&_stream, do_deflate(out, _options.flush));
    }

    RET_TYPE_T do_deflate(buffer_t& out, FLUSH_FLAG_T flush_flag)
    {
        _stream.avail_out = out.count;
        _stream.next_out = (decltype(_stream.next_out))out.data;

        auto ret = deflate_once(&_stream, flush_flag);
        
        out.count -= _stream.avail_out;

        return ret;
    }

protected:
    virtual bool do_init(STREAM_T* stream) = 0;
    virtual void do_cleanup(STREAM_T* stream) = 0;
    virtual RET_TYPE_T deflate_once(STREAM_T* stream, FLUSH_FLAG_T flush_flag) = 0;
    virtual compress_status_t check_deflate(STREAM_T* stream, RET_TYPE_T ret) = 0;
    virtual compress_status_t check_flush(STREAM_T* stream, RET_TYPE_T ret) = 0;

protected:
    bool init_internal()
    {
        return do_init(&_stream);
    }

    compress_status_t cleanup_internal(buffer_t& out)
    {
        if (out.data != nullptr && out.count != 0)
        {
            auto ret = flush(out);
            if (ret != compress_status_t::COMPLETE)
            {
                return ret;
            }
        }

        do_cleanup(&_stream);
        return compress_status_t::COMPLETE;
    }

    compress_status_t compress_internal(const buffer_t& in, buffer_t& out)
    {
        if (in.is_valid())
        {
            _stream.avail_in = in.count;
            _stream.next_in = (decltype(_stream.next_in))in.data;
        }

        try
        {
            return deflate(out);
        }
        catch (const std::exception& exc)
        {
            set_last_error(exc);
            return compress_status_t::ERROR;
        }
    }
};