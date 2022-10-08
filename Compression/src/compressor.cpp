#include "compressor_type.hpp"
#include "gzip_compressor.hpp"
#include "xz_compressor.hpp"

class error_compressor_t : public compressor_t
{
protected:
    bool init()
    {
        return false;
    }

    bool compress(char* data, int len, int fd)
    {
        return false;
    }

    void do_cleanup()
    {

    }

public:
    error_compressor_t(const std::string_view& message)
    {
        set_last_error(message);
    }
};

extern "C" bool compressor_create(compressor_type_t type, char* src_file, char* dst_file, compressor_t** p_compressor)
{
    compressor_t* compressor;
    switch (type)
    {
        case compressor_type_t::GZIP:
            *p_compressor = new gzip_compressor_t();
            break;
        case compressor_type_t::XZ:
            *p_compressor = new xz_compressor_t();
            break;
        default:
            *p_compressor = new error_compressor_t(MKSTR("Compressor with type " << type << " not found."));
            break;
    }

    return (*p_compressor)->init(src_file, dst_file);
}

extern "C" void compressor_destroy(compressor_t* compressor)
{
    if (compressor == nullptr)
    {
        return;
    }

    compressor->cleanup();
    delete compressor;
}

extern "C" const char* compressor_get_last_error(const compressor_t* compressor)
{
    return compressor->get_last_error();
}