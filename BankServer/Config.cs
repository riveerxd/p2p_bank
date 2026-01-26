using DotNetEnv;

namespace P2PBank;

public class Config
{
    public int Port { get; set; } = 65525;
    public int Timeout { get; set; } = 5000;
    public string IpAddress { get; set; } = "";
    public int RemotePort { get; set; } = 65525; // port for connecting to other banks

    // database config
    public string DbServer { get; set; } = "localhost";
    public int DbPort { get; set; } = 3306;
    public string DbName { get; set; } = "p2p_bank";
    public string DbUser { get; set; } = "root";
    public string DbPassword { get; set; } = "";

    public string GetConnectionString()
    {
        // TODO: maybe add pooling options later?
        return "Server=" + DbServer + ";Port=" + DbPort + ";Database=" + DbName + ";User=" + DbUser + ";Password=" + DbPassword + ";";
    }

    public static Config Load(string[] args)
    {
        // load .env file if it exists
        if (File.Exists(".env"))
        {
            Env.Load();
        }

        Config config = new Config();

        // load from environment variables first (from .env or system)
        config.Port = GetEnvInt("BANK_PORT", config.Port);
        config.Timeout = GetEnvInt("BANK_TIMEOUT", config.Timeout);
        config.IpAddress = GetEnvString("BANK_IP", config.IpAddress);
        config.RemotePort = GetEnvInt("BANK_REMOTE_PORT", config.RemotePort);
        config.DbServer = GetEnvString("DB_SERVER", config.DbServer);
        config.DbPort = GetEnvInt("DB_PORT", config.DbPort);
        config.DbName = GetEnvString("DB_NAME", config.DbName);
        config.DbUser = GetEnvString("DB_USER", config.DbUser);
        config.DbPassword = GetEnvString("DB_PASSWORD", config.DbPassword);

        // command line args override everything
        for(int i = 0; i < args.Length; i++)
        {
            if(args[i] == "--port" && i + 1 < args.Length)
            {
                int p;
                if(int.TryParse(args[i+1], out p))
                    config.Port = p;
            }
            else if(args[i] == "--timeout" && i + 1 < args.Length)
            {
                int t;
                if(int.TryParse(args[i+1], out t))
                    config.Timeout = t;
            }
            else if (args[i] == "--ip" && i + 1 < args.Length)
            {
                config.IpAddress = args[i + 1];
            }
            else if(args[i] == "--db-server" && i+1 < args.Length)
            {
                config.DbServer = args[i + 1];
            }
            else if(args[i] == "--db-port" && i + 1 < args.Length)
            {
                int dp;
                if (int.TryParse(args[i + 1], out dp))
                    config.DbPort = dp;
            }
            else if (args[i] == "--db-name" && i + 1 < args.Length)
            {
                config.DbName = args[i + 1];
            }
            else if (args[i] == "--db-user" && i + 1 < args.Length)
            {
                config.DbUser = args[i+1];
            }
            else if(args[i] == "--db-pass" && i + 1 < args.Length)
            {
                config.DbPassword = args[i + 1];
            }
            else if(args[i] == "--remote-port" && i + 1 < args.Length)
            {
                int rp;
                if(int.TryParse(args[i + 1], out rp))
                    config.RemotePort = rp;
            }
        }

        // get IP if not provided
        if(string.IsNullOrEmpty(config.IpAddress))
        {
            config.IpAddress = GetLocalIp();
        }

        return config;
    }

    static string GetEnvString(string key, string defaultVal)
    {
        var val = Environment.GetEnvironmentVariable(key);
        return string.IsNullOrEmpty(val) ? defaultVal : val;
    }

    static int GetEnvInt(string key, int defaultVal)
    {
        var val = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrEmpty(val)) return defaultVal;

        int result;
        if (int.TryParse(val, out result))
            return result;
        return defaultVal;
    }

    static string GetLocalIp()
    {
        // idk if this is the best way to get local ip but it works
        try
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach(var ip in host.AddressList)
            {
                if(ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
        }
        catch { }

        return "127.0.0.1";
    }
}
