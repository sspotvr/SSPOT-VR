namespace SSPot.Scripts.DeveloperConsole.Commands
{
    public interface IConsoleCommand
    {
        string CommandWord { get; }
        string Description { get; }

        bool Process(string[] args);
    }
}
