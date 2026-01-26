using System.Net;
using System.Net.Sockets;
using P2PBank.Commands;
using P2PBank.Logging;

namespace P2PBank.Server;

public class TcpBankServer
{
    private int _port;
    private CommandParser _parser;
    private Logger _logger;
    private int _timeout;
    private TcpListener? _listener;
    private bool _running = false;
    private CancellationTokenSource? _cts;

    private Action _onShutdown = () => {};

    public TcpBankServer(int port, CommandParser parser, Logger logger, int timeout)
    {
        _port = port;
        _parser = parser;
        _logger = logger;
        _timeout = timeout;
    }

    public void Start()
    {
        _listener = new TcpListener(IPAddress.Any, _port);
        _listener.Start();
        _running = true;

        _logger.LogInfo("Bank server started on port " + _port);

        // main accept loop
        _cts = new CancellationTokenSource();
        while(_running)
        {
            try
            {
                TcpClient client = _listener.AcceptTcpClient();

                // new thread for each client - assignment says real parallelism
                Thread t = new Thread(async () =>
                {
                    var handler = new ClientHandler(client, _parser, _logger, _timeout, this);
                    await handler.Handle(_cts.Token);
                });
                t.Start();
            }
            catch(SocketException)
            {
                // this happens when we stop the listener, its fine
                if(!_running) break;
            }
            catch(Exception ex)
            {
                _logger.LogError("Error accepting client: " + ex.Message);
            }
        }
    }

    public void Stop()
    {
        _running = false;
        _listener?.Stop();
        _logger.LogInfo("Bank server stopped");
        _cts?.Cancel();
        _cts = null;
    }
}
