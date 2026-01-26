using System.Net.Sockets;

namespace MonitoringWebApi.Stream;

public class TcpClientStream : IDisposable
{
    private readonly TcpClient _tcpClient;
    private readonly StreamReader _reader;

    public TcpClientStream(TcpClient tcpClient)
    {
        _tcpClient = tcpClient;
        _reader = new StreamReader(_tcpClient.GetStream());
    }

    public StreamReader GetStream()
    {
        return _reader;
    }

    public bool CanRead => _tcpClient.Connected && _tcpClient.GetStream().CanRead;

    public void Dispose()
    {
        _reader.Dispose();
        try
        {
            _tcpClient.GetStream().Close();
        }
        catch
        {
            
        }
        _tcpClient.Close();
    }
}