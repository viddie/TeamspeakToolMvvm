using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.Messages;
using TeamspeakToolMvvm.Logic.Misc;
using TSClient.Events;

namespace TeamspeakToolMvvm.Logic.ChatCommands {
    public class StopCommand : ChatCommand {
        public override string CommandPrefix { get; set; } = "stop";
        public override List<string> CommandAliases { get; set; } = new List<string>();
        public override bool HasExceptionWhiteList { get; set; } = true;

        public override bool CanExecuteSubCommand(string uniqueId, string command, List<string> parameters) {
            return false;
        }

        public override string GetUsageDescription(string command, List<string> parameters) {
            return "Stops the plugin";
        }

        public override string GetUsageSyntax(string command, List<string> parameters) {
            return $"{command} [message]";
        }

        public override bool IsValidCommandSyntax(string command, List<string> parameters) {
            return true;
        }

        public override void HandleCommand(NotifyTextMessageEvent evt, string command, List<string> parameters, Action<string> messageCallback) {
            messageCallback.Invoke(ColorCoder.ErrorBright("Plugin instance was killed!"));
            Messenger.Default.Send(new StopApplicationMessage());
        }
    }
}
