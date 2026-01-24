using P2PBank.Services;

namespace P2PBank.Commands;

public class CommandParser
{
    private Dictionary<string, ICommand> _commands;

    public CommandParser(BankService bank)
    {
        // register all commands
        _commands = new Dictionary<string, ICommand>();
        _commands.Add("BC", new BCCommand(bank));
        _commands.Add("AC", new ACCommand(bank));
        _commands.Add("AD", new ADCommand(bank));
        _commands.Add("AW", new AWCommand(bank));
        _commands.Add("AB", new ABCommand(bank));
        _commands.Add("AR", new ARCommand(bank));
        _commands.Add("BA", new BACommand(bank));
        _commands.Add("BN", new BNCommand(bank));
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
