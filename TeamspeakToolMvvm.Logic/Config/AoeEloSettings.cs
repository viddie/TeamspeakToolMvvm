using AdvancedSettings.Logic.Settings;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.Groups;

namespace TeamspeakToolMvvm.Logic.Config {
    public class AoeEloSettings : Settings {
        #region Lazy Pattern
        private static readonly Lazy<AoeEloSettings> lazy = new Lazy<AoeEloSettings>(() => new AoeEloSettings());

        public static AoeEloSettings Instance { get { return lazy.Value; } }
        #endregion

        public DateTime LastUpdated { get; set; }
        public TimeSpan UpdateFrequency { get; set; } = TimeSpan.FromDays(30);
        public Dictionary<string, List<Tuple<int, string>>> AllPlayersElos { get; set; } = new Dictionary<string, List<Tuple<int, string>>>();
    }
}
