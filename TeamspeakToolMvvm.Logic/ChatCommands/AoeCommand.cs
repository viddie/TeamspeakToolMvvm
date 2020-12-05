using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.Config;
using TeamspeakToolMvvm.Logic.Misc;
using TeamspeakToolMvvm.Logic.Models;
using TSClient.Events;

namespace TeamspeakToolMvvm.Logic.ChatCommands {

    // !aoe match [-leaderboard] [name]
    // !aoe stats [-leaderboard] [name]

    public class AoeCommand : ChatCommand {

        public static object ScraperLock = new object();
        public static bool IsScraperBusy = false;

        public static Dictionary<string, int> Leaderboards = new Dictionary<string, int>() {
            ["unranked"] = 0,
            ["dm"] = 1,
            ["tdm"] = 2,
            ["1v1"] = 3,
            ["tg"] = 4,
        };

        public override string CommandPrefix { get; set; } = "aoe";
        public override List<string> CommandAliases { get; set; } = new List<string>();
        public override bool HasExceptionWhiteList { get; set; } = true;


        public string Action;
        public string SelectedLeaderboard = "tg";
        public string SelectedName;


        public override bool IsValidCommandSyntax(string command, List<string> parameters) {
            if (parameters.Count == 0) return false;
            if (parameters[0] != "match" && parameters[0] != "stats") return false;

            Action = parameters[0];

            if (parameters.Count > 1) {
                if (parameters[1].StartsWith("-")) {
                    string leaderboard = parameters[1].Substring(1);
                    if (!Leaderboards.ContainsKey(leaderboard)) return false;
                    SelectedLeaderboard = leaderboard;

                    parameters = parameters.Skip(1).ToList();
                }

            }

            if (parameters.Count > 1) {
                SelectedName = string.Join(" ", parameters.Skip(1));
            }

            return true;
        }

        public override string GetUsageDescription(string command, List<string> parameters) {
            return "Displays some AoE 2 DE statistics";
        }

        public override string GetUsageSyntax(string command, List<string> parameters) {
            return command;
        }

        public override bool CanExecuteSubCommand(string uniqueId, string command, List<string> parameters) {
            return false;
        }

        public override void HandleCommand(NotifyTextMessageEvent evt, string command, List<string> parameters, Action<string> messageCallback) {
            SelectedName = SelectedName ?? evt.InvokerName;
            int leaderboardId = Leaderboards[SelectedLeaderboard];

            lock (ScraperLock) {
                IsScraperBusy = true;

                if (Action == "stats") {
                    HandleStatsRequest(SelectedName, leaderboardId, messageCallback);
                } else if (Action == "match") {
                    HandleLastMatchRequest(SelectedName, leaderboardId, messageCallback);
                }

                IsScraperBusy = false;
            }
        }


        public static void HandleStatsRequest(string nameSearch, int leaderboardId, Action<string> messageCallback) {
            AoeLeaderboardResponse response = GetLeaderboardResponse(leaderboardId, nameSearch, 2);

            if (response.Count == 0) {
                messageCallback.Invoke($"There were no players found on leaderboard '{leaderboardId}' with the name '{nameSearch}'");
                return;
            } else if (response.Count > 1) {
                messageCallback.Invoke($"There were multiple players found on leaderboard '{leaderboardId}' with the name '{nameSearch}'");
                return;
            }

            AoePlayer player = response.Leaderboard[0];
            string clanAddition = player.Clan == null ? "" : $"[{player.Clan}]";
            string leaderboardName = Leaderboards.First(kv => kv.Value == leaderboardId).Key;
            int ratingDiff = player.Rating.Value - player.PreviousRating.Value;
            string ratingDiffStr = ratingDiff < 0 ? ColorCoder.ErrorBright(ratingDiff) : ratingDiff > 0 ? ColorCoder.SuccessDim(ratingDiff) : $"{ratingDiff}";
            double ratioPercent = (double)player.Wins.Value * 100 / player.Games.Value;
            string ratioPercentStr = $"{ratioPercent:0.##}%";
            ratioPercentStr = ratioPercent < 50 ? ColorCoder.ErrorBright(ratioPercentStr) : ratioPercent > 50 ? ColorCoder.SuccessDim(ratioPercentStr) : $"{ratioPercentStr}";


            string toPrint = $"{ColorCoder.Bold(leaderboardName.ToUpper())} stats for {ColorCoder.Bold(player.Name+clanAddition)}:";
            toPrint += $"\n\tRating: {ColorCoder.Bold(player.Rating.Value)} ({ratingDiffStr}) (#{player.Rank})";
            toPrint += $"\n\tPeak Rating: {player.HighestRating}";
            toPrint += $"\n\tStreak: {player.Streak} | Lowest: {player.LowestStreak} | Highest: {player.HighestStreak}";
            toPrint += $"\n\tGames: {player.Games} ({ColorCoder.SuccessDim(player.Wins.Value+"W")} - {ColorCoder.ErrorBright(player.Losses.Value+"L")} | {ratioPercentStr})";
            toPrint += $"\n\tCountry: {player.Country}";

            messageCallback.Invoke(toPrint);
        }



        public static void HandleLastMatchRequest(string nameSearch, int leaderboardId, Action<string> messageCallback) {
            AoeLeaderboardResponse response = GetLeaderboardResponse(leaderboardId, nameSearch, 2);
            AoePlayer player;

            if (response.Count == 0) {
                messageCallback.Invoke($"There were no players found on leaderboard '{leaderboardId}' with the name '{nameSearch}'");
                return;
            } else if (response.Count > 1) {
                messageCallback.Invoke($"There were multiple players found on leaderboard '{leaderboardId}' with the name '{nameSearch}', taking the highest rated player...");
                player = response.Leaderboard[0];
            } else {
                player = response.Leaderboard[0];
            }


            AoeLastMatchResponse lastMatchResponse = GetLastMatchResponse(player.SteamId);
            Match lastMatch = lastMatchResponse.LastMatch;
            Dictionary<int, List<List<AoePlayer>>> matchTeams = new Dictionary<int, List<List<AoePlayer>>>();
            foreach (AoePlayer matchPlayer in lastMatch.Players) {
                if (!matchTeams.ContainsKey(matchPlayer.Team.Value)) {
                    matchTeams.Add(matchPlayer.Team.Value, new List<List<AoePlayer>>());
                }
                List<List<AoePlayer>> team = matchTeams[matchPlayer.Team.Value];
                List<AoePlayer> playerInfo = new List<AoePlayer>();
                playerInfo.Add(matchPlayer);

                if (matchPlayer.SteamId != player.SteamId) {
                    AoeLeaderboardResponse lb = GetLeaderboardResponse(leaderboardId, matchPlayer.SteamId, 2, true);
                    if (lb.Leaderboard.Count == 0)
                        playerInfo.Add(null);
                    else
                        playerInfo.Add(lb.Leaderboard[0]);
                } else {
                    playerInfo.Add(player);
                }
                team.Add(playerInfo);
            }

            string toPrint = "";
            toPrint += lastMatch.Finished.HasValue ? "Last Match (" + (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(lastMatch.Finished.Value).ToLocalTime()).ToString("dd.MM.yy HH:mm") + "):" : "Current Match:";
            toPrint += lastMatch.Ranked.HasValue && lastMatch.Ranked.Value ? " Ranked" : " Unranked";
            string mapName = GetAoeString("map_type", lastMatch.MapType.Value);
            toPrint += $" on {mapName}";

            bool firstTeam = true;
            foreach (int teamPos in matchTeams.Keys) {
                List<List<AoePlayer>> team = matchTeams[teamPos];

                string winStatusString;
                if (!team[0][0].Won.HasValue)
                    winStatusString = "◯";
                else if (team[0][0].Won.Value)
                    winStatusString = "👑";
                else
                    winStatusString = "☠";

                if (firstTeam)
                    firstTeam = false;
                else
                    toPrint += "\n\t\tvs.";

                foreach (List<AoePlayer> playerInfo in team) {
                    string playerName = playerInfo[0].Name;
                    string civ = GetAoeString("civ", playerInfo[0].Civ.Value);

                    if (playerName.ToLower().Contains(nameSearch.ToLower())) {
                        playerName = ColorCoder.Bold(playerName);
                    }

                    if (playerInfo[1] == null) //Player is not yet ranked in this leaderboard
                    {
                        string line = $"\n{winStatusString} [--] {playerName} (<placements>) on {civ}";
                        toPrint += line;
                    } else {
                        string country = playerInfo[1].Country;
                        string rating = playerInfo[1].Rating.HasValue ? $"{playerInfo[1].Rating.Value}" : "<placement>";
                        int games = playerInfo[1].Games.Value;
                        int wins = playerInfo[1].Wins.Value;

                        double ratioPercent = Math.Round((double)wins * 100 / games);
                        string ratioPercentStr = $"{ratioPercent}%";
                        ratioPercentStr = ratioPercent < 50 ? ColorCoder.ErrorBright(ratioPercentStr) : ratioPercent > 50 ? ColorCoder.SuccessDim(ratioPercentStr) : $"{ratioPercentStr}";

                        string line = $"\n{winStatusString} [{country}] {playerName} ({rating}, {games} G, {ratioPercentStr}) on {civ}";
                        toPrint += line;
                    }
                }
            }

            messageCallback.Invoke(toPrint);
        }






        public static AoeLeaderboardResponse GetLeaderboardResponse(int leaderId, string name, int count, bool isSteamId = false) {
            string profileUrl;

            if (isSteamId) {
                profileUrl = $"https://aoe2.net/api/leaderboard?game=aoe2de&leaderboard_id={leaderId}&steam_id={name}&count={count}";
            } else {
                profileUrl = $"https://aoe2.net/api/leaderboard?game=aoe2de&leaderboard_id={leaderId}&search={name}&count={count}";
            }
            return JsonConvert.DeserializeObject<AoeLeaderboardResponse>(GetWebsiteAsString(profileUrl));
        }

        public static AoeLastMatchResponse GetLastMatchResponse(string steamId) {
            string url = $"https://aoe2.net/api/player/lastmatch?game=aoe2de&steam_id={steamId}";
            return JsonConvert.DeserializeObject<AoeLastMatchResponse>(GetWebsiteAsString(url));
        }

        public static string GetAoeString(string field, int id) {
            AoeSettings aoeSettings = AoeSettings.Instance;
            if (aoeSettings.AoeStrings == null) {
                aoeSettings.AoeStrings = new Dictionary<string, Dictionary<int, string>>();

                string url = $"https://aoe2.net/api/strings?game=aoe2de&language={aoeSettings.AoeLanguage}";
                JObject response = JObject.Parse(GetWebsiteAsString(url));
                foreach (string name in response.Properties().Select(p => p.Name).ToList()) {
                    if (name != "language") {
                        JArray fieldObj = (JArray)response.GetValue(name);
                        aoeSettings.AoeStrings.Add(name, new Dictionary<int, string>());
                        foreach (JToken entryToken in fieldObj.Children()) {
                            JObject entryObj = (JObject)entryToken;
                            string entryId = entryObj.GetValue("id").ToObject<string>();
                            string entryValue = entryObj.GetValue("string").ToObject<string>();
                            aoeSettings.AoeStrings[name].Add(int.Parse(entryId), entryValue);
                        }
                    }
                }

                aoeSettings.DelayedSave();
            }

            return aoeSettings.AoeStrings[field][id];
        }

        public static string GetWebsiteAsString(string getUrl) {
            HttpClient client = new HttpClient();
            return client.GetStringAsync(getUrl).Result;
        }
    }
}
