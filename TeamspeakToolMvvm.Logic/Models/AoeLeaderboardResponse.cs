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
        [JsonProperty("profile_id", NullValueHandling = NullValueHandling.Ignore)]
        public int? ProfileId { get; set; }

        [JsonProperty("rank", NullValueHandling = NullValueHandling.Ignore)]
        public int? Rank { get; set; }

        [JsonProperty("rating", NullValueHandling = NullValueHandling.Ignore)]
        public int? Rating { get; set; }

        [JsonProperty("steam_id", NullValueHandling = NullValueHandling.Ignore)]
        public string SteamId { get; set; }

        [JsonProperty("icon")]
        public object Icon { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("clan")]
        public string Clan { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("previous_rating", NullValueHandling = NullValueHandling.Ignore)]
        public int? PreviousRating { get; set; }

        [JsonProperty("highest_rating", NullValueHandling = NullValueHandling.Ignore)]
        public int? HighestRating { get; set; }

        [JsonProperty("streak")]
        public int? Streak { get; set; }

        [JsonProperty("lowest_streak", NullValueHandling = NullValueHandling.Ignore)]
        public int? LowestStreak { get; set; }

        [JsonProperty("highest_streak", NullValueHandling = NullValueHandling.Ignore)]
        public int? HighestStreak { get; set; }

        [JsonProperty("games")]
        public int? Games { get; set; }

        [JsonProperty("wins")]
        public int? Wins { get; set; }

        [JsonProperty("losses", NullValueHandling = NullValueHandling.Ignore)]
        public int? Losses { get; set; }

        [JsonProperty("drops")]
        public int? Drops { get; set; }

        [JsonProperty("last_match", NullValueHandling = NullValueHandling.Ignore)]
        public long? LastMatch { get; set; }

        [JsonProperty("last_match_time", NullValueHandling = NullValueHandling.Ignore)]
        public long? LastMatchTime { get; set; }

        [JsonProperty("slot", NullValueHandling = NullValueHandling.Ignore)]
        public int? Slot { get; set; }

        [JsonProperty("slot_type", NullValueHandling = NullValueHandling.Ignore)]
        public int? SlotType { get; set; }

        [JsonProperty("rating_change")]
        public object RatingChange { get; set; }

        [JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)]
        public int? Color { get; set; }

        [JsonProperty("team", NullValueHandling = NullValueHandling.Ignore)]
        public int? Team { get; set; }

        [JsonProperty("civ", NullValueHandling = NullValueHandling.Ignore)]
        public int? Civ { get; set; }

        [JsonProperty("won", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Won { get; set; }
    }
}
