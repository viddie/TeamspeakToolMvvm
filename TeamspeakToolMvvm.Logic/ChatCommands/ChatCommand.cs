using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.Config;
using TeamspeakToolMvvm.Logic.ViewModels;
using TSClient.Events;

namespace TeamspeakToolMvvm.Logic.ChatCommands {
    public abstract class ChatCommand {

        public MainViewModel Parent { get; set; }
        public MySettings Settings { get; set; }

        public abstract string CommandPrefix { get; set; }
        public abstract List<string> CommandAliases { get; set; }



        public abstract void HandleCommand(NotifyTextMessageEvent evt, string command, List<string> parameters, Action<string> messageCallback); 
        public abstract bool IsValidCommandSyntax(string command, List<string> parameters);
        public abstract string GetUsageHelp(string command, List<string> parameters);
    }
}
