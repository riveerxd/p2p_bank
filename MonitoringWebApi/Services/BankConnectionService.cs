using System.Net.Sockets;

namespace MonitoringWebApi.Services;

public class BankConnectionService : IBankConnectionService
{
    private readonly string _host;
    private readonly int _port;

    private TcpClient? _tcpClient = null;

    public BankConnectionService(string host, int port)
    {
        _host = host;
        _port = port;
    }

    private void InitializeClient()
    {
        if (_tcpClient == null || !_tcpClient.Connected)
        {
            _tcpClient = new TcpClient();
            _tcpClient.Connect(_host, _port);
        }
    }

    public async Task ShutdownServerAsync()
    {
        if (_tcpClient == null)
        {
            InitializeClient();
        }
        if (_tcpClient == null)
        {
            throw new InvalidOperationException("TCP client could not be initialized.");
        }
        using var networkStream = _tcpClient.GetStream();
        using var writer = new StreamWriter(networkStream) { AutoFlush = true };

        await writer.WriteLineAsync("SHUTDOWN");
        networkStream.Close();
        _tcpClient.Close();
    }

    public async Task<StreamReader> GetLogStreamReader()
    {
        if (_tcpClient == null)
        {
            InitializeClient();
        }
        if (_tcpClient == null)
        {
            throw new InvalidOperationException("TCP client could not be initialized.");
        }
        using var networkStream = _tcpClient.GetStream();

        using var writer = new StreamWriter(networkStream) { AutoFlush = true };
        await writer.WriteLineAsync("LISTENER");
        
        return new StreamReader(networkStream);
    }
}