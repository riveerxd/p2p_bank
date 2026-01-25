using P2PBank.Logging.Subscribers;

namespace P2PBank.Logging;

public class Logger
{
    private HashSet<ILoggerSubscriber> _subscribers = new HashSet<ILoggerSubscriber>();
    private readonly object _lock = new object();

    public Logger()
    {
    }

    public void Log(string msg)
    {
        lock (_lock)
        {
            var disposed = new LinkedList<ILoggerSubscriber>();
            foreach (var subscriber in _subscribers)
            {
                if (subscriber.IsDisposed)
                {
                    disposed.AddLast(subscriber);
                }
                else
                {
                    subscriber.Log(msg);
                }
            }

            foreach (var subscriber in disposed)
            {
                _subscribers.Remove(subscriber);
            }
        }
    }

    public void LogInfo(string msg)
    {
        Log("[INFO] " + msg);
    }

    public void LogError(string msg)
    {
        Log("[ERROR] " + msg);
    }

    public void LogCommand(string clientIp, string cmd, string response)
    {
        // format: [CMD] ip -> command -> response
        Log("[CMD] " + clientIp + " -> " + cmd.Trim() + " -> " + response.Trim());
    }

    public void LogConnection(string clientIp, bool connected)
    {
        if (connected)
            Log("[CONN] Client connected: " + clientIp);
        else
            Log("[CONN] Client disconnected: " + clientIp);
    }

    public void Subscribe(ILoggerSubscriber subscriber)
    {
        _subscribers.Add(subscriber);
    }
}
