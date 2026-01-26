using MySqlConnector;
using P2PBank.Models;

namespace P2PBank.Data;

public class AccountRepository
{
    private readonly IDbConnectionFactory _db;

    public AccountRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public Account? GetAccount(int accNum)
    {
        using var conn = _db.GetConnection();
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT account_number, balance, created_at FROM accounts WHERE account_number = @num";
        cmd.Parameters.AddWithValue("@num", accNum);

        using var reader = cmd.ExecuteReader();
        if(reader.Read())
        {
            return new Account
            {
                AccountNumber = reader.GetInt32(0),
                Balance = reader.GetInt64(1),
                CreatedAt = reader.GetDateTime(2)
            };
        }

        return null;
    }

    public bool AccountExists(int accNum)
    {
        using var conn = _db.GetConnection();
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM accounts WHERE account_number = @num";
        cmd.Parameters.AddWithValue("@num", accNum);

        int cnt = Convert.ToInt32(cmd.ExecuteScalar());
        return cnt > 0;
    }

    public Account CreateAccount(int accNum)
    {
        using var conn = _db.GetConnection();
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO accounts (account_number, balance) VALUES (@num, 0)";
        cmd.Parameters.AddWithValue("@num", accNum);
        cmd.ExecuteNonQuery();

        return new Account { AccountNumber = accNum, Balance = 0, CreatedAt = DateTime.Now };
    }

    public void UpdateBalance(int accNum, long newBalance)
    {
        using var conn = _db.GetConnection();
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE accounts SET balance = @bal WHERE account_number = @num";
        cmd.Parameters.AddWithValue("@bal", newBalance);
        cmd.Parameters.AddWithValue("@num", accNum);
        cmd.ExecuteNonQuery();
    }

    public bool DeleteAccount(int accNum)
    {
        using var conn = _db.GetConnection();
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM accounts WHERE account_number = @num";
        cmd.Parameters.AddWithValue("@num", accNum);

        int rows = cmd.ExecuteNonQuery();
        return rows > 0;
    }

    public long GetTotalBalance()
    {
        using var conn = _db.GetConnection();
        conn.Open();

        var cmd = conn.CreateCommand();
        // COALESCE handles empty table case
        cmd.CommandText = "SELECT COALESCE(SUM(balance), 0) FROM accounts";

        var res = cmd.ExecuteScalar();
        return Convert.ToInt64(res);
    }

    public int GetAccountCount()
    {
        using var conn = _db.GetConnection();
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM accounts";

        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public List<Account> GetAllAccounts()
    {
        List<Account> accounts = new List<Account>();

        using var conn = _db.GetConnection();
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT account_number, balance, created_at FROM accounts";

        using var reader = cmd.ExecuteReader();
        while(reader.Read())
        {
            accounts.Add(new Account
            {
                AccountNumber = reader.GetInt32(0),
                Balance = reader.GetInt64(1),
                CreatedAt = reader.GetDateTime(2)
            });
        }

        return accounts;
    }

    // generates next account number
    public int GetNextAccountNumber()
    {
        using var conn = _db.GetConnection();
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT MAX(account_number) FROM accounts";

        var res = cmd.ExecuteScalar();
        if(res == DBNull.Value || res == null)
            return 10000; // first account starts at 10000

        return Convert.ToInt32(res) + 1;
    }
}
