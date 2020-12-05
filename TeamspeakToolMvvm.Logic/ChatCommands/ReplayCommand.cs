using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.Config;
using TeamspeakToolMvvm.Logic.Exceptions;
using TeamspeakToolMvvm.Logic.Misc;
using TeamspeakToolMvvm.Logic.Models;
using TSClient.Events;

namespace TeamspeakToolMvvm.Logic.ChatCommands {

    // !replay          - Fetches random TG replay between 800 and 2400 elo
    // !replay <tg|1v1>   - 
    // !replay <tg|1v1> <lel|normal|ok|good|pro|all>
    // !replay <tg|1v1> <fromElo> <toElo>

    public class ReplayCommand : ChatCommand {

        public static string ModeTgName = "tg";
        public static string Mode1v1Name = "1v1";

        public static Dictionary<string, int> LeaderboardIds = new Dictionary<string, int>() {
            [Mode1v1Name] = 3,
            [ModeTgName] = 4,
        };

        public static Dictionary<string, Dictionary<string, Tuple<int, int>>> EloRanges = new Dictionary<string, Dictionary<string, Tuple<int, int>>>() {
            [ModeTgName] = new Dictionary<string, Tuple<int, int>>() {
                ["lel"] = Tuple.Create(800, 1200),
                ["normal"] = Tuple.Create(1201, 1700),
                ["ok"] = Tuple.Create(1701, 2100),
                ["good"] = Tuple.Create(2101, 2400),
                ["pro"] = Tuple.Create(2402, -1),
                ["all"] = Tuple.Create(-1, -1),
            },
            [Mode1v1Name] = new Dictionary<string, Tuple<int, int>>() {
                ["lel"] = Tuple.Create(600, 1000),
                ["normal"] = Tuple.Create(1001, 1300),
                ["ok"] = Tuple.Create(1301, 1600),
                ["good"] = Tuple.Create(1601, 1900),
                ["pro"] = Tuple.Create(1901, -1),
                ["all"] = Tuple.Create(-1, -1),
            },
        };


        public static bool IsBusy { get; set; } = false;
        public static object LockObject { get; set; } = new object();



        public override string CommandPrefix { get; set; } = "replay";
        public override List<string> CommandAliases { get; set; } = new List<string>() { };
        public override bool HasExceptionWhiteList { get; set; } = true;

        public string SelectedLeaderboard = null;
        public string SelectedLevel = null;
        public uint InputFromElo = uint.MaxValue;
        public uint InputToElo = uint.MaxValue;

        public AoeSettings Elos { get; set; }

        public override bool IsValidCommandSyntax(string command, List<string> parameters) {
            if (parameters.Count == 0) return true;

            if (parameters.Count == 1 && EloRanges.ContainsKey(parameters[0])) {
                SelectedLeaderboard = parameters[0];
                return true;
            }

            if (parameters.Count == 2 && EloRanges[ModeTgName].ContainsKey(parameters[1])) {
                SelectedLeaderboard = parameters[0];
                SelectedLevel = parameters[1];
                return true;
            }
            if (parameters.Count == 3) {
                SelectedLeaderboard = parameters[0];
                if (!uint.TryParse(parameters[1], out InputFromElo)) {
                    throw new CommandParameterInvalidFormatException(2, parameters[1], "fromElo", typeof(uint), GetUsageSyntax(command, parameters));
                }

                if (!uint.TryParse(parameters[2], out InputToElo)) {
                    throw new CommandParameterInvalidFormatException(3, parameters[2], "toElo", typeof(uint), GetUsageSyntax(command, parameters));
                }
                return true;
            }

            return false;
        }

        public override string GetUsageSyntax(string command, List<string> parameters) {
            string joinedLevels = string.Join("|", EloRanges[ModeTgName].Keys.ToList());
            return $"{command} <tg|1v1> <{joinedLevels}>";
        }

        public override string GetUsageDescription(string command, List<string> parameters) {
            if (parameters.Count == 0) {
                return "Gets the link to a recent replay of a random player";
            }

            if (parameters.Count == 1 && EloRanges.ContainsKey(parameters[0])) {
                string leaderboard = parameters[0];
                return $"Gets the link to a recent replay of a random {leaderboard.ToUpper()} player";
            }

            if (parameters.Count == 2 && EloRanges[ModeTgName].ContainsKey(parameters[1])) {
                string leaderboard = parameters[0];
                string level = parameters[1];
                Tuple<int, int> range = EloRanges[leaderboard][level];
                int lower = range.Item1 == -1 ? 0 : range.Item1;
                int upper = range.Item2 == -1 ? 5000 : range.Item2;

                return $"Gets the link to a recent replay of a random {leaderboard.ToUpper()} player at the elo range {lower}-{upper}";
            }

            if (parameters.Count == 3) {
                string leaderboard = parameters[0];
                uint fromElo, toElo;
                if (!uint.TryParse(parameters[1], out fromElo)) {
                    throw new CommandParameterInvalidFormatException(2, parameters[1], "fromElo", typeof(uint), GetUsageSyntax(command, parameters));
                }

                if (!uint.TryParse(parameters[2], out toElo)) {
                    throw new CommandParameterInvalidFormatException(3, parameters[2], "toElo", typeof(uint), GetUsageSyntax(command, parameters));
                }

                return $"Gets the link to a recent replay of a random {leaderboard.ToUpper()} player at the elo range {fromElo}-{toElo}";
            }

            return "Gets the link to a recent replay of a random player";
        }

        public override bool CanExecuteSubCommand(string uniqueId, string command, List<string> parameters) {
            return false;
        }

        public override void HandleCommand(NotifyTextMessageEvent evt, string command, List<string> parameters, Action<string> messageCallback) {
            lock (LockObject) {
                if (IsBusy) {
                    messageCallback.Invoke(ColorCoder.ErrorBright("The fetcher is busy, try again when the current request has completed..."));
                    return;
                }
                IsBusy = true;
            }

            Elos = Parent.EloSettings;
            if (Elos.LastUpdated + Elos.UpdateFrequency < DateTime.Now) {
                messageCallback.Invoke($"Leaderboards were outdated (older than 1 month), started refreshing cache. This could take a few minutes...");

                Parent.LogMessage("[*] Starting fetch AoE2DE leaderboards...");
                foreach (string mode in LeaderboardIds.Keys) {
                    FetchClientList(mode, messageCallback);
                    Parent.LogMessage($"[*] {mode} done");
                }
                Parent.LogMessage("[*] AoE2DE leaderboards fetch done");

                //Implicitly saves the elos to the hard drive
                Elos.LastUpdated = DateTime.Now;
            }


            //HttpClient client = new HttpClient();
            SelectedLeaderboard = SelectedLeaderboard ?? ModeTgName;
            int fromElo, toElo;

            if (!string.IsNullOrEmpty(SelectedLevel)) {
                (fromElo, toElo) = EloRanges[SelectedLeaderboard][SelectedLevel];
                if (fromElo == -1) fromElo = Elos.AllPlayersElos[SelectedLeaderboard].Last().Item1;
                if (toElo == -1) toElo = Elos.AllPlayersElos[SelectedLeaderboard].First().Item1;

            } else if (InputFromElo != uint.MaxValue) {
                if (InputFromElo > InputToElo) {
                    uint tempElo = InputFromElo;
                    InputFromElo = InputToElo;
                    InputToElo = tempElo;
                }

                fromElo = Math.Max((int)InputFromElo, Elos.AllPlayersElos[SelectedLeaderboard].Last().Item1);
                toElo = Math.Min((int)InputToElo, Elos.AllPlayersElos[SelectedLeaderboard].First().Item1);

            } else {
                fromElo = EloRanges[SelectedLeaderboard]["lel"].Item1;
                toElo = EloRanges[SelectedLeaderboard]["good"].Item2;
            }

            Random r = new Random();
            int offset = r.Next(1, 10);

            int lowerRankIndex = int.MaxValue, upperRankIndex = int.MinValue; //indices inclusive bounds
            List<Tuple<int, string>> leaderboard = Elos.AllPlayersElos[SelectedLeaderboard];
            for (int i = 0; i < leaderboard.Count; i++) {
                Tuple<int, string> position = leaderboard[i];
                if (upperRankIndex == int.MinValue && position.Item1 <= toElo) {
                    upperRankIndex = i;
                }
                if (lowerRankIndex == int.MaxValue && position.Item1 < fromElo) {
                    lowerRankIndex = i-1;
                }
            }


            bool hasFoundMatch = false;
            string downloadLink = "";

            while (!hasFoundMatch) {
                int selectedIndex = r.Next(upperRankIndex, lowerRankIndex + 1);
                string steamId = Elos.AllPlayersElos[SelectedLeaderboard][selectedIndex].Item2;

                string fetchUrl = $"https://aoe2.net/api/player/matches?game=aoe2de&steam_id={steamId}&count=10&start=1";

                HttpClient client = new HttpClient();
                HttpResponseMessage response = client.GetAsync(fetchUrl).Result;
                string content = response.Content.ReadAsStringAsync().Result;

                JArray matches = JArray.Parse(content);
                JObject foundMatch = null;

                foreach (JToken matchToken in matches.Children()) {
                    JObject match = (JObject)matchToken;
                    bool isRanked = match.GetValue("ranked").ToObject<bool>();
                    int playerCount = match.GetValue("num_players").ToObject<int>();

                    if (!isRanked) {
                        Parent.LogMessage("Game was not ranked, skipping...");
                        continue;
                    }

                    if ((SelectedLeaderboard == Mode1v1Name && playerCount == 2) || (SelectedLeaderboard == ModeTgName && playerCount > 2)) {
                        foundMatch = match;
                    } else {
                        Parent.LogMessage($"Game was not of leaderboard '{SelectedLeaderboard}', skipping...");
                    }
                }

                if (foundMatch == null) {
                    Parent.LogMessage($"Randomly selected player did not play a ranked '{SelectedLeaderboard}' game in the last 10 games");
                    continue;
                }

                string matchId = foundMatch.GetValue("match_id").ToObject<string>();
                JArray players = foundMatch.GetValue("players").ToObject<JArray>();
                JObject firstPlayer = (JObject)players[0];

                long profileId = firstPlayer.GetValue("profile_id").ToObject<long>();
                downloadLink = $"https://aoe.ms/replay/?gameId={matchId}&profileId={profileId}";
                hasFoundMatch = true;
            }

            messageCallback.Invoke($"Here is your {ColorCoder.Bold(SelectedLeaderboard.ToUpper())} replay of elo range ({fromElo}-{toElo}): [url={downloadLink}]Download[/url]");

            lock (LockObject) {
                IsBusy = false;
            }
        }

        public void FetchClientList(string mode, Action<string> messageCallback) {
            int leaderboardId = LeaderboardIds[mode];
            string baseUrl = "https://aoe2.net/api/leaderboard?game=aoe2de&leaderboard_id={leaderboard}&start={start}&count={increment}";
            HttpClient client = new HttpClient();
            int startPosition = 1;
            int increment = 1000;
            Elos.AllPlayersElos[mode] = new List<Tuple<int, string>>();

            bool isDone = false;
            while (!isDone) {
                string fetchUrl = baseUrl.Replace("{leaderboard}", leaderboardId.ToString()).Replace("{start}", startPosition.ToString()).Replace("{increment}", increment.ToString());
                HttpResponseMessage response = client.GetAsync(fetchUrl).Result;
                string content = response.Content.ReadAsStringAsync().Result;
                AoeLeaderboardResponse parsed = JsonConvert.DeserializeObject<AoeLeaderboardResponse>(content);

                foreach (AoePlayer aoePlayer in parsed.Leaderboard) {
                    Elos.AllPlayersElos[mode].Add(Tuple.Create(aoePlayer.Rating.Value, aoePlayer.SteamId));
                }

                Parent.LogMessage($"[*] <{mode}>: Loaded rank #{startPosition} - #{startPosition + parsed.Count}");
                if (parsed.Count != increment) {
                    messageCallback.Invoke($"<{mode}>: Loaded rank #1 - #{startPosition + parsed.Count}");
                }


                if (parsed.Count != increment) {
                    isDone = true;
                } else {
                    startPosition += increment;
                }
            }
        }
    }
}
