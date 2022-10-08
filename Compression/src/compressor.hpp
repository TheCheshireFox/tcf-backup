#pragma once

#include <thread>
#include <utility>
#include <sstream>
#include <string_view>

#include <poll.h>
#include <fcntl.h>
#include <sys/eventfd.h>

#define MKSTR(x) (std::stringstream() << x).str()

class compressor_t
{
private:
    class filehandle_t
    {
    private:
        int _fd;

    public:
        filehandle_t(int fd) : _fd(fd)
        {

        }

        filehandle_t(filehandle_t&& other) : _fd(std::exchange(other._fd, -1))
        {

        }

        ~filehandle_t()
        {
            if (_fd != -1)
            {
                close(_fd);
            }
        }

        operator int() const
        {
            return _fd;
        }

        bool is_valid() const
        {
            return _fd != -1;
        }
    };

private:
    constexpr static int BUFFER_SIZE = 16 * 1024;

private:
    std::string _last_error;
    std::thread _poll_thread;
    int _stop_eventfd;

private:
    void compress_from_fd(int src_fd, int dst_fd)
    {
        char buffer[BUFFER_SIZE];

        int count = 0;
        while ((count = ::read(src_fd, buffer, BUFFER_SIZE)) > 0)
        {
            if (!compress(buffer, count, dst_fd))
            {
                // TODO: indecate fail
            }
        }
    }

protected:
    void set_last_error(const std::exception& exc)
    {
        _last_error = exc.what();
    }

    void set_last_error(const std::string_view& message)
    {
        _last_error = message;
    }

    void write_all(int fd, char* data, int len)
    {
        int count = 0;
        while ((count = ::write(fd, data, len)) != -1)
        {
            data += count;
            if ((len -= count) == 0)
            {
                break;
            }
        }

        if (count == -1)
        {
            throw std::runtime_error("Unable to write data to stream");
        }
    }

protected:
    virtual bool init() = 0;
    virtual bool compress(char* data, int len, int fd) = 0;
    virtual void do_cleanup() = 0;

public:
    const char* get_last_error() const
    {
        return _last_error.c_str();
    }

    bool init(char* src_file, char* dst_file)
    {
        if (!init())
        {
            set_last_error("Initialization failed");
            return false;
        }

        filehandle_t src(open(src_file, O_RDONLY | O_NONBLOCK));
        filehandle_t dst(open(dst_file, O_WRONLY | O_NONBLOCK));

        if (!src.is_valid())
        {
            auto err = errno;
            set_last_error(MKSTR("Unable to open " << src_file << ": " << err));
            return false;
        }

        if (!dst.is_valid())
        {
            auto err = errno;
            set_last_error(MKSTR("Unable to open " << dst_file << ": " << err));
            return false;
        }

        _stop_eventfd = eventfd(0, 0);
        _poll_thread = std::thread([this, src = std::move(src), dst = std::move(dst)]()
            {
                pollfd pfds[2] = { { src, POLLIN, 0 }, { _stop_eventfd, POLLIN, 1 } };

                while (true)
                {
                    auto ret = poll(pfds, 2, -1);
                    switch (ret)
                    {
                    case -1: // error
                        // TODO: indecate error
                        return;
                    default:
                        if (pfds[0].revents & POLLIN)
                        {
                            compress_from_fd(src, dst);
                        }
                        if (pfds[1].revents & POLLIN)
                        {
                            compress_from_fd(src, dst);
                            return;
                        }
                    }
                }
            });

        return true;
    }

    void cleanup()
    {
        eventfd_write(_stop_eventfd, 1);
        try
        {
            if (_poll_thread.joinable())
            {
                _poll_thread.join();
            }
        }
        catch (const std::exception&)
        {
            // NOP
        }

        do_cleanup();
    }

    virtual ~compressor_t()
    {

    }
};