using AdvancedSettings.Logic.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdvancedSettings.Logic.Settings;
using TeamspeakToolMvvm.Logic.Config;
using GalaSoft.MvvmLight.Command;
using System.Threading;

namespace TeamspeakToolMvvm.Logic.ViewModels
{
    public class MySettingsViewModel : SettingsViewModel
    {
        public MySettings SettingsInstance { get; set; }

        #region Settings
        #endregion

        public MySettingsViewModel(MySettings instance) : base(instance) { }

        protected override List<Setting> CreateSettings()
        {
            
            return new List<Setting>() {
                new TitleSetting("TS Query Settings"),
                new PasswordStringSetting("Auth Key", null, nameof(SettingsInstance.ClientAuthKey)) { HasShowPasswordButton = true },
                new StringSetting("Host", null, nameof(SettingsInstance.ClientHost)),
                new BoundedIntSetting("Port", null, nameof(SettingsInstance.ClientPort)) { Min = 1 },


                new HorizontalRuleSetting(),
                new TitleSetting("Window Settings"),
                new BoundedIntSetting("Width", null, nameof(SettingsInstance.WindowWidth)) { Unit = "Pixels" },
                new BoundedIntSetting("Height", null, nameof(SettingsInstance.WindowHeight)) { Unit = "Pixels" },
                new BoundedIntSetting("Left", null, nameof(SettingsInstance.WindowLeft)) { Unit = "Pixels" },
                new BoundedIntSetting("Top", null, nameof(SettingsInstance.WindowTop)) { Unit = "Pixels" },
                new BooleanSetting("Maximiert", null, nameof(SettingsInstance.WindowIsMaximized)),
            };
        }
    }
}
