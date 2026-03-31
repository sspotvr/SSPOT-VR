using UnityEngine;

namespace SSPot.Scripts.DeveloperConsole.Commands
{
	public abstract class ConsoleCommand : ScriptableObject, IConsoleCommand
	{
		[SerializeField] private string commandWord = string.Empty;
		[TextArea] [SerializeField] private string description;

		public string CommandWord => commandWord;
		public string Description => description;

		public abstract bool Process(string[] args);
	}
}
