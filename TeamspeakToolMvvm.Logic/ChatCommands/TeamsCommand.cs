using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.Exceptions;
using TeamspeakToolMvvm.Logic.Misc;
using TSClient.Events;

namespace TeamspeakToolMvvm.Logic.ChatCommands {

    /// <summary>
    /// Usage:
    /// !teams [x]
    /// Puts all unmuted clients in your channel randomly in x teams (default 2)
    /// 
    /// !teams [x] [client1] [client2]...
    /// Puts all listed names randomly in x teams (default 2)
    /// 
    /// !teams channel [x]
    /// Puts all unmuted clients in your channel randomly in x teams (default 2)
    /// 
    /// !teams server [x]
    /// Puts all unmuted clients in your server randomly in x teams (default 2)
    /// 
    /// </summary>
    public class TeamsCommand : ChatCommand {
        public override string CommandPrefix { get; set; } = "teams";
        public override List<string> CommandAliases { get; set; } = new List<string>() { "team" };
        public override bool HasExceptionWhiteList { get; set; } = true;


        public uint TeamCount = 2;
        public int StartParameter = 0;


        public override bool IsValidCommandSyntax(string command, List<string> parameters) {
            if (parameters.Count == 0) return true;
            if (parameters.Count == 1 && parameters[0] == "channel") return true;
            if (parameters.Count == 2 && parameters[0] == "channel") {
                if (!uint.TryParse(parameters[1], out TeamCount) || TeamCount == 0) {
                    throw new CommandParameterInvalidFormatException(2, parameters[1], "teams", typeof(uint), GetUsageSyntax(command, parameters));
                }
                return true;
            }
            if (parameters.Count >  2 && parameters[0] == "channel") return false;

            if (parameters.Count == 1 && parameters[0] == "server") return true;
            if (parameters.Count == 2 && parameters[0] == "server") {
                if (!uint.TryParse(parameters[1], out TeamCount) || TeamCount == 0) {
                    throw new CommandParameterInvalidFormatException(2, parameters[1], "teams", typeof(uint), GetUsageSyntax(command, parameters));
                }
                return true;
            }
            if (parameters.Count >  2 && parameters[0] == "server") return false;

            if (parameters.Count >= 1 && uint.TryParse(parameters[0], out uint tempCount)) {
                TeamCount = tempCount;
                parameters.RemoveAt(0);
            }

            return true;
        }

        public override string GetUsageSyntax(string command, List<string> parameters) {
            if (parameters.Count >= 1 && parameters[0] == "channel") return $"{command} channel [teams]";
            if (parameters.Count >= 1 && parameters[0] == "server") return $"{command} server [teams]";
            return $"{command} [teams] [name1] [name2]...";
        }

        public override string GetUsageDescription(string command, List<string> parameters) {
            return $"Quickly group users into teams";
        }

        public override bool CanExecuteSubCommand(string uniqueId, string command, List<string> parameters) {
            return false;
        }

        public override void HandleCommand(NotifyTextMessageEvent evt, string command, List<string> parameters, Action<string> messageCallback) {
            List<string> namesToSplit = new List<string>();


            if ((parameters.Count == 1 || parameters.Count == 2) && parameters[0] == "channel") {
                namesToSplit.AddRange(Parent.Client.GetClientsInChannel(Parent.MyChannelId).Select(client => client.Nickname));

            } else if ((parameters.Count == 1 || parameters.Count == 2) && parameters[0] == "server") {
                namesToSplit.AddRange(Parent.Client.GetClientList().Select(client => client.Nickname));

            } else if (parameters.Count > 0) {
                namesToSplit.AddRange(parameters);

            } else {
                namesToSplit.AddRange(Parent.Client.GetUnmutedClients(Parent.Client.GetClientsInChannel(Parent.MyChannelId)).Select(client => client.Nickname));
            }

            if (namesToSplit.Count == 0) {
                messageCallback.Invoke("Can't really make teams without players...");
                return;
            }
            if (namesToSplit.Count == 1) {
                messageCallback.Invoke("Can't really make teams with only one player...");
                return;
            }

            List<List<string>> teams = new List<List<string>>();
            for (int i = 0; i < TeamCount; i++) {
                teams.Add(new List<string>());
            }

            Random r = new Random();
            uint teamCounter = 0;
            while (namesToSplit.Count > 0) {
                int index = r.Next(namesToSplit.Count);
                teams[(int)teamCounter].Add(ColorCoder.Bold($"'{namesToSplit[index]}'"));
                teamCounter = (teamCounter + 1) % TeamCount;
                namesToSplit.RemoveAt(index);
            }

            teams = teams.OrderBy(o => r.Next()).ToList();

            string toPrint = "Here are the teams:";
            for (int i = 0; i < teams.Count; i++) {
                List<string> team = teams[i];
                string joined = string.Join(", ", team);
                toPrint += $"\n\tTeam {i+1}: {joined}";
            }

            messageCallback.Invoke(toPrint);
        }
    }
}
