using P2PBank.Services;

namespace P2PBank.Commands;

// remove/delete account
public class ARCommand : ICommand
{
    private BankService _bank;

    public ARCommand(BankService bank)
    {
        _bank = bank;
    }

    public string Execute(string[] args)
    {
        // AR account/ip
        if(args.Length < 2)
            return "ER Invalid format. Use: AR account/ip";

        var parts = args[1].Split('/');
        if(parts.Length != 2)
            return "ER Invalid account format. Use: account/ip";

        int accNum;
        if(!int.TryParse(parts[0], out accNum))
            return "ER Account number must be a number";

        if(accNum < 10000 || accNum > 99999)
            return "ER Account number must be between 10000 and 99999";

        string ip = parts[1];

        // cant delete accounts from other banks
        if(ip != _bank.GetBankCode())
            return "ER Cannot delete account from another bank";

        var res = _bank.RemoveAccount(accNum);
        if(res.success)
            return "AR";
        else
            return "ER " + res.error;
    }
}
