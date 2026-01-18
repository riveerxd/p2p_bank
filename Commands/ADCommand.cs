using P2PBank.Services;

namespace P2PBank.Commands;

// deposit money into account
public class ADCommand : ICommand
{
    private BankService _bank;

    public ADCommand(BankService bank)
    {
        _bank = bank;
    }

    public string Execute(string[] args)
    {
        // AD account/ip amount
        if(args.Length < 3)
            return "ER Invalid format. Use: AD account/ip amount";

        var parts = args[1].Split('/');
        if(parts.Length != 2)
            return "ER Invalid account format. Use: account/ip";

        int accNum;
        if(!int.TryParse(parts[0], out accNum))
            return "ER Account number must be a number";

        if(accNum < 10000 || accNum > 99999)
            return "ER Account number must be between 10000 and 99999";

        string ip = parts[1];

        long amount;
        if(!long.TryParse(args[2], out amount))
            return "ER Amount must be a number";

        if(amount <= 0)
            return "ER Amount must be positive";

        // TODO: proxy to other bank if ip doesnt match
        if(ip != _bank.GetBankCode())
            return "ER This bank does not handle accounts for " + ip;

        var res = _bank.Deposit(accNum, amount);
        if(res.success)
            return "AD";
        else
            return "ER " + res.error;
    }
}
