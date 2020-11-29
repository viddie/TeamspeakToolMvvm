using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamspeakToolMvvm.Logic.Models {
    public partial class AoeMatchResponse {
        [JsonProperty("match_id")]
        public string MatchId { get; set; }

        [JsonProperty("lobby_id")]
        public object LobbyId { get; set; }

        [JsonProperty("match_uuid")]
        public Guid MatchUuid { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("num_players")]
        public long NumPlayers { get; set; }

        [JsonProperty("num_slots")]
        public long NumSlots { get; set; }

        [JsonProperty("average_rating")]
        public object AverageRating { get; set; }

        [JsonProperty("cheats")]
        public bool Cheats { get; set; }

        [JsonProperty("full_tech_tree")]
        public bool FullTechTree { get; set; }

        [JsonProperty("ending_age")]
        public long EndingAge { get; set; }

        [JsonProperty("expansion")]
        public object Expansion { get; set; }

        [JsonProperty("game_type")]
        public long GameType { get; set; }

        [JsonProperty("has_custom_content")]
        public object HasCustomContent { get; set; }

        [JsonProperty("has_password")]
        public bool HasPassword { get; set; }

        [JsonProperty("lock_speed")]
        public bool LockSpeed { get; set; }

        [JsonProperty("lock_teams")]
        public bool LockTeams { get; set; }

        [JsonProperty("map_size")]
        public long MapSize { get; set; }

        [JsonProperty("map_type")]
        public long MapType { get; set; }

        [JsonProperty("pop")]
        public long Pop { get; set; }

        [JsonProperty("ranked")]
        public bool Ranked { get; set; }

        [JsonProperty("leaderboard_id")]
        public long LeaderboardId { get; set; }

        [JsonProperty("rating_type")]
        public long RatingType { get; set; }

        [JsonProperty("resources")]
        public long Resources { get; set; }

        [JsonProperty("rms")]
        public object Rms { get; set; }

        [JsonProperty("scenario")]
        public object Scenario { get; set; }

        [JsonProperty("server")]
        public string Server { get; set; }

        [JsonProperty("shared_exploration")]
        public bool SharedExploration { get; set; }

        [JsonProperty("speed")]
        public long Speed { get; set; }

        [JsonProperty("starting_age")]
        public long StartingAge { get; set; }

        [JsonProperty("team_together")]
        public bool TeamTogether { get; set; }

        [JsonProperty("team_positions")]
        public bool TeamPositions { get; set; }

        [JsonProperty("treaty_length")]
        public long TreatyLength { get; set; }

        [JsonProperty("turbo")]
        public bool Turbo { get; set; }

        [JsonProperty("victory")]
        public long Victory { get; set; }

        [JsonProperty("victory_time")]
        public long VictoryTime { get; set; }

        [JsonProperty("visibility")]
        public long Visibility { get; set; }

        [JsonProperty("opened")]
        public long Opened { get; set; }

        [JsonProperty("started")]
        public long Started { get; set; }

        [JsonProperty("finished")]
        public long Finished { get; set; }

        [JsonProperty("players")]
        public List<AoePlayer> Players { get; set; }
    }
}
