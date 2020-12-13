using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSClient.Events;

namespace TeamspeakToolMvvm.Logic.ChatCommands {
    public class WakemeupCommand : ChatCommand {
        public override string CommandPrefix { get; set; } = "wakemeup";
        public override List<string> CommandAliases { get; set; } = new List<string>() { "wmu" };
        public override bool HasExceptionWhiteList { get; set; } = true;

        public override bool IsValidCommandSyntax(string command, List<string> parameters) {
            return true;
        }

        public override string GetUsageSyntax(string command, List<string> parameters) {
            return $"{command}";
        }

        public override string GetUsageDescription(string command, List<string> parameters) {
            return $"Does something";
        }

        public override bool CanExecuteSubCommand(string uniqueId, string command, List<string> parameters) {
            return false;
        }

        public override void HandleCommand(NotifyTextMessageEvent evt, string command, List<string> parameters, Action<string> messageCallback) {
            string message = null;
            if (parameters.Count > 0) {
                message = string.Join(" ", parameters);
            }
            Parent.Client.PokeClient(evt.InvokerId, message);
        }
    }
}
