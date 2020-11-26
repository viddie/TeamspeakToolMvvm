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
    public class RouletteCommand : ChatCommand {
        public override string CommandPrefix { get; set; } = "roulette";
        public override List<string> CommandAliases { get; set; } = new List<string>() { "roul" };
        public override bool HasExceptionWhiteList { get; set; } = true;

        public bool IsAll = false;
        public bool IsPercent = false;
        public uint Amount;

        public override bool CanExecuteSubCommand(string uniqueId, string command, List<string> parameters) {
            return false;
        }

        public override bool IsValidCommandSyntax(string command, List<string> parameters) {
            if (parameters.Count != 1) return false;
            if (parameters[0] == "all") {
                IsAll = true;

            } else if (parameters[0].EndsWith("%")) {
                IsPercent = true;
                parameters[0] = parameters[0].Replace("%", "");
                if (!uint.TryParse(parameters[0], out Amount)) {
                    throw new CommandParameterInvalidFormatException(1, parameters[0], "balance%", typeof(uint), GetUsageSyntax(command, parameters));
                }

            } else {
                if (!uint.TryParse(parameters[0], out Amount)) {
                    throw new CommandParameterInvalidFormatException(1, parameters[0], "points", typeof(uint), GetUsageSyntax(command, parameters));
                }
            }

            return true;
        }

        public override string GetUsageDescription(string command, List<string> parameters) {
            return "Gamble all your problems (and points) away";
        }

        public override string GetUsageSyntax(string command, List<string> parameters) {
            return $"{command} <points|balance%|all>";
        }

        public override void HandleCommand(NotifyTextMessageEvent evt, string command, List<string> parameters, Action<string> messageCallback) {
            int amountToGamble = 0;

            if (!Settings.RouletteEnabled) {
                messageCallback.Invoke(ColorCoder.Error($"The casino is currently closed, sorry {ColorCoder.Username(evt.InvokerName)}!"));
                return;
            }

            CooldownManager.ThrowIfCooldown(evt.InvokerUniqueId, "command:roulette");
            CooldownManager.SetCooldown(evt.InvokerUniqueId, "command:roulette", Settings.RouletteCooldown);

            int currentBalance = EconomyManager.GetBalanceForUser(evt.InvokerUniqueId);

            if (currentBalance >= Settings.EcoSoftBalanceLimit) {
                messageCallback.Invoke(ColorCoder.Error($"You can't gamble while being over {ColorCoder.Bold($"{Settings.EcoSoftBalanceLimit} {Settings.EcoPointUnitName}")}, {ColorCoder.Username(evt.InvokerName)}"));
                return;
            }

            if (IsAll) {
                amountToGamble = EconomyManager.GetBalanceForUser(evt.InvokerUniqueId);

            } else if (IsPercent) {
                if (Amount > 100) {
                    messageCallback.Invoke(ColorCoder.Error($"You can't gamble away more than you own, {ColorCoder.Username(evt.InvokerName)}"));
                    return;
                }

                amountToGamble = EconomyManager.GetBalanceForUser(evt.InvokerUniqueId);
                if (amountToGamble == 0) {
                    messageCallback.Invoke(ColorCoder.Error($"You don't have any cash on you, get out of my casino!"));
                    return;
                }

                amountToGamble = (int)Math.Ceiling(amountToGamble * ((double)Amount / 100));

            } else {
                int balanceAfterPay = EconomyManager.GetUserBalanceAfterPaying(evt.InvokerUniqueId, (int)Amount);
                if (balanceAfterPay < 0) {
                    messageCallback.Invoke(ColorCoder.Error($"You don't have enough to gamble away '{ColorCoder.Bold($"{Amount} {Settings.EcoPointUnitName}")}', {ColorCoder.Username(evt.InvokerName)}"));
                    return;
                }

                amountToGamble = (int)Amount;
            }


            double winChancePercent = Settings.RouletteWinChancePercent;
            double jackpotChancePercent = Settings.RouletteJackpotChancePercent;

            if (amountToGamble == Settings.EcoSoftBalanceLimit) {
                winChancePercent += 1;
            }

            winChancePercent /= 100;
            jackpotChancePercent /= 100;

            Settings.StatisticRouletteGames++;
            Random r = new Random();
            double chosenValue = r.NextDouble();

            int changeBalanceAmount;
            string message;
            string ptsUnit = Settings.EcoPointUnitName;

            if (chosenValue < winChancePercent) { //User won the roulette

                double yield = Settings.RouletteWinYieldMultiplier;
                changeBalanceAmount = (int)Math.Floor(amountToGamble * (yield - 1));

                if (chosenValue < jackpotChancePercent) {
                    Settings.StatisticRouletteJackpots++;
                    changeBalanceAmount = (int)(changeBalanceAmount * Settings.RouletteJackpotMultiplier);
                    messageCallback.Invoke(ColorCoder.Success($"\t-\t JACKPOT! Your reward has just been tripled!\t-\t"));
                }

                Settings.StatisticRoulettePointsWon += changeBalanceAmount;
                message = ColorCoder.ColorText(Color.DarkGreen, ColorCoder.Username(evt.InvokerName) + $" won {ColorCoder.Bold(changeBalanceAmount.ToString())} {ptsUnit} in roulette and now has {ColorCoder.Bold((currentBalance + changeBalanceAmount).ToString())} {ptsUnit} forsenPls");

            } else {
                changeBalanceAmount = -amountToGamble;
                Settings.StatisticRoulettePointsLost += amountToGamble;
                message = ColorCoder.Error(ColorCoder.Username(evt.InvokerName) + $" lost {ColorCoder.Bold(amountToGamble.ToString())} {ptsUnit} in roulette and now has {ColorCoder.Bold((currentBalance + changeBalanceAmount).ToString())} {ptsUnit} FeelsBadMan");
            }

            EconomyManager.ChangeBalanceForUser(evt.InvokerUniqueId, changeBalanceAmount);
            messageCallback.Invoke(message);
        }
    }
}
