namespace P2PBank.Logging.Subscribers;

public class ConsoleLoggerSubscriber : ILoggerSubscriber
{
    public bool IsDisposed => false;

    public void Log(string message)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string line = "[" + timestamp + "] " + message;
        Console.WriteLine(line);
    }
}