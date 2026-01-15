using MySqlConnector;

namespace P2PBank.Data;

public class DbConnection
{
    private string _connStr;

    public DbConnection(string connectionString)
    {
        _connStr = connectionString;
    }

    public MySqlConnection GetConnection()
    {
        return new MySqlConnection(_connStr);
    }

    // creates table if it doesnt exist
    public void InitializeDatabase()
    {
        using var conn = GetConnection();
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"CREATE TABLE IF NOT EXISTS accounts (
            account_number INT PRIMARY KEY,
            balance BIGINT NOT NULL DEFAULT 0,
            created_at DATETIME DEFAULT CURRENT_TIMESTAMP
        )";

        cmd.ExecuteNonQuery();
    }
}
