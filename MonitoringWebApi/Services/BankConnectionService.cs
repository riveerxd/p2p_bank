using System.Net.Sockets;
using MonitoringWebApi.Stream;

namespace MonitoringWebApi.Services;

public class BankConnectionService : IBankConnectionService
{
    private readonly string _host;
    private readonly int _port;

    public BankConnectionService(string host, int port)
    {
        _host = host;
        _port = port;
    }

    private async Task<TcpClient> InitializeClient()
    {
        var tcpClient = new TcpClient();
        if (tcpClient == null || !tcpClient.Connected)
        {
            tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(_host, _port);
        }
        return tcpClient;
    }

    public async Task ShutdownServerAsync()
    {
        var tcpClient = await InitializeClient();
        using var networkStream = tcpClient.GetStream();
        using var writer = new StreamWriter(networkStream) { AutoFlush = true };

        await writer.WriteLineAsync("SHUTDOWN");
        networkStream.Close();
        tcpClient.Close();
    }

    public async Task<TcpClientStream> GetLogStreamReader()
    {
        var tcpClient = await InitializeClient();
        var networkStream = tcpClient.GetStream();

        var writer = new StreamWriter(networkStream) { AutoFlush = true };
        await writer.WriteLineAsync("LISTENER");

        return new TcpClientStream(tcpClient);
    }
}