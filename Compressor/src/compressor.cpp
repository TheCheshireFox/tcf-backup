#include "logging.hpp"
#include "compressor_type.hpp"
#include "compressors/gzip_compressor.hpp"
#include "compressors/bzip2_compressor.hpp"
#include "compressors/xz_compressor.hpp"

class error_compressor_t : public compressor_t
{
protected:
    bool init_internal()
    {
        return false;
    }

    compress_status_t compress_internal(const buffer_t& in, buffer_t& out)
    {
        return compress_status_t::ERROR;
    }

    compress_status_t cleanup_internal(buffer_t& out)
    {
        return compress_status_t::ERROR;
    }

public:
    error_compressor_t(const std::string& message)
    {
        set_last_error(message);
    }
};

extern "C" void set_logging(log_level_t level, log_method_callback_t method)
{
    logging::details::set(level, method);
}

extern "C" bool compressor_create(compressor_type_t type, compressor_t** p_compressor)
{
    compressor_t* compressor;
    switch (type)
    {
        case compressor_type_t::GZIP:
            *p_compressor = new gzip_compressor_t();
            break;
        case compressor_type_t::BZIP2:
            *p_compressor = new bzip2_compressor_t();
            break;
        case compressor_type_t::XZ:
            *p_compressor = new xz_compressor_t();
            break;
        default:
            *p_compressor = new error_compressor_t(MKSTR("Compressor with type " << type << " not found."));
            break;
    }

    return (*p_compressor)->init();
}

extern "C" compress_status_t compressor_compress(compressor_t* compressor, char* src, int src_size, char* dst, int dst_size, int* comressed_size)
{
    if (compressor == nullptr)
    {
        return compress_status_t::ERROR;
    }

    return compressor->compress(src, src_size, dst, dst_size, comressed_size);
}

extern "C" compress_status_t compressor_cleanup(compressor_t* compressor, char* dst, int dst_size, int* comressed_size)
{
    if (compressor == nullptr)
    {
        return compress_status_t::ERROR;
    }

    return compressor->cleanup(dst, dst_size, comressed_size);
}

extern "C" void compressor_destroy(compressor_t* compressor, char* dst, int dst_size, int* comressed_size)
{
    if (compressor == nullptr)
    {
        return;
    }

    delete compressor;
}

extern "C" const char* compressor_get_last_error(const compressor_t* compressor)
{
    return compressor->get_last_error();
}