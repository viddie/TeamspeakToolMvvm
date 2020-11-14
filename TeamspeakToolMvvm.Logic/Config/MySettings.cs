using AdvancedSettings.Logic.Settings;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamspeakToolMvvm.Logic.Config
{
    public class MySettings : Settings
    {
        #region Lazy Pattern
        private static readonly Lazy<MySettings> lazy = new Lazy<MySettings>(() => new MySettings());

        public static MySettings Instance { get { return lazy.Value; } }
        #endregion



        #region Window Properties
        public int WindowWidth { get; set; } = 530;
        public int WindowHeight { get; set; } = 350;
        public int WindowLeft { get; set; } = 200;
        public int WindowTop { get; set; } = 200;
        public bool WindowIsMaximized { get; set; } = false;
        #endregion

        public string ClientAuthKey { get; set; } = "<your-api-key-here>";
        public string ClientHost { get; set; } = "localhost";
        public int ClientPort { get; set; } = 25639;

    }
}
