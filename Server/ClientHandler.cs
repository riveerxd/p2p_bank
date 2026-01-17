using System.Net.Sockets;
using System.Text;
using P2PBank.Commands;
using P2PBank.Logging;

namespace P2PBank.Server;

public class ClientHandler
{
    private TcpClient _client;
    private CommandParser _parser;
    private Logger _logger;
    private int _timeout;

    public ClientHandler(TcpClient client, CommandParser parser, Logger logger, int timeout)
    {
        _client = client;
        _parser = parser;
        _logger = logger;
        _timeout = timeout;
    }

    public void Handle()
    {
        string clientIp = _client.Client.RemoteEndPoint?.ToString() ?? "unknown";
        _logger.LogConnection(clientIp, true);

        try
        {
            var stream = _client.GetStream();
            var reader = new StreamReader(stream, Encoding.UTF8);
            var writer = new StreamWriter(stream, Encoding.UTF8);
            writer.AutoFlush = true;

            while(_client.Connected)
            {
                string? line = null;

                try
                {
                    line = reader.ReadLine();
                }
                catch(IOException)
                {
                    // timeout probably
                    break;
                }

                if(line == null)
                    break;

                // parse and execute command
                string response = _parser.Parse(line);
                _logger.LogCommand(clientIp, line, response);

                writer.WriteLine(response);
            }

            // cleanup
            reader.Close();
            writer.Close();
            stream.Close();
        }
        catch(Exception ex)
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
