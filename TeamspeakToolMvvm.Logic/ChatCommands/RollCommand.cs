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


        public override bool IsValidCommandSyntax(string command, List<string> parameters) {
            if (parameters.Count > 2) return false;

            if (parameters.Count >= 1 && !int.TryParse(parameters[0], out int res)) {
                throw new CommandParameterInvalidFormatException() { ParameterPosition = 1, ParameterName = parameters.Count == 1 ? "range" : "from", ParameterType = typeof(int), ParameterValue = parameters[0] };
            }

            if (parameters.Count == 2 && !int.TryParse(parameters[1], out int res2)) {
                throw new CommandParameterInvalidFormatException() { ParameterPosition = 2, ParameterName = "to", ParameterType = typeof(int), ParameterValue = parameters[1] };
            }

            return true;
        }

        public override string GetUsageHelp(string command, List<string> parameters) {
            return $"{command} [range]\n\tor\t{command} [from] [to]";
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
