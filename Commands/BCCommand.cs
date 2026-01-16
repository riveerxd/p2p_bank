using P2PBank.Services;

namespace P2PBank.Commands;

// returns bank code (ip address)
public class BCCommand : ICommand
{
    private BankService _bank;

    public BCCommand(BankService bank)
    {
        _bank = bank;
    }

    public string Execute(string[] args)
    {
        // simple one, just return our ip
        return "BC " + _bank.GetBankCode();
    }
}
