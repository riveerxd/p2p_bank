using System.Net.Sockets;

namespace MonitoringWebApi.Services;

public class BankConnectionService : IConnectionService
{
    private readonly string _host;
    private readonly int _port;

    public BankConnectionService(string host, int port)
    {
        _host = host;
        _port = port;
    }

    public async Task ShutdownServerAsync()
    {
        var client = new TcpClient();
        await client.ConnectAsync(_host, _port);
        using var networkStream = client.GetStream();
        using var writer = new StreamWriter(networkStream) { AutoFlush = true };

        await writer.WriteLineAsync("SHUTDOWN");
        networkStream.Close();
        client.Close();
    }

    public async Task<StreamReader> GetLogStreamReader()
    {
        var client = new TcpClient();
        await client.ConnectAsync(_host, _port);
        using var networkStream = client.GetStream();

        using var writer = new StreamWriter(networkStream) { AutoFlush = true };
        await writer.WriteLineAsync("LISTENER");
        
        return new StreamReader(networkStream);
    }
}