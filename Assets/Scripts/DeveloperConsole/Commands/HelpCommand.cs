using System.Collections.Generic;
using UnityEngine;

namespace SSPot.Scripts.DeveloperConsole.Commands
{
	[CreateAssetMenu(fileName = "HelpCommand", menuName = "DeveloperConsole/Commands/HelpCommand", order = 3)]

	public class HelpCommand : ConsoleCommand
    {
		private IEnumerable<IConsoleCommand> allCommands;

        public void Initialize(IEnumerable<IConsoleCommand> commands) => allCommands = commands;

		public override bool Process(string[] args)
		{
			// "help" sem argumentos - Lista todos
            if (args.Length == 0)
            {
                string helpText = "[Command] Available Commands:\n";
                foreach (var cmd in allCommands)
                {
                    helpText += $"- {cmd.CommandWord}\n";
                }
                helpText += "\nUse 'help <command>' for more details.";
                Debug.Log(helpText);
                return true;
            }

            // "help <command>" - Busca descrição específica
            string targetCommand = args[0];
            foreach (var cmd in allCommands)
            {
                if (cmd.CommandWord.Equals(targetCommand, System.StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Log($"[Command] <b>{cmd.CommandWord}</b>: {cmd.Description}");
                    return true;
                }
            }

            Debug.LogError($"Command '{targetCommand}' not found.");
            return false;
		}
	}
}
