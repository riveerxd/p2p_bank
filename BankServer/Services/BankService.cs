using P2PBank.Data;
using P2PBank.Models;

namespace P2PBank.Services;

public class BankService
{
    private readonly AccountRepository _repo;
    private readonly string _bankCode;
    private readonly Config _config;
    private object _lock = new object(); // for thread safety

    public BankService(AccountRepository repo, string bankCode, Config config)
    {
        _repo = repo;
        _bankCode = bankCode;
        _config = config;
    }

    public string GetBankCode() => _bankCode;
    public Config GetConfig() => _config;

    public (bool success, string result) CreateAccount()
    {
        lock(_lock)
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
    }

    public (bool success, string error) Deposit(int accNum, long amount)
    {
        if(amount <= 0)
            return (false, "Amount must be positive");

        lock(_lock)
        {
            var acc = _repo.GetAccount(accNum);
            if(acc == null)
                return (false, "Account not found");

            // overflow check
            if(acc.Balance > long.MaxValue - amount)
                return (false, "Deposit would cause overflow");

            long newBal = acc.Balance + amount;
            _repo.UpdateBalance(accNum, newBal);
            return (true, "");
        }
    }

    public (bool success, string error) Withdraw(int accNum, long amount)
    {
        if(amount <= 0)
            return (false, "Amount must be positive");

        lock(_lock)
        {
            var acc = _repo.GetAccount(accNum);
            if(acc == null)
                return (false, "Account not found");

            if(acc.Balance < amount)
                return (false, "Insufficient funds");

            _repo.UpdateBalance(accNum, acc.Balance - amount);
            return (true, "");
        }
    }

    public (bool success, long balance, string error) GetBalance(int accNum)
    {
        var acc = _repo.GetAccount(accNum);
        if(acc == null)
            return (false, 0, "Account not found");

        return (true, acc.Balance, "");
    }

    public (bool success, string error) RemoveAccount(int accNum)
    {
        lock(_lock)
        {
            var acc = _repo.GetAccount(accNum);
            if(acc == null)
                return (false, "Account not found");

            // cant delete if theres still money
            if(acc.Balance != 0)
                return (false, "Cannot delete account with non-zero balance");

            _repo.DeleteAccount(accNum);
            return (true, "");
        }
    }

    public long GetTotalAmount()
    {
        return _repo.GetTotalBalance();
    }

    public int GetClientCount()
    {
        return _repo.GetAccountCount();
    }

    public bool AccountExists(int accNum)
    {
        return _repo.AccountExists(accNum);
    }
}
