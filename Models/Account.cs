namespace P2PBank.Models;

public class Account
{
    public int AccountNumber { get; set; }
    public long Balance { get; set; }
    public DateTime CreatedAt { get; set; }

    public Account()
    {
        // default values
        Balance = 0;
        CreatedAt = DateTime.Now;
    }
}
