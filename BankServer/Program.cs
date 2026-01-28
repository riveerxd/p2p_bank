using P2PBank;
using P2PBank.Commands;
using P2PBank.Data;
using P2PBank.Logging;
using P2PBank.Logging.Subscribers;
using P2PBank.Server;
using P2PBank.Services;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== P2P Bank Node ===");
        Console.WriteLine();

        var config = Config.Load(args);

        Console.WriteLine("IP: " + config.IpAddress);
        Console.WriteLine("Port: " + config.Port);
        Console.WriteLine($"Timeout: {config.Timeout}ms");
        Console.WriteLine();

        var logger = new Logger();
        logger.Subscribe(new ConsoleLoggerSubscriber());
        logger.Subscribe(new FileLoggerSubscriber("bank.log"));
        logger.LogInfo("Starting P2P Bank Node...");

        // database setup
        DbConnection db;
        try
        {
            db = new DbConnection(config.GetConnectionString());
            db.InitializeDatabase();
            logger.LogInfo("Database initialized");
        }
        catch(Exception ex)
        {
            logger.LogError("Failed to connect to database: " + ex.Message);
            Console.WriteLine("ERROR: Could not connect to MySQL database!");
            Console.WriteLine("Make sure MySQL is running and credentials are correct.");
            Console.WriteLine("Connection: " + config.DbServer + ":" + config.DbPort + "/" + config.DbName);
            return;
        }

        var repo = new AccountRepository(db);
        var bankService = new BankService(repo, config.IpAddress, config);
        var parser = new CommandParser(bankService);
        var server = new TcpBankServer(config.Port, parser, logger, config.Timeout, config.PrivateKey);

        // ctrl+c handler
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            logger.LogInfo("Shutting down...");
            server.Stop();
        };

        try
        {
            server.Start();
        }
        catch (Exception ex)
        {
            logger.LogError("Server error: " + ex.Message);
        }

        Console.WriteLine("Server stopped.");
    }
}
