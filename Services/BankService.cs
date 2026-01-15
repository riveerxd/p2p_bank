using P2PBank.Data;
using P2PBank.Models;

namespace P2PBank.Services;

public class BankService
{
    private readonly AccountRepository _repo;
    private readonly string _bankCode;

    public BankService(AccountRepository repo, string bankCode)
    {
        _repo = repo;
        _bankCode = bankCode;
    }

    public string GetBankCode() => _bankCode;

    public (bool success, string result) CreateAccount()
    {
        try
        {
            int num = _repo.GetNextAccountNumber();

            if(num > 99999)
            {
                return (false, "Bank has reached maximum number of accounts");
            }

            var acc = _repo.CreateAccount(num);
            return (true, acc.AccountNumber + "/" + _bankCode);
        }
        catch(Exception ex)
        {
            return (false, "Failed to create account: " + ex.Message);
        }
    }

    public (bool success, string error) Deposit(int accNum, long amount)
    {
        if(amount <= 0)
            return (false, "Amount must be positive");

        var acc = _repo.GetAccount(accNum);
        if(acc == null)
            return (false, "Account not found");

        long newBal = acc.Balance + amount;
        _repo.UpdateBalance(accNum, newBal);
        return (true, "");
    }
}
