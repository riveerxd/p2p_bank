using MySqlConnector;

namespace P2PBank.Data;

// copied from orm_kanban, modified a bit
public interface IDbConnectionFactory
{
    MySqlConnection GetConnection();
    string ConnStr { get; }
}

public class DbConnection : IDbConnectionFactory
{
    public string ConnStr { get; }

    public DbConnection(string connStr)
    {
        ConnStr = connStr;
    }

    // creates new connection each time
    public MySqlConnection GetConnection()
    {
        return new MySqlConnection(ConnStr);
    }

    // creates table if it doesnt exist
    public void InitializeDatabase()
    {
        using var conn = GetConnection();
        conn.Open();

        var cmd = conn.CreateCommand();
        // FIXME: should probably check if database exists first?
        cmd.CommandText = @"CREATE TABLE IF NOT EXISTS accounts (
            account_number INT PRIMARY KEY,
            balance BIGINT NOT NULL DEFAULT 0,
            created_at DATETIME DEFAULT CURRENT_TIMESTAMP
        )";

        cmd.ExecuteNonQuery();
    }
}
