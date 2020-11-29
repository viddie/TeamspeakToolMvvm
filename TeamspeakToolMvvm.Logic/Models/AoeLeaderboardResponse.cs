using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamspeakToolMvvm.Logic.Models {

    public partial class AoeLeaderboardResponse {
        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("leaderboard_id")]
        public long LeaderboardId { get; set; }

        [JsonProperty("start")]
        public long Start { get; set; }

        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("leaderboard")]
        public List<AoePlayer> Leaderboard { get; set; }
    }

    public partial class AoePlayer {
        [JsonProperty("profile_id")]
        public long ProfileId { get; set; }

        [JsonProperty("rank")]
        public int Rank { get; set; }

        [JsonProperty("rating")]
        public int Rating { get; set; }

        [JsonProperty("steam_id")]
        public string SteamId { get; set; }

        [JsonProperty("icon")]
        public object Icon { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("clan")]
        public object Clan { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("previous_rating")]
        public int? PreviousRating { get; set; }

        [JsonProperty("highest_rating")]
        public int HighestRating { get; set; }

        [JsonProperty("streak")]
        public int Streak { get; set; }

        [JsonProperty("lowest_streak")]
        public int LowestStreak { get; set; }

        [JsonProperty("highest_streak")]
        public int HighestStreak { get; set; }

        [JsonProperty("games")]
        public int Games { get; set; }

        [JsonProperty("wins")]
        public int Wins { get; set; }

        [JsonProperty("losses")]
        public int Losses { get; set; }

        [JsonProperty("drops")]
        public int Drops { get; set; }

        [JsonProperty("last_match")]
        public long LastMatch { get; set; }

        [JsonProperty("last_match_time")]
        public long LastMatchTime { get; set; }
    }
}
