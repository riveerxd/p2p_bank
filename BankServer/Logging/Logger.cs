using P2PBank.Logging.Subscribers;

namespace P2PBank.Logging;

public class Logger
{
    private HashSet<ILoggerSubscriber> _subscribers = new HashSet<ILoggerSubscriber>();
    private readonly object _lock = new object();

    public Logger()
    {
    }

    private void WriteLog(string msg)
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

    // instance methods
    public void Log(string msg) => WriteLog(msg);
    public void LogInfo(string msg) => WriteLog("[INFO] " + msg);
    public void LogError(string msg) => WriteLog("[ERROR] " + msg);

    public void LogCommand(string clientIp, string cmd, string response)
    {
        WriteLog("[CMD] " + clientIp + " -> " + cmd.Trim() + " -> " + response.Trim());
    }

    public void LogConnection(string clientIp, bool connected)
    {
        if (connected)
            Log("[CONN] Client connected: " + clientIp);
        else
            WriteLog("[CONN] Disconnected: " + clientIp);
    }

    public void Subscribe(ILoggerSubscriber subscriber)
    {
        _subscribers.Add(subscriber);
    }
}
