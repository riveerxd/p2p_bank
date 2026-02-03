namespace P2PBank.Logging.Subscribers;

public class StreamLoggerSubscriber : ILoggerSubscriber
{
    private StreamWriter _writer;
    private bool _disposed = false;

    public StreamLoggerSubscriber(StreamWriter stream)
    {
        _writer = stream;
        _writer.AutoFlush = true;
    }

    public bool IsDisposed => _disposed || _writer == null || _writer.BaseStream == null || !_writer.BaseStream.CanWrite;

    public void Log(string message)
    {
        try
        {
            _writer.WriteLine(message);
        }
        catch (IOException)
        {
            _disposed = true;
        }
        catch (ObjectDisposedException)
        {
            _disposed = true;
        }
    }
}