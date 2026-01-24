namespace P2PBank.Logging.Subscribers;

public class FileLoggerSubscriber : ILoggerSubscriber
{
    private string _logFile;
    private object _lock = new object();

    public FileLoggerSubscriber(string logFile = "bank.log")
    {
        _logFile = logFile;
    }

    public bool IsDisposed => false;

    public void Log(string message)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string line = "[" + timestamp + "] " + message;
        try
        {
            File.AppendAllText(_logFile, line + "\n");
        }
        catch
        {
            
        }
    }
}