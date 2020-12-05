using AdvancedSettings.Logic.Settings;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.ChatCommands;
using TeamspeakToolMvvm.Logic.Groups;
using TeamspeakToolMvvm.Logic.Models;

namespace TeamspeakToolMvvm.Logic.Config {
    public class StatisticSettings : Settings {
        #region Lazy Pattern
        private static readonly Lazy<StatisticSettings> lazy = new Lazy<StatisticSettings>(() => new StatisticSettings());

        public static StatisticSettings Instance { get { return lazy.Value; } }
        #endregion



        #region Statistics
        public int MovesDenied { get; set; } = 0;

        public int DoorUsed { get; set; } = 0;

        public int YouTubeLinksFetched { get; set; } = 0;

        public int CoinflipHeads { get; set; } = 0;
        public int CoinflipTails { get; set; } = 0;

        public RouletteStatistics Roulette { get; set; } = new RouletteStatistics();
        public Dictionary<string, RouletteStatistics> RouletteIndividual { get; set; } = new Dictionary<string, RouletteStatistics>();
        #endregion
    }
}
