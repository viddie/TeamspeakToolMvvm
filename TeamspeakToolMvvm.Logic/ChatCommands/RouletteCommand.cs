using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.Config;
using TeamspeakToolMvvm.Logic.Economy;
using TeamspeakToolMvvm.Logic.Exceptions;
using TeamspeakToolMvvm.Logic.Misc;
using TeamspeakToolMvvm.Logic.Models;
using TSClient.Events;
using TSClient.Models;

namespace TeamspeakToolMvvm.Logic.ChatCommands {
    // !roul <points>
    // !roul <balance%>
    // !roul <all>
    // !roul stats
    // !roul stats all
    public class RouletteCommand : ChatCommand {
        public override string CommandPrefix { get; set; } = "roulette";
        public override List<string> CommandAliases { get; set; } = new List<string>() { "roul" };
        public override bool HasExceptionWhiteList { get; set; } = true;

        public bool RequestedStats = false;
        public string RequestedStatsName = null;
        public bool IsAll = false;
        public bool IsPercent = false;
        public uint Amount;

        public override bool CanExecuteSubCommand(string uniqueId, string command, List<string> parameters) {
            return false;
        }

        public override bool IsValidCommandSyntax(string command, List<string> parameters) {
            if (parameters.Count == 0 || (parameters[0] != "stats" && parameters.Count != 1) || (parameters[0] == "stats" && parameters.Count > 2)) return false;
            if (parameters[0] == "all") {
                IsAll = true;

            } else if (parameters[0].EndsWith("%")) {
                IsPercent = true;
                parameters[0] = parameters[0].Replace("%", "");
                if (!uint.TryParse(parameters[0], out Amount)) {
                    throw new CommandParameterInvalidFormatException(1, parameters[0], "balance%", typeof(uint), GetUsageSyntax(command, parameters));
                }

            } else if (parameters[0] == "stats") {
                RequestedStats = true;
                if (parameters.Count > 1) {
                    RequestedStatsName = parameters[1];
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
            StatisticSettings stats = Parent.StatSettings;

            if (RequestedStats) {
                RouletteStatistics displayStats;
                string toPrint;

                if (RequestedStatsName != null && RequestedStatsName == "all") {
                    displayStats = stats.Roulette;
                    toPrint = "Global roulette stats:";
                } else {
                    string userUid = evt.InvokerUniqueId;
                    string userName = evt.InvokerName;

                    if (RequestedStatsName != null) {
                        Client user = Parent.Client.GetClientByNamePart(RequestedStatsName);
                        userUid = user.UniqueId;
                        userName = user.Nickname;
                    }

                    CheckHasStatisticsEntry(userUid);
                    displayStats = stats.RouletteIndividual[userUid];
                    toPrint = $"Roulette stats for {ColorCoder.Username(userName)}:";
                }

                int won = displayStats.GamesWon;
                int lost = displayStats.GamesLost;
                double ratioPercent = (double)won * 100 / (lost + won);
                string ratioPercentStr;
                if (double.IsNaN(ratioPercent)) {
                    ratioPercentStr = "-%";
                } else {
                    ratioPercentStr = ColorCoder.ColorPivotPercent(ratioPercent, 50);
                }
                toPrint += $"\n\tGames: {won + lost} ({ColorCoder.SuccessDim($"{won}W")}, {ColorCoder.ErrorBright($"{lost}L")} | {ratioPercentStr})";

                int balanceWon = displayStats.PointsWon;
                int balanceLost = displayStats.PointsLost;
                int balanceDiff = balanceWon - balanceLost;
                string balanceDiffStr = ColorCoder.ColorPivot(balanceDiff, 0);
                toPrint += $"\n\tPoints: {balanceDiffStr} ({ColorCoder.SuccessDim($"+{balanceWon} won")}, {ColorCoder.ErrorBright($"-{balanceLost} lost")})";

                int jackpots = displayStats.Jackpots;
                int jackpotPoints = displayStats.JackpotsPointsWon;
                toPrint += $"\n\tJackpots: {jackpots} ({ColorCoder.SuccessDim($"+{jackpotPoints} bonus")})";

                double rolls = displayStats.Rolls;
                string rollsStr = (won + lost) == 0 ? "-" : $"{rolls / (won + lost):0.####}";
                toPrint += $"\n\tAvg. Roll: {rollsStr}";

                messageCallback.Invoke(toPrint);
                return;
            }


            if (!Settings.RouletteEnabled) {
                messageCallback.Invoke(ColorCoder.ErrorBright($"The casino is currently closed, sorry {ColorCoder.Username(evt.InvokerName)}!"));
                return;
            }

            CooldownManager.ThrowIfCooldown(evt.InvokerUniqueId, "command:roulette");

            int currentBalance = EconomyManager.GetBalanceForUser(evt.InvokerUniqueId);

            if (currentBalance > Settings.EcoSoftBalanceLimit) {
                messageCallback.Invoke(ColorCoder.ErrorBright($"You can't gamble while being over {ColorCoder.Bold($"{Settings.EcoSoftBalanceLimit} {Settings.EcoPointUnitName}")}, {ColorCoder.Username(evt.InvokerName)}"));
                return;
            }

            if (IsAll) {
                amountToGamble = EconomyManager.GetBalanceForUser(evt.InvokerUniqueId);

            } else if (IsPercent) {
                if (Amount > 100) {
                    messageCallback.Invoke(ColorCoder.ErrorBright($"You can't gamble away more than you own, {ColorCoder.Username(evt.InvokerName)}"));
                    return;
                }

                amountToGamble = EconomyManager.GetBalanceForUser(evt.InvokerUniqueId);
                amountToGamble = (int)Math.Ceiling(amountToGamble * ((double)Amount / 100));

            } else {
                int balanceAfterPay = EconomyManager.GetUserBalanceAfterPaying(evt.InvokerUniqueId, (int)Amount);
                if (balanceAfterPay < 0) {
                    messageCallback.Invoke(ColorCoder.ErrorBright($"You don't have enough to gamble away '{ColorCoder.Bold($"{Amount} {Settings.EcoPointUnitName}")}', {ColorCoder.Username(evt.InvokerName)}"));
                    return;
                }

                amountToGamble = (int)Amount;
            }

            if (amountToGamble <= 0) {
                messageCallback.Invoke(ColorCoder.ErrorBright($"You don't have any cash on you, get out of my casino!"));
                return;
            }

            CooldownManager.SetCooldown(evt.InvokerUniqueId, "command:roulette", Settings.RouletteCooldown);


            double winChancePercent = Settings.RouletteWinChancePercent;
            double jackpotChancePercent = Settings.RouletteJackpotChancePercent;

            if (amountToGamble == Settings.EcoSoftBalanceLimit) {
                winChancePercent += 1;
            }

            winChancePercent /= 100;
            jackpotChancePercent /= 100;

            Random r = new Random();
            double chosenValue = r.NextDouble();

            string resultLogMsg = $"({chosenValue:0.#####} | win {winChancePercent:0.#####} | jackpot {jackpotChancePercent:0.#####})";
            Parent.LastRouletteResult = resultLogMsg;
            Parent.LogMessage($"Roulette result: {resultLogMsg}");

            int changeBalanceAmount;
            string message;
            string ptsUnit = Settings.EcoPointUnitName;

            CheckHasStatisticsEntry(evt.InvokerUniqueId);
            RouletteStatistics indivStats = stats.RouletteIndividual[evt.InvokerUniqueId];
            RouletteStatistics allStats = stats.Roulette;

            allStats.Rolls += chosenValue;
            indivStats.Rolls += chosenValue;

            if (chosenValue < winChancePercent) { //User won the roulette

                double yield = Settings.RouletteWinYieldMultiplier;
                changeBalanceAmount = (int)Math.Floor(amountToGamble * (yield - 1));

                if (chosenValue < jackpotChancePercent) {
                    allStats.Jackpots++;
                    int jackpotPoints = (int)(changeBalanceAmount * Settings.RouletteJackpotMultiplier);

                    allStats.JackpotsPointsWon += jackpotPoints - changeBalanceAmount;
                    indivStats.JackpotsPointsWon += jackpotPoints - changeBalanceAmount;

                    changeBalanceAmount = jackpotPoints;
                    messageCallback.Invoke(ColorCoder.Success($"\t-\t JACKPOT! Your reward has just been tripled!\t-\t"));
                }

                allStats.GamesWon++;
                indivStats.GamesWon++;
                allStats.PointsWon += changeBalanceAmount;
                indivStats.PointsWon += changeBalanceAmount;
                message = ColorCoder.ColorText(Color.DarkGreen, ColorCoder.Username(evt.InvokerName) + $" won {ColorCoder.Bold(changeBalanceAmount.ToString())} {ptsUnit} in roulette and now has {ColorCoder.Bold((currentBalance + changeBalanceAmount).ToString())} {ptsUnit} forsenPls ({chosenValue:0.####})");

            } else {
                changeBalanceAmount = -amountToGamble;
                allStats.GamesLost++;
                indivStats.GamesLost++;
                allStats.PointsLost += amountToGamble;
                indivStats.PointsLost += amountToGamble;
                message = ColorCoder.ErrorBright(ColorCoder.Username(evt.InvokerName) + $" lost {ColorCoder.Bold(amountToGamble.ToString())} {ptsUnit} in roulette and now has {ColorCoder.Bold((currentBalance + changeBalanceAmount).ToString())} {ptsUnit} FeelsBadMan ({chosenValue:0.####})");
            }

            stats.DelayedSave();

            EconomyManager.ChangeBalanceForUser(evt.InvokerUniqueId, changeBalanceAmount);
            messageCallback.Invoke(message);
        }

        public void CheckHasStatisticsEntry(string uid) {
            StatisticSettings stats = Parent.StatSettings;
            if (stats.RouletteIndividual.ContainsKey(uid)) return;

            stats.RouletteIndividual.Add(uid, new RouletteStatistics());
        }
    }
}
