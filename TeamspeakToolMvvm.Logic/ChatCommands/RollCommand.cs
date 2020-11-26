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
    public class RollCommand : ChatCommand {
        public override string CommandPrefix { get; set; } = "roll";
        public override List<string> CommandAliases { get; set; } = new List<string>() { };
        public override bool HasExceptionWhiteList { get; set; } = true;


        public override bool IsValidCommandSyntax(string command, List<string> parameters) {
            if (parameters.Count > 2) return false;

            if (parameters.Count >= 1 && !int.TryParse(parameters[0], out int res)) {
                throw new CommandParameterInvalidFormatException(1, parameters[0], parameters.Count == 1 ? "range" : "from", typeof(int), GetUsageSyntax(command, parameters));
            }

            if (parameters.Count == 2 && !int.TryParse(parameters[1], out int res2)) {
                throw new CommandParameterInvalidFormatException(2, parameters[1], "to", typeof(int), GetUsageSyntax(command, parameters));
            }

            return true;
        }

        public override string GetUsageSyntax(string command, List<string> parameters) {
            return $"{command} [range]\n\tor\t{command} [from] [to]";
        }

        public override string GetUsageDescription(string command, List<string> parameters) {
            return $"Rolls a dice";
        }

        public override bool CanExecuteSubCommand(string uniqueId, string command, List<string> parameters) {
            return false;
        }

        public override void HandleCommand(NotifyTextMessageEvent evt, string command, List<string> parameters, Action<string> messageCallback) {
            int lower = 1;
            int upper = 100;

            if (parameters.Count == 1) {
                upper = int.Parse(parameters[0]);
            } else if (parameters.Count == 2) {
                lower = int.Parse(parameters[0]);
                upper = int.Parse(parameters[1]);
            }

            int range = upper - lower;

            Random r = new Random();
            int rolled = lower + (r.Next(range+1));

            messageCallback.Invoke($"{ColorCoder.Username(evt.InvokerName)} rolled a '{ColorCoder.Bold(rolled.ToString())}'");
        }
    }
}
