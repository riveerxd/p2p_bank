namespace P2PBank.Models;

// for storing info about other banks (network scan)
public class BankInfo
{
    public string IpAddress { get; set; } = "";
    public long TotalAmount { get; set; } = 0;
    public int ClientCount { get; set; } = 0;
}
