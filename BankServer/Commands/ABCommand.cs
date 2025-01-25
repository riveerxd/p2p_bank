using P2PBank.Services;
using P2PBank.Network;

namespace P2PBank.Commands;

// get account balance
public class ABCommand : ICommand
{
    private BankService _bank;

    public ABCommand(BankService bank)
    {
        _bank = bank;
    }

    public string Execute(string[] args)
    {
        // AB account/ip
        if(args.Length < 2)
            return "ER Invalid format. Use: AB account/ip";

        var parts = args[1].Split('/');
        if(parts.Length != 2)
            return "ER Invalid account format. Use: account/ip";

        int accNum;
        if(!int.TryParse(parts[0], out accNum))
            return "ER Account number must be a number";

        if(accNum < 10000 || accNum > 99999)
            return "ER Account number must be between 10000 and 99999";

        string ip = parts[1];

        // proxy to other bank
        if(ip != _bank.GetBankCode())
        {
            var cfg = _bank.GetConfig();
            string cmd = "AB " + accNum + "/" + ip;
            return BankClient.SendCommand(ip, cfg.RemotePort, cmd, cfg.Timeout);
        }

        var res = _bank.GetBalance(accNum);
        if(res.success)
            return "AB " + res.balance;
        else
            return "ER " + res.error;
    }
}
