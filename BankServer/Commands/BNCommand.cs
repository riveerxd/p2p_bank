using P2PBank.Services;

namespace P2PBank.Commands;

// number of accounts/clients
public class BNCommand : ICommand
{
    private BankService _bank;

    public BNCommand(BankService bank)
    {
        _bank = bank;
    }

    public string Execute(string[] args)
    {
        int cnt = _bank.GetClientCount();
        return "BN " + cnt;
    }
}
