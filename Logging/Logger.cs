namespace P2PBank.Logging;

public class Logger
{
    private string _logFile;

    public Logger(string logFile = "bank.log")
    {
        _logFile = logFile;
    }

    public void Log(string msg)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string line = "[" + timestamp + "] " + msg;

        Console.WriteLine(line);
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
}
