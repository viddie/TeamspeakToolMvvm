using AdvancedSettings.Logic.Settings;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.Groups;

namespace TeamspeakToolMvvm.Logic.Config {
    public class AoeSettings : Settings {
        #region Lazy Pattern
        private static readonly Lazy<AoeSettings> lazy = new Lazy<AoeSettings>(() => new AoeSettings());

        public static AoeSettings Instance { get { return lazy.Value; } }
        #endregion

        public DateTime LastUpdated { get; set; }
        public TimeSpan UpdateFrequency { get; set; } = TimeSpan.FromDays(30);
        public Dictionary<string, List<Tuple<int, string>>> AllPlayersElos { get; set; } = new Dictionary<string, List<Tuple<int, string>>>();

        public string AoeLanguage = "en";
        public List<string> AoeLanguages = new List<string>() { "en", "de", "el", "es", "es-MX", "fr", "hi", "it", "ja", "ko", "ms", "nl", "pt", "ru", "tr", "vi", "zh", "zh-TW" };
        public Dictionary<string, Dictionary<int, string>> AoeStrings = null;
    }
}
