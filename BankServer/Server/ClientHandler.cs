using System.Net.Sockets;
using System.Text;
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

    private TcpBankServer _server;

    public ClientHandler(TcpClient client, CommandParser parser, Logger logger, int timeout, TcpBankServer server)
    {
        _client = client;
        _parser = parser;
        _server = server;
        _logger = logger;
        _timeout = timeout;
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
                    _timeout = Timeout.Infinite;
                    //_client.ReceiveTimeout = _timeout;
                    //_client.SendTimeout = _timeout;
                    _logger.LogInfo("Listener connected: " + clientIp);
                    _logger.Subscribe(new CompressedStreamLoggerSubscriber(writer, new ZstdCompressor()));
                }
                else if (line.Trim() == "SHUTDOWN")
                {
                    specialCommand = true;
                    _logger.LogInfo("Shutdown command received from: " + clientIp);
                    _server.Stop();
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
}
