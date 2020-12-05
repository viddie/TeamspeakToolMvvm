using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamspeakToolMvvm.Logic.Models {
    public partial class AoeLastMatchResponse {
        [JsonProperty("profile_id", NullValueHandling = NullValueHandling.Ignore)]
        public long? ProfileId { get; set; }

        [JsonProperty("steam_id", NullValueHandling = NullValueHandling.Ignore)]
        public string SteamId { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("country", NullValueHandling = NullValueHandling.Ignore)]
        public string Country { get; set; }

        [JsonProperty("last_match", NullValueHandling = NullValueHandling.Ignore)]
        public Match LastMatch { get; set; }
    }

    public partial class Match {
        [JsonProperty("match_id", NullValueHandling = NullValueHandling.Ignore)]
        public string MatchId { get; set; }

        [JsonProperty("lobby_id")]
        public object LobbyId { get; set; }

        [JsonProperty("match_uuid", NullValueHandling = NullValueHandling.Ignore)]
        public string MatchUuid { get; set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("num_players", NullValueHandling = NullValueHandling.Ignore)]
        public int? NumPlayers { get; set; }

        [JsonProperty("num_slots", NullValueHandling = NullValueHandling.Ignore)]
        public int? NumSlots { get; set; }

        [JsonProperty("average_rating")]
        public object AverageRating { get; set; }

        [JsonProperty("cheats", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Cheats { get; set; }

        [JsonProperty("full_tech_tree", NullValueHandling = NullValueHandling.Ignore)]
        public bool? FullTechTree { get; set; }

        [JsonProperty("ending_age", NullValueHandling = NullValueHandling.Ignore)]
        public int? EndingAge { get; set; }

        [JsonProperty("expansion")]
        public object Expansion { get; set; }

        [JsonProperty("game_type", NullValueHandling = NullValueHandling.Ignore)]
        public int? GameType { get; set; }

        [JsonProperty("has_custom_content")]
        public object HasCustomContent { get; set; }

        [JsonProperty("has_password", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasPassword { get; set; }

        [JsonProperty("lock_speed", NullValueHandling = NullValueHandling.Ignore)]
        public bool? LockSpeed { get; set; }

        [JsonProperty("lock_teams", NullValueHandling = NullValueHandling.Ignore)]
        public bool? LockTeams { get; set; }

        [JsonProperty("map_size", NullValueHandling = NullValueHandling.Ignore)]
        public int? MapSize { get; set; }

        [JsonProperty("map_type", NullValueHandling = NullValueHandling.Ignore)]
        public int? MapType { get; set; }

        [JsonProperty("pop", NullValueHandling = NullValueHandling.Ignore)]
        public int? Pop { get; set; }

        [JsonProperty("ranked", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Ranked { get; set; }

        [JsonProperty("leaderboard_id", NullValueHandling = NullValueHandling.Ignore)]
        public int? LeaderboardId { get; set; }

        [JsonProperty("rating_type", NullValueHandling = NullValueHandling.Ignore)]
        public int? RatingType { get; set; }

        [JsonProperty("resources", NullValueHandling = NullValueHandling.Ignore)]
        public int? Resources { get; set; }

        [JsonProperty("rms")]
        public object Rms { get; set; }

        [JsonProperty("scenario")]
        public object Scenario { get; set; }

        [JsonProperty("server", NullValueHandling = NullValueHandling.Ignore)]
        public string Server { get; set; }

        [JsonProperty("shared_exploration", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SharedExploration { get; set; }

        [JsonProperty("speed", NullValueHandling = NullValueHandling.Ignore)]
        public int? Speed { get; set; }

        [JsonProperty("starting_age", NullValueHandling = NullValueHandling.Ignore)]
        public int? StartingAge { get; set; }

        [JsonProperty("team_together", NullValueHandling = NullValueHandling.Ignore)]
        public bool? TeamTogether { get; set; }

        [JsonProperty("team_positions", NullValueHandling = NullValueHandling.Ignore)]
        public bool? TeamPositions { get; set; }

        [JsonProperty("treaty_length", NullValueHandling = NullValueHandling.Ignore)]
        public long? TreatyLength { get; set; }

        [JsonProperty("turbo", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Turbo { get; set; }

        [JsonProperty("victory", NullValueHandling = NullValueHandling.Ignore)]
        public long? Victory { get; set; }

        [JsonProperty("victory_time", NullValueHandling = NullValueHandling.Ignore)]
        public long? VictoryTime { get; set; }

        [JsonProperty("visibility", NullValueHandling = NullValueHandling.Ignore)]
        public int? Visibility { get; set; }

        [JsonProperty("opened", NullValueHandling = NullValueHandling.Ignore)]
        public long? Opened { get; set; }

        [JsonProperty("started", NullValueHandling = NullValueHandling.Ignore)]
        public long? Started { get; set; }

        [JsonProperty("finished", NullValueHandling = NullValueHandling.Ignore)]
        public long? Finished { get; set; }

        [JsonProperty("players", NullValueHandling = NullValueHandling.Ignore)]
        public List<AoePlayer> Players { get; set; }
    }
}
