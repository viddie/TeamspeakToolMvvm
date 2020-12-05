using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.Misc;
using TSClient.Events;
using TSClient.Models;

namespace TeamspeakToolMvvm.Logic.ChatCommands {
    public class ResetCooldown : ChatCommand {
        public override string CommandPrefix { get; set; } = "resetcooldown";
        public override List<string> CommandAliases { get; set; } = new List<string>() { };
        public override bool HasExceptionWhiteList { get; set; } = true;

        public string TargetNamePart;

        public override bool IsValidCommandSyntax(string command, List<string> parameters) {
            if (parameters.Count == 0) return true;
            TargetNamePart = string.Join(" ", parameters);
            return true;
        }

        public override string GetUsageSyntax(string command, List<string> parameters) {
            return command;
        }

        public override string GetUsageDescription(string command, List<string> parameters) {
            return $"Resets your or someone elses cooldowns";
        }

        public override bool CanExecuteSubCommand(string uniqueId, string command, List<string> parameters) {
            return false;
        }

        public override void HandleCommand(NotifyTextMessageEvent evt, string command, List<string> parameters, Action<string> messageCallback) {
            string targetUid = evt.InvokerUniqueId;
            string targetName = evt.InvokerName;

            if (TargetNamePart != null) {
                Client target = Parent.Client.GetClientByNamePart(TargetNamePart);
                targetUid = target.UniqueId;
                targetName = target.Nickname;
            }

            CooldownManager.ResetCooldowns(targetUid);
            messageCallback.Invoke(ColorCoder.Success($"Reset all cooldowns for {ColorCoder.Username(targetName)}"));
        }
    }
}
