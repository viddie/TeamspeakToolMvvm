using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.Config;
using TeamspeakToolMvvm.Logic.Exceptions;
using TeamspeakToolMvvm.Logic.Groups;
using TeamspeakToolMvvm.Logic.Misc;
using TSClient.Events;

namespace TeamspeakToolMvvm.Logic.ChatCommands {
    public class TickCommand : ChatCommand {
        public override string CommandPrefix { get; set; } = "tick";
        public override List<string> CommandAliases { get; set; } = new List<string>() { };
        public override bool HasExceptionWhiteList { get; set; } = true;

        public int RollCount;

        public override bool IsValidCommandSyntax(string command, List<string> parameters) {
            return true;
        }

        public override string GetUsageSyntax(string command, List<string> parameters) {
            return command;
        }

        public override string GetUsageDescription(string command, List<string> parameters) {
            TimeSpan untilTick = GetTimeSpanUntilTick();
            return $"Shows the time until the next economy tick is executed ({Utils.FormatTimeSpanShort(untilTick)})";
        }

        public override bool CanExecuteSubCommand(string uniqueId, string command, List<string> parameters) {
            return false;
        }

        public TimeSpan GetTimeSpanUntilTick() {
            return (MySettings.Instance.EcoLastTick + TimeSpan.FromSeconds(MySettings.Instance.EcoTickTimeSeconds)) - DateTime.Now;
        }

        public override void HandleCommand(NotifyTextMessageEvent evt, string command, List<string> parameters, Action<string> messageCallback) {
            TimeSpan untilTick = GetTimeSpanUntilTick();
            messageCallback.Invoke($"Time until next economy tick ({ColorCoder.SuccessDim($"+{ColorCoder.Currency(Settings.EcoTickGain, Settings.EcoPointUnitName)}")}): {ColorCoder.Bold(Utils.FormatTimeSpanShort(untilTick))}");
        }
    }
}
