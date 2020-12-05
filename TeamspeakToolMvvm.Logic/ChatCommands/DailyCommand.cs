using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.Economy;
using TeamspeakToolMvvm.Logic.Exceptions;
using TeamspeakToolMvvm.Logic.Misc;
using TSClient.Events;

namespace TeamspeakToolMvvm.Logic.ChatCommands {
    // !roul <points>
    // !roul <balance%>
    // !roul <all>
    public class DailyCommand : ChatCommand {
        public override string CommandPrefix { get; set; } = "daily";
        public override List<string> CommandAliases { get; set; } = new List<string>() { };
        public override bool HasExceptionWhiteList { get; set; } = true;

        public override bool CanExecuteSubCommand(string uniqueId, string command, List<string> parameters) {
            return false;
        }

        public override bool IsValidCommandSyntax(string command, List<string> parameters) {
            return true;
        }

        public override string GetUsageDescription(string command, List<string> parameters) {
            return "Get your daily batch of points directly into your bank account";
        }

        public override string GetUsageSyntax(string command, List<string> parameters) {
            return $"{command}";
        }

        public override void HandleCommand(NotifyTextMessageEvent evt, string command, List<string> parameters, Action<string> messageCallback) {
            CooldownManager.ThrowIfCooldown(evt.InvokerUniqueId, "command:daily");

            int currentBalance = EconomyManager.GetBalanceForUser(evt.InvokerUniqueId);
            if (currentBalance + Settings.DailyReward > Settings.EcoSoftBalanceLimit) {
                messageCallback.Invoke(ColorCoder.ErrorBright($"You would have more than {ColorCoder.Currency(Settings.EcoSoftBalanceLimit, Settings.EcoPointUnitName)}, {ColorCoder.Username(evt.InvokerName)}. Your balance: {ColorCoder.Currency(currentBalance, Settings.EcoPointUnitName)} |  Daily reward: {ColorCoder.Currency(Settings.DailyReward, Settings.EcoPointUnitName)}"));
                return;
            }


            DateTime cooldownDue = DateTime.Today.AddDays(1).AddHours(7);
            DateTime todayCheck = DateTime.Today.AddHours(7);
            if (todayCheck > DateTime.Now) {
                cooldownDue = todayCheck;
            }

            CooldownManager.SetCooldown(evt.InvokerUniqueId, "command:daily", cooldownDue);

            EconomyManager.ChangeBalanceForUser(evt.InvokerUniqueId, Settings.DailyReward);
            messageCallback.Invoke(ColorCoder.SuccessDim($"{ColorCoder.Currency(Settings.DailyReward, Settings.EcoPointUnitName)} have been added to your balance as daily reward, {ColorCoder.Username(evt.InvokerName)}"));
        }
    }
}
