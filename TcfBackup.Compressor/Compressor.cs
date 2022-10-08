namespace TcfBackup.Compressor;

public class Compressor : IDisposable
{
    private readonly bool _ownOutput;
    private IntPtr _pCompressor;

    public Stream Input { get; }
    public Stream Output { get; }

    public Compressor(CompressorType compressorType)
        : this(compressorType, FifoFactory.MkFifo(), true)
    {

    }

    public Compressor(CompressorType compressorType, FileStream output)
        : this(compressorType, output, false)
    {

    }

    private Compressor(CompressorType compressorType, FileStream output, bool ownOutput)
    {
        _ownOutput = ownOutput;
        Output = output;
        
        try
        {
            Input = FifoFactory.MkFifo();

            if (!CompressorNative.Create(compressorType, ((FileStream)Input).Name, output.Name, out _pCompressor))
            {
                var error = CompressorNative.GetLastError(_pCompressor);
                CompressorNative.Destroy(_pCompressor);
                throw new Exception(error);
            }

        }
        catch (Exception)
        {
            Cleanup();
            throw;
        }
    }

    private void Cleanup()
    {
        if (_pCompressor != IntPtr.Zero)
        {
            CompressorNative.Destroy(_pCompressor);
        }
            
        Input?.Dispose();
        if (_ownOutput)
        {
            Output.Dispose();
        }
    }
    
    protected virtual void Dispose(bool disposing)
    {
        Cleanup();
        _pCompressor = IntPtr.Zero;
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Compressor()
    {
        Dispose(false);
    }
}