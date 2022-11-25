using System;
using System.IO;
using TcfBackup.Managers;

namespace TcfBackup.Factory;

public class TarCompressionManagerCompressorFactory : ITarCompressionManagerCompressorFactory
{
    private readonly Func<Stream, Stream> _factory;

    public TarCompressionManagerCompressorFactory(Func<Stream, Stream> factory) => _factory = factory;
    public Stream Create(Stream output) => _factory(output);
}