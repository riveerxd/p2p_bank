namespace P2PBank.Commands;

// all commands implement this
public interface ICommand
{
    string Execute(string[] args);
}
