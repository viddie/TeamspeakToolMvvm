using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.Groups;
using TeamspeakToolMvvm.Logic.Misc;
using TSClient.Events;
using TSClient.Models;

namespace TeamspeakToolMvvm.Logic.ChatCommands {
    public class GroupsCommand : ChatCommand {
        public override string CommandPrefix { get; set; } = "groups";
        public override List<string> CommandAliases { get; set; } = new List<string>() {  };

        public override bool HasExceptionWhiteList { get; set; } = true;

        public override bool IsValidCommandSyntax(string command, List<string> parameters) {
            if (parameters.Count < 2) return false;
            if (parameters[0] == "add" && parameters.Count != 3) return false;
            if (parameters[0] == "remove" && parameters.Count != 3) return false;
            if (parameters[0] == "list" && parameters.Count != 2) return false;

            return true;
        }

        public override string GetUsageSyntax(string command, List<string> parameters) {
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

        public override string GetUsageDescription(string command, List<string> parameters) {
            if (parameters.Count >= 1) {
                if (parameters[0] == "add") {
                    return $"Adds a user to a group";
                } else if (parameters[0] == "remove") {
                    return $"Removes a user from a group";
                } else if (parameters[0] == "list") {
                    return $"Lists all groups of a user";
                }
            }
            return $"Manage user groups";
        }

        public override bool CanExecuteSubCommand(string uniqueId, string command, List<string> parameters) {
            if (parameters.Count > 0 && parameters[0] == "list" && AccessManager.UserHasAccessToSubCommand(uniqueId, "command:groups_list")) return true;
            return false;
        }


        public override void HandleCommand(NotifyTextMessageEvent evt, string command, List<string> parameters, Action<string> messageCallback) {
            string action = parameters[0];
            string targetUser = parameters[1];
            Client target = Parent.Client.GetClientByNamePart(targetUser);
            Group group = null;

            if (action == "add" || action == "remove") {
                group = AccessManager.GetGroupByName(parameters[2]);
                if (group == null) {
                    messageCallback.Invoke(ColorCoder.Error($"The group named '{ColorCoder.Bold(parameters[2])}' was not found"));
                    return;
                }
            }

            if (action == "add") {
                bool success = AccessManager.AddUserGroup(target.UniqueId, group);
                if (success) {
                    messageCallback.Invoke(ColorCoder.Success($"{ColorCoder.Username(target.Nickname)} was added to the group '{ColorCoder.Bold(group.DisplayName)}'"));
                } else {
                    messageCallback.Invoke(ColorCoder.Error($"{ColorCoder.Username(target.Nickname)} is already in the group '{ColorCoder.Bold(group.DisplayName)}'"));
                }

            } else if (action == "remove") {
                bool success = AccessManager.RemoveUserGroup(target.UniqueId, group);
                if (success) {
                    messageCallback.Invoke(ColorCoder.Success($"{ColorCoder.Username(target.Nickname)} was removed from the group '{ColorCoder.Bold(group.DisplayName)}'"));
                } else {
                    messageCallback.Invoke(ColorCoder.Error($"{ColorCoder.Username(target.Nickname)} was not in the group '{ColorCoder.Bold(group.DisplayName)}'"));
                }

            } else if (action == "list") {
                List<Group> groups = AccessManager.GetUserGroups(target.UniqueId);
                string toPrint = $"{ColorCoder.Username(target.Nickname)} has the following groups:";
                foreach (Group printGroup in groups) {
                    toPrint += $"\n\t- {printGroup.DisplayName}";
                }
                messageCallback.Invoke(toPrint);
            }
        }
    }
}
