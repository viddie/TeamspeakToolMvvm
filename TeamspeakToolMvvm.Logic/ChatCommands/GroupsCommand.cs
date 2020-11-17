using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSClient.Events;
using TSClient.Models;

namespace TeamspeakToolMvvm.Logic.ChatCommands {
    public class GroupsCommand : ChatCommand {
        public override string CommandPrefix { get; set; } = "groups";
        public override List<string> CommandAliases { get; set; } = new List<string>() {  };

        public override string GetUsageHelp(string command, List<string> parameters) {
            if (parameters.Count >= 1) {
                if (parameters[0] == "add") {
                    return $"{command} add <target_user> <groupName>";
                } else if (parameters[0] == "remove") {
                    return $"{command} remove <target_user> <groupName>";
                } else if (parameters[0] == "list") {
                    return $"{command} list <target_user>";
                }
            }
            return $"{command} <add|remove|list> <target_user> [groupName]";
        }

        public override bool IsValidCommandSyntax(string command, List<string> parameters) {
            if (parameters.Count < 2) return false;
            if (parameters[0] == "add" && parameters.Count != 3) return false;
            if (parameters[0] == "remove" && parameters.Count != 3) return false;
            if (parameters[0] == "list" && parameters.Count != 2) return false;

            return true;
        }

        public override void HandleCommand(NotifyTextMessageEvent evt, string command, List<string> parameters, Action<string> messageCallback) {
            string action = parameters[0];
            string targetUser = parameters[1];
            Client client = Parent.Client.GetClientByNamePart(targetUser);

            if (action == "add") {
                string groupName = parameters[2];

            } else if (action == "remove") {
                string groupName = parameters[2];

            } else if (action == "list") {

            }
        }
    }
}
