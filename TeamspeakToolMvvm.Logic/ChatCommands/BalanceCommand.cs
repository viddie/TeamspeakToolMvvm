using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.Economy;
using TeamspeakToolMvvm.Logic.Exceptions;
using TeamspeakToolMvvm.Logic.Groups;
using TeamspeakToolMvvm.Logic.Misc;
using TSClient.Events;
using TSClient.Exceptions;
using TSClient.Models;

namespace TeamspeakToolMvvm.Logic.ChatCommands {
    public class BalanceCommand : ChatCommand {
        public override string CommandPrefix { get; set; } = "balance";
        public override List<string> CommandAliases { get; set; } = new List<string>() { "b", "setbalance" };
        public override bool HasExceptionWhiteList { get; set; } = false;


        public string TargetName = null;
        public uint SetAmount;


        public override bool IsValidCommandSyntax(string command, List<string> parameters) {
            if (parameters.Count > 0) {
                TargetName = parameters[0];
            }

            if (command == "setbalance") {
                if (parameters.Count != 2) return false;

                if (!uint.TryParse(parameters[1], out SetAmount)) {
                    throw new CommandParameterInvalidFormatException(2, parameters[1], "amount", typeof(uint), GetUsageSyntax(command, parameters));
                }

            } else if (parameters.Count > 1) {
                return false;
            }

            return true;
        }

        public override string GetUsageSyntax(string command, List<string> parameters) {
            if (command == "setbalance") {
                return $"setbalance <name> <value>";
            }

            return $"{command} [name]";
        }

        public override string GetUsageDescription(string command, List<string> parameters) {
            if (command == "setbalance") {
                return $"Sets a users balance";
            }

            return $"Checks your own or another users' balance";
        }

        public override bool CanExecuteSubCommand(string uniqueId, string command, List<string> parameters) {
            if (command == "setbalance" && !AccessManager.UserHasAccessToSubCommand(uniqueId, "command:balance_set")) return false;
            return true;
        }

        public override void HandleCommand(NotifyTextMessageEvent evt, string command, List<string> parameters, Action<string> messageCallback) {

            Client target = null;

            if (TargetName != null) {
                target = Parent.Client.GetClientByNamePart(TargetName);
            }

            if (target == null) {
                target = Parent.Client.GetClientById(evt.InvokerId);
            }

            if (command == "setbalance") {
                EconomyManager.SetBalanceForUser(target.UniqueId, (int)SetAmount);
                int setTo = EconomyManager.GetBalanceForUser(target.UniqueId);
                messageCallback.Invoke(ColorCoder.Success($"Set balance for {ColorCoder.Username(target.Nickname)} to {ColorCoder.Bold($"{setTo} {Settings.EcoPointUnitName}")}"));

            } else {
                int max = Settings.EcoSoftBalanceLimit;
                int balance = EconomyManager.GetBalanceForUser(target.UniqueId);
                messageCallback.Invoke($"Balance of {ColorCoder.Username(target.Nickname)}: {ColorCoder.Bold(balance.ToString())} / {ColorCoder.Bold($"{max} {Settings.EcoPointUnitName}")}");
            }
        }
    }
}
