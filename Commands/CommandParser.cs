using P2PBank.Services;

namespace P2PBank.Commands;

public class CommandParser
{
    private Dictionary<string, ICommand> _commands;

    public CommandParser(BankService bank)
    {
        // register all commands
        _commands = new Dictionary<string, ICommand>
        {
            { "BC", new BCCommand(bank) },
            { "AC", new ACCommand(bank) },
            { "AD", new ADCommand(bank) },
            { "AW", new AWCommand(bank) },
            { "AB", new ABCommand(bank) },
            { "AR", new ARCommand(bank) },
            { "BA", new BACommand(bank) },
            { "BN", new BNCommand(bank) }
        };
    }

    public string Parse(string input)
    {
        if(string.IsNullOrWhiteSpace(input))
            return "ER Empty command";

        string[] parts = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if(parts.Length == 0)
            return "ER Empty command";

        string cmd = parts[0].ToUpper();

        if(!_commands.ContainsKey(cmd))
            return "ER Unknown command: " + cmd;

        try
        {
            return _commands[cmd].Execute(parts);
        }
        catch(Exception ex)
        {
            // shouldnt happen but just in case
            return "ER Internal error: " + ex.Message;
        }
    }
}
