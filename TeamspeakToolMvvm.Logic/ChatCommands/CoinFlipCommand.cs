﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.Exceptions;
using TeamspeakToolMvvm.Logic.Groups;
using TeamspeakToolMvvm.Logic.Misc;
using TSClient.Events;

namespace TeamspeakToolMvvm.Logic.ChatCommands {
    public class CoinFlipCommand : ChatCommand {
        public override string CommandPrefix { get; set; } = "coinflip";
        public override List<string> CommandAliases { get; set; } = new List<string>() { "coin" };

        public override bool IsValidCommandSyntax(string command, List<string> parameters) {
            if (parameters.Count > 1) return false;

            if (parameters.Count == 1 && !int.TryParse(parameters[0], out int res)) {
                throw new CommandParameterInvalidFormatException() { ParameterPosition = 1, ParameterName = "amount_of_coins", ParameterType = typeof(int), ParameterValue = parameters[0] };
            }

            return true;
        }

        public override string GetUsageHelp(string command, List<string> parameters) {
            return "coinflip [amount_of_coins]";
        }

        public override bool CanExecuteSubCommand(string uniqueId, string command, List<string> parameters) {
            return false;
        }


        public override void HandleCommand(NotifyTextMessageEvent evt, string command, List<string> parameters, Action<string> messageCallback) {
            Random r = new Random();

            int rolledHeads = 0;
            int rolledTails = 0;

            if (parameters.Count == 0) {
                string result = r.NextDouble() >= 0.5 ? "Heads" : "Tails";

                messageCallback.Invoke($"{ColorCoder.Username(evt.InvokerName)} flipped a coin... it show {ColorCoder.Bold(result)}");

                if (result == "Heads") {
                    rolledHeads = 1;
                } else {
                    rolledTails = 1;
                }

            } else {
                int count = int.Parse(parameters[0]);
                if (count <= 0) {
                    messageCallback.Invoke($"Why would you want to flip that, {ColorCoder.Username(evt.InvokerName)}...");
                    return;
                } else if (count > 10000000) {
                    string inEuro = ""+count / 100;
                    messageCallback.Invoke($"Even with 1-Cent coins that would be {inEuro:0.##} EUR. I doubt you will ever have that many coins to flip, {ColorCoder.Username(evt.InvokerName)} :^)");
                    return;
                }


                for (int i = 0; i < count; i++) {
                    if ((i % 2 == 0 && r.NextDouble() >= 0.5) || (i % 2 == 1 && r.NextDouble() <= 0.5)) {
                        rolledHeads++;
                    } else {
                        rolledTails++;
                    }
                }


                messageCallback.Invoke($"{ColorCoder.Username(evt.InvokerName)} flipped {ColorCoder.Bold(""+count)} coins... they showed {ColorCoder.Bold($"Heads {rolledHeads}")} and {ColorCoder.Bold($"Tails {rolledTails}")} times");
            }

            Settings.StatisticCoinflipHeads += rolledHeads;
            Settings.StatisticCoinflipTails += rolledTails;
        }
    }
}
