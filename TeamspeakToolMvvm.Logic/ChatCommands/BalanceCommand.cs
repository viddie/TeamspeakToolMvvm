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


        public string TargetName;

        public bool IsSendingBalance = false;
        public string SendParam;
        public uint SendAmount;

        public bool SetAmountIsRelative = false;
        public int SetAmount;


        public override bool IsValidCommandSyntax(string command, List<string> parameters) {
            if (parameters.Count > 0 && parameters[0] == "send") {
                if (parameters.Count < 3) return false;
                IsSendingBalance = true;
                parameters = parameters.Skip(1).ToList();
            }
            
            if (parameters.Count > 0) {
                TargetName = parameters[0];
            }

            if (parameters.Count > 1 && IsSendingBalance) {
                if (parameters[1] == "all" || parameters[1] == "excess") {
                    SendParam = parameters[1];

                } else if (!uint.TryParse(parameters[1], out SendAmount)) {
                    throw new CommandParameterInvalidFormatException(3, parameters[1], "amount", typeof(uint), GetUsageSyntax(command, parameters));
                }
                parameters = parameters.Skip(1).ToList();
            }
            


            if (command == "setbalance") {
                if (parameters.Count != 2) return false;

                if (parameters[1].StartsWith("~")) {
                    SetAmountIsRelative = true;
                    parameters[1] = parameters[1].Substring(1);
                }

                if (!int.TryParse(parameters[1], out SetAmount)) {
                    throw new CommandParameterInvalidFormatException(2, parameters[1], "amount", typeof(int), GetUsageSyntax(command, parameters));
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

            if (IsSendingBalance) {
                if (SendParam == "all") {
                    SendAmount = (uint)Math.Max(0, EconomyManager.GetBalanceForUser(evt.InvokerUniqueId));
                } else if(SendParam == "excess") {
                    int tempB = EconomyManager.GetBalanceForUser(evt.InvokerUniqueId);
                    if (tempB > Settings.EcoSoftBalanceLimit) {
                        SendAmount = (uint)(tempB - Settings.EcoSoftBalanceLimit);
                    } else {
                        SendAmount = 0;
                    }
                }

                if (SendAmount == 0) {
                    messageCallback.Invoke(ColorCoder.ErrorBright($"Why would you want to send {ColorCoder.Currency(0)}? What are you, stupid {ColorCoder.Username(evt.InvokerName)}"));
                    return;

                } else if (EconomyManager.GetUserBalanceAfterPaying(evt.InvokerUniqueId, (int)SendAmount) < 0) {
                    messageCallback.Invoke(ColorCoder.ErrorBright($"You can't afford to send that much cash, {ColorCoder.Username(evt.InvokerName)}"));
                    return;

                } else if (EconomyManager.GetUserBalanceAfterPaying(target.UniqueId, -1 * (int)SendAmount) > Settings.EcoSoftBalanceLimit) {
                    messageCallback.Invoke(ColorCoder.ErrorBright($"You can't send that much cash because {ColorCoder.Username(target.Nickname)} would have more than '{ColorCoder.Currency(Settings.EcoSoftBalanceLimit)}'"));
                    return;
                }

                EconomyManager.ChangeBalanceForUser(evt.InvokerUniqueId, (-1) * (int)SendAmount);
                EconomyManager.ChangeBalanceForUser(target.UniqueId, (int)SendAmount);

                messageCallback.Invoke(ColorCoder.SuccessDim($"Sent '{ColorCoder.Currency(SendAmount)}' to {ColorCoder.Username(target.Nickname)}"));

            } else if (command == "setbalance") {
                int setAmount = SetAmount;
                if (SetAmountIsRelative) {
                    setAmount += EconomyManager.GetBalanceForUser(target.UniqueId);
                }

                EconomyManager.SetBalanceForUser(target.UniqueId, setAmount);
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
