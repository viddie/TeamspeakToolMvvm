using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSClient.Events;

namespace TeamspeakToolMvvm.Logic.ChatCommands {
    public class TimeCommand : ChatCommand {
        public override string CommandPrefix { get; set; } = "time";
        public override List<string> CommandAliases { get; set; } = new List<string>() {
            "now",
        };

        public override void HandleCommand(NotifyTextMessageEvent evt, string command, List<string> parameters, Action<string> messageCallback) {
            string time = DateTime.Now.ToString();
            messageCallback.Invoke($"The current time is: {time}");
        }

        public override bool IsValidCommandSyntax(string command, List<string> parameters) {
            return true;
        }

        public override string GetUsageHelp(string command, List<string> parameters) {
            return "time";
        }
    }
}
