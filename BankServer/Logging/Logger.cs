using P2PBank.Logging.Subscribers;

namespace P2PBank.Logging;

public class Logger
{
    private List<ILoggerSubscriber> _subscribers = new List<ILoggerSubscriber>();

    public Logger()
    {
    }

    public void Log(string msg)
    {
        foreach (var subscriber in _subscribers)
        {
            subscriber.Log(msg);
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
        if(connected)
            Log("[CONN] Client connected: " + clientIp);
        else
            Log("[CONN] Client disconnected: " + clientIp);
    }

    public void Subscribe(ILoggerSubscriber subscriber)
    {
        _subscribers.Add(subscriber);
    }
}
