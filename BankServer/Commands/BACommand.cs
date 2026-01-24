using P2PBank.Services;

namespace P2PBank.Commands;

// total money in bank
public class BACommand : ICommand
{
    private BankService _bank;

    public BACommand(BankService bank)
    {
        _bank = bank;
    }

    public string Execute(string[] args)
    {
        var total = _bank.GetTotalAmount();
        return "BA " + total.ToString();
    }
}
