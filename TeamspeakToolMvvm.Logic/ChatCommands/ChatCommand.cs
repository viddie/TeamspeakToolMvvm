using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.Config;
using TeamspeakToolMvvm.Logic.Groups;
using TeamspeakToolMvvm.Logic.ViewModels;
using TSClient.Events;

namespace TeamspeakToolMvvm.Logic.ChatCommands {
    public abstract class ChatCommand {

        public MainViewModel Parent { get; set; }
        public MySettings Settings { get; set; }

        public abstract string CommandPrefix { get; set; }
        public abstract List<string> CommandAliases { get; set; }
        public abstract bool HasExceptionWhiteList { get; set; }



        public abstract void HandleCommand(NotifyTextMessageEvent evt, string command, List<string> parameters, Action<string> messageCallback);

        public bool CanExecute(string uniqueId, string command, List<string> parameters) {
            Type t = GetType();

            if (HasExceptionWhiteList) {
                if (AccessManager.UserHasAccessToCommand(uniqueId, t)) return true;
                return CanExecuteSubCommand(uniqueId, command, parameters);
            } else {
                if (!AccessManager.UserHasAccessToCommand(uniqueId, t)) return false;
                return CanExecuteSubCommand(uniqueId, command, parameters);
            }

        }
        public abstract bool CanExecuteSubCommand(string uniqueId, string command, List<string> parameters);
        public abstract bool IsValidCommandSyntax(string command, List<string> parameters);
        public abstract string GetUsageSyntax(string command, List<string> parameters);
        public abstract string GetUsageDescription(string command, List<string> parameters);
    }
}
