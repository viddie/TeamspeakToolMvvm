using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.Exceptions;
using TeamspeakToolMvvm.Logic.Groups;
using TeamspeakToolMvvm.Logic.Misc;
using TSClient.Events;

namespace TeamspeakToolMvvm.Logic.ChatCommands {
    // !help
    // !help <command>
    // !help <command> <parameters>
    public class HelpCommand : ChatCommand {
        public override string CommandPrefix { get; set; } = "help";
        public override List<string> CommandAliases { get; set; } = new List<string>();
        public override bool HasExceptionWhiteList { get; set; } = true;

        public override bool CanExecuteSubCommand(string uniqueId, string command, List<string> parameters) {
            return false;
        }

        public override string GetUsageSyntax(string command, List<string> parameters) {
            return $"{command} [command] [parameters]";
        }

        public override void HandleCommand(NotifyTextMessageEvent evt, string command, List<string> parameters, Action<string> messageCallback) {

            if (parameters.Count == 0) {

                string toPrint = $"All commands are listed below ({ChatCommandHandler.ChatCommands.Count}):";
                foreach (ChatCommand cmd in ChatCommandHandler.ChatCommands.OrderBy((cmd) => cmd.CommandPrefix)) {
                    string commandHelp = $"\n{ColorCoder.Bold($"{Settings.ChatCommandPrefix}{cmd.CommandPrefix}")}:\t-\t{cmd.GetUsageDescription(cmd.CommandPrefix, parameters)}";

                    if (!cmd.CanExecute(evt.InvokerUniqueId, cmd.CommandPrefix, parameters)){
                        commandHelp = ColorCoder.ColorText(Color.LightRed, commandHelp);
                    }

                    toPrint += commandHelp;
                }

                messageCallback.Invoke(toPrint);

            } else {
                string requestedCommand = parameters[0];
                parameters = parameters.Skip(1).ToList();

                foreach (ChatCommand cmd in ChatCommandHandler.ChatCommands) {
                    if (cmd.CommandPrefix == requestedCommand || cmd.CommandAliases.Contains(requestedCommand)) {
                        ChatCommand chatCommand = (ChatCommand)Activator.CreateInstance(cmd.GetType());
                        chatCommand.Parent = Parent;
                        chatCommand.Settings = Settings;

                        try {
                            if (!chatCommand.CanExecute(evt.InvokerUniqueId, requestedCommand, parameters)) {
                                messageCallback.Invoke(ColorCoder.ErrorBright($"You don't have access to view the help of this command"));
                                break;
                            }
                            if (!chatCommand.IsValidCommandSyntax(requestedCommand, parameters)) {
                                //parameters = new List<string>();
                            }
                        } catch (CommandParameterInvalidFormatException) {}

                        string helpUsage = Settings.ChatCommandPrefix + chatCommand.GetUsageSyntax(requestedCommand, parameters);
                        string helpDescription = chatCommand.GetUsageDescription(requestedCommand, parameters);
                        string aliases = string.Join(", ", chatCommand.CommandAliases);
                        aliases = string.IsNullOrEmpty(aliases) ? "<None>" : aliases;

                        messageCallback.Invoke($"Help for command {ColorCoder.Bold($"'{chatCommand.CommandPrefix}'")}:\n\t{ColorCoder.Bold("Aliases:")} {aliases}\n\t{ColorCoder.Bold("Usage:")} {helpUsage}\n\t{ColorCoder.Bold("Description:")} {helpDescription}");
                        break;
                    }
                }
            }
        }

        public override bool IsValidCommandSyntax(string command, List<string> parameters) {
            return true;
        }

        public override string GetUsageDescription(string command, List<string> parameters) {
            return $"Get the usage description of commands";
        }
    }
}
