using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.Config;
using TeamspeakToolMvvm.Logic.Groups;
using TeamspeakToolMvvm.Logic.Misc;
using TSClient.Events;

namespace TeamspeakToolMvvm.Logic.ChatCommands {

    //!commands <add|remove|list>
    //!commands add <name> <some really long text>
    //!commands remove <name>
    //!commands list

    public class DynamicCommand : ChatCommand {
        public override string CommandPrefix { get; set; } = "commands";
        public override List<string> CommandAliases {
            get {
                return MySettings.Instance.DynamicCommands.Keys.ToList();
            }
            set { }
        }

        public override bool HasExceptionWhiteList { get; set; } = true;



        public override bool IsValidCommandSyntax(string command, List<string> parameters) {
            if (command != CommandPrefix) return true;

            if (parameters.Count == 0) return false;
            if (parameters[0] == "add" && parameters.Count < 3) return false;
            if (parameters[0] == "remove" && parameters.Count != 2) return false;
            if (parameters[0] == "list" && parameters.Count != 1) return false;

            return true;
        }

        public override string GetUsageSyntax(string command, List<string> parameters) {
            if (parameters.Count >= 1) {
                if (parameters[0] == "add") {
                    return $"{command} add <name> <some> [text]...";
                } else if (parameters[0] == "remove") {
                    return $"{command} remove <name>";
                } else if (parameters[0] == "list") {
                    return $"{command} list";
                }
            }
            return $"{command} <add|remove|list> [<name>] [groupName]";
        }

        public override string GetUsageDescription(string command, List<string> parameters) {
            if (parameters.Count >= 1) {
                if (parameters[0] == "add") {
                    return $"Adds a dynamic command with the given text";
                } else if (parameters[0] == "remove") {
                    return $"Removes a dynamic command";
                } else if (parameters[0] == "list") {
                    return $"Lists all dynamic commands";
                }
            }
            return $"Manage dynamic commands";
        }

        public override bool CanExecuteSubCommand(string uniqueId, string command, List<string> parameters) {
            if (parameters.Count > 0 && parameters[0] == "list" && AccessManager.UserHasAccessToSubCommand(uniqueId, "command:commands_list")) return true;
            if (command != CommandPrefix) return true;
            return false;
        }

        public override void HandleCommand(NotifyTextMessageEvent evt, string command, List<string> parameters, Action<string> messageCallback) {
            if (command != CommandPrefix) {
                if (!Settings.DynamicCommands.ContainsKey(command.ToLower())) {
                    messageCallback.Invoke(ColorCoder.ErrorBright($"The command {command} was not found in the dynamic commands list (should never happen)"));
                }

                string parametersJoined = string.Join(" ", parameters);
                string toPrint = Settings.DynamicCommands[command.ToLower()];
                toPrint = toPrint.Replace("{params}", parametersJoined);
                messageCallback.Invoke(toPrint);
                return;
            }

            string action = parameters[0];
            string commandName = "";
            string commandText = "";

            if (action == "add") {
                commandName = parameters[1];
                commandText = string.Join(" ", parameters.Skip(2));
            } else if (action == "remove") {
                commandName = parameters[1];
            }

            commandName = commandName.ToLower();


            if (action == "add") {
                if (Settings.DynamicCommands.ContainsKey(commandName)) {
                    messageCallback.Invoke(ColorCoder.ErrorBright($"The command {ColorCoder.Bold($"'{commandName}'")} already exists."));
                    return;
                }

                Settings.DynamicCommands.Add(commandName, commandText);
                messageCallback.Invoke(ColorCoder.Success($"Added command {ColorCoder.Bold($"'{commandName}'")}."));
                Settings.DelayedSave();

            } else if (action == "remove") {
                if (!Settings.DynamicCommands.ContainsKey(commandName)) {
                    messageCallback.Invoke(ColorCoder.ErrorBright($"The command {ColorCoder.Bold($"'{commandName}'")} does not exists."));
                    return;
                }

                Settings.DynamicCommands.Remove(commandName);
                messageCallback.Invoke(ColorCoder.Success($"Removed command {ColorCoder.Bold($"'{commandName}'")}."));
                Settings.DelayedSave();

            } else if (action == "list") {
                if (Settings.DynamicCommands.Count == 0) {
                    messageCallback.Invoke($"The dynamic commands list is currently empty.");
                    return;
                }


                string toPrint = $"These are all registered commands ({Settings.DynamicCommands.Count}):";
                foreach (string dynCommand in Settings.DynamicCommands.Keys.OrderBy(s => s)) {
                    string dynCommandText = Settings.DynamicCommands[dynCommand];
                    toPrint += $"\n\t{ColorCoder.Bold($"'{dynCommand}'")}: {dynCommandText}";
                }

                messageCallback.Invoke(toPrint);
            }
        }
    }
}
