using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;
using MonitoringWebApi.Stream;

namespace MonitoringWebApi.Services.BankConnection;

public class BankConnectionService : IBankConnectionService
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _privateKey;

    public BankConnectionService(string host, int port, string privateKey)
    {
        _host = host;
        _port = port;
        _privateKey = privateKey;
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
        using var reader = new StreamReader(networkStream);
        using var writer = new StreamWriter(networkStream) { AutoFlush = true };

        await writer.WriteLineAsync("SHUTDOWN");
        
        await HandleChallenge(reader, writer);
        
        networkStream.Close();
        tcpClient.Close();
    }

    public async Task<TcpClientStream> GetLogStreamReader()
    {
        var tcpClient = await InitializeClient();
        var networkStream = tcpClient.GetStream();

        var reader = new StreamReader(networkStream);
        var writer = new StreamWriter(networkStream) { AutoFlush = true };
        await writer.WriteLineAsync("LISTENER");
        
        await HandleChallenge(reader, writer);

        return new TcpClientStream(tcpClient);
    }

    private async Task HandleChallenge(StreamReader reader, StreamWriter writer)
    {
        var challenge = await reader.ReadLineAsync();
        if (challenge != null)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_privateKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(challenge));
            var response = Convert.ToBase64String(hash);
            await writer.WriteLineAsync(response);
        }
    }
}