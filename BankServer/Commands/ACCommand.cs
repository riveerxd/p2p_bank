using P2PBank.Services;

namespace P2PBank.Commands;

// create new account
public class ACCommand : ICommand
{
    private BankService _bank;

    public ACCommand(BankService bank)
    {
        _bank = bank;
    }

    public string Execute(string[] args)
    {
        var res = _bank.CreateAccount();

        if(res.success)
            return "AC " + res.result;
        else
            return "ER " + res.result;
    }
}
