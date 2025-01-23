namespace P2PBank.Logging;

public class Logger
{
    private static Logger? _instance;
    private string _logFile;
    private object _lock = new object();

    public Logger(string logFile = "bank.log")
    {
        _logFile = logFile;
        _instance = this;
    }

    // static versions for when you dont have the instance
    public static void Info(string msg) => _instance?.WriteLog("[INFO] " + msg);
    public static void Error(string msg) => _instance?.WriteLog("[ERROR] " + msg);

    private void WriteLog(string msg)
    {
        string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string line = "[" + time + "] " + msg;

        Console.WriteLine(line);

        lock(_lock)
        {
            try
            {
                File.AppendAllText(_logFile, line + "\n");
            }
            catch { }
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
        if(connected)
            WriteLog("[CONN] Connected: " + clientIp);
        else
            WriteLog("[CONN] Disconnected: " + clientIp);
    }
}
