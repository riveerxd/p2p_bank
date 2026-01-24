namespace P2PBank.Logging.Subscribers;

public class StreamLoggerSubscriber : ILoggerSubscriber
{
    private StreamWriter _writer;

    public StreamLoggerSubscriber(StreamWriter stream)
    {
        _writer = stream;
        _writer.AutoFlush = true;
    }

    public void Log(string message)
    {
        if (_writer.BaseStream != null && _writer.BaseStream.CanWrite) _writer.WriteLine(message);
    }
}