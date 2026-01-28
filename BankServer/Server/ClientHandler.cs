using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;
using Compression;
using P2PBank.Commands;
using P2PBank.Logging;
using P2PBank.Logging.Subscribers;

namespace P2PBank.Server;

public class ClientHandler
{
    private TcpClient _client;
    private CommandParser _parser;
    private Logger _logger;
    private int _timeout;
    private string _privateKey;

    private TcpBankServer _server;

    public ClientHandler(TcpClient client, CommandParser parser, Logger logger, int timeout, TcpBankServer server, string privateKey)
    {
        _client = client;
        _parser = parser;
        _server = server;
        _logger = logger;
        _timeout = timeout;
        _privateKey = privateKey;
    }

    public async Task Handle(CancellationToken cancellationToken = default)
    {
        string clientIp = _client.Client.RemoteEndPoint?.ToString() ?? "unknown";
        _logger.LogConnection(clientIp, true);

        try
        {
            //_client.ReceiveTimeout = _timeout;
            //_client.SendTimeout = _timeout;

            var stream = _client.GetStream();
            var reader = new StreamReader(stream, Encoding.UTF8);
            var writer = new StreamWriter(stream, Encoding.UTF8);
            writer.AutoFlush = true;

            while (_client.Connected)
            {
                string? line = null;

                try
                {
                    var task = reader.ReadLineAsync();
                    bool isComplete = task.Wait(_timeout, cancellationToken);
                    if (!isComplete)
                        throw new IOException("Read timeout");
                    line = task.Result;
                }
                catch (IOException)
                {
                    // timeout probably
                    break;
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                if (line == null)
                    break;

                // Not ideal!
                bool specialCommand = false;

                if (line.Trim() == "LISTENER")
                {
                    specialCommand = true;
                    if (PerformChallenge(reader, writer))
                    {
                        _timeout = Timeout.Infinite;
                        //_client.ReceiveTimeout = _timeout;
                        //_client.SendTimeout = _timeout;
                        _logger.LogInfo("Listener connected: " + clientIp);
                        _logger.Subscribe(new CompressedStreamLoggerSubscriber(writer, new ZstdCompressor(), _privateKey));
                    }
                    else
                    {
                        _logger.LogError("Listener authentication failed: " + clientIp);
                        break;
                    }
                }
                else if (line.Trim() == "SHUTDOWN")
                {
                    specialCommand = true;
                    if (PerformChallenge(reader, writer))
                    {
                        _logger.LogInfo("Shutdown command received from: " + clientIp);
                        _server.Stop();
                    }
                    else
                    {
                        _logger.LogError("Shutdown authentication failed: " + clientIp);
                        break;
                    }
                }

                // parse and execute command
                if (!specialCommand)
                {
                    string response = _parser.Parse(line);
                    _logger.LogCommand(clientIp, line, response);

                    writer.WriteLine(response);
                }
            }

            // cleanup
            reader.Close();
            writer.Close();
            stream.Close();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error handling client " + clientIp + ": " + ex.Message);
        }
        finally
        {
            _logger.LogConnection(clientIp, false);
            _client.Close();
        }
    }

    private bool PerformChallenge(StreamReader reader, StreamWriter writer)
    {
        try
        {
            var challenge = Guid.NewGuid().ToString();
            writer.WriteLine(challenge);

            var responseTask = reader.ReadLineAsync();
            if (!responseTask.Wait(_timeout)) return false;
            var response = responseTask.Result;

            if (response == null) return false;

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_privateKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(challenge));
                var expectedResponse = Convert.ToBase64String(hash);
                return response == expectedResponse;
            }
        }
        catch
        {
            return false;
        }
    }
}
