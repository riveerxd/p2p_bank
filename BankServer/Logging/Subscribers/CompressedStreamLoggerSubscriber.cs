using System.Text;
using Compression;

namespace P2PBank.Logging.Subscribers;

public class CompressedStreamLoggerSubscriber : ILoggerSubscriber
{
    private StreamWriter _writer;
    private bool _disposed = false;

    private ICompressor _compressor;

    public CompressedStreamLoggerSubscriber(StreamWriter stream, ICompressor compressor)
    {
        _writer = stream;
        _writer.AutoFlush = true;
        _compressor = compressor;
    }

    public bool IsDisposed => _disposed || _writer == null || _writer.BaseStream == null || !_writer.BaseStream.CanWrite;

    public void Log(string message)
    {
        try
        {
            var compressedMessage = _compressor.Compress(Encoding.UTF8.GetBytes(message));
            var base64Message = Convert.ToBase64String(compressedMessage);
            _writer.WriteLine(base64Message);
        }
        catch (IOException)
        {
            _disposed = true;
        }
    }
}