using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.Misc;
using TSClient.Events;

namespace TeamspeakToolMvvm.Logic.ChatCommands {
    public class ChooseCommand : ChatCommand {
        public override string CommandPrefix { get; set; } = "choose";
        public override List<string> CommandAliases { get; set; } = new List<string>() { };
        public override bool HasExceptionWhiteList { get; set; } = true;

        public override bool IsValidCommandSyntax(string command, List<string> parameters) {
            return parameters.Count >= 2;
        }

        public override string GetUsageSyntax(string command, List<string> parameters) {
            return $"{command} <value1> <value2> [value3]...";
        }

        public override string GetUsageDescription(string command, List<string> parameters) {
            return $"Chooses something for you";
        }

        public override bool CanExecuteSubCommand(string uniqueId, string command, List<string> parameters) {
            return false;
        }

        public override void HandleCommand(NotifyTextMessageEvent evt, string command, List<string> parameters, Action<string> messageCallback) {
            Random r = new Random();
            string chosenValue = parameters[r.Next(parameters.Count)];
            messageCallback.Invoke(ColorCoder.Bold($"Chosen was: '{chosenValue}'"));
        }
    }
}
