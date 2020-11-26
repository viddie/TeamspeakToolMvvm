using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.Groups;
using TSClient.Events;

namespace TeamspeakToolMvvm.Logic.ChatCommands {
    public class TimeCommand : ChatCommand {
        public override string CommandPrefix { get; set; } = "time";
        public override List<string> CommandAliases { get; set; } = new List<string>() { "now" };
        public override bool HasExceptionWhiteList { get; set; } = true;

        public override bool IsValidCommandSyntax(string command, List<string> parameters) {
            return true;
        }

        public override string GetUsageSyntax(string command, List<string> parameters) {
            return "time";
        }

        public override string GetUsageDescription(string command, List<string> parameters) {
            return $"Checks the clock for you";
        }

        public override bool CanExecuteSubCommand(string uniqueId, string command, List<string> parameters) {
            return false;
        }

        public override void HandleCommand(NotifyTextMessageEvent evt, string command, List<string> parameters, Action<string> messageCallback) {
            string time = DateTime.Now.ToString();
            messageCallback.Invoke($"The current time is: {time}");
        }
    }
}
