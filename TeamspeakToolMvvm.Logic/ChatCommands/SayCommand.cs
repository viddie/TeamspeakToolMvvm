using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.Groups;
using TSClient.Events;

namespace TeamspeakToolMvvm.Logic.ChatCommands {
    public class SayCommand : ChatCommand {
        public override string CommandPrefix { get; set; } = "say";
        public override List<string> CommandAliases { get; set; } = new List<string>() { };


        public override bool IsValidCommandSyntax(string command, List<string> parameters) {
            return parameters.Count > 0;
        }

        public override string GetUsageHelp(string command, List<string> parameters) {
            return $"{command} <something...>";
        }

        public override bool CanExecuteSubCommand(string uniqueId, string command, List<string> parameters) {
            return false;
        }

        public override void HandleCommand(NotifyTextMessageEvent evt, string command, List<string> parameters, Action<string> messageCallback) {
            string toSay = string.Join(" ", parameters);
            messageCallback.Invoke($"[Bot]: {toSay}");
        }
    }
}
