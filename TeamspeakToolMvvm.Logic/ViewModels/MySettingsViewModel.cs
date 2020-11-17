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
                new TitleSetting("Tools"),
                new BooleanSetting("Admin Tools Enabled", "Enables a list of features only accessible when you have admin control of the TS server", nameof(SettingsInstance.HasAdmin)),

                new TitleSetting("No Move") { Type = TitleType.H2 },
                new BooleanSetting("Enabled", null, nameof(SettingsInstance.NoMoveEnabled)),
                new StringSetting("Protected Usernames", "Comma separated list of usernames that can't be moved", nameof(SettingsInstance.NoMoveUsername)),

                new TitleSetting("Door Channel") { Type = TitleType.H2 },
                new BooleanSetting("Enabled", "When the door channel is enabled, any client joining the door channel will be kicked from the server", nameof(SettingsInstance.DoorChannelEnabled)),
                new StringSetting("Channel Name", null, nameof(SettingsInstance.DoorChannelName)),
                new StringSetting("Kick Message", null, nameof(SettingsInstance.DoorMessage)),


                new TitleSetting("Playsounds") { Type = TitleType.H2 },
                new BooleanSetting("Enabled", "Playsounds allow users in your TS server to play sounds via your audio device", nameof(SettingsInstance.PlaysoundsEnabled)),
                new BoundedIntSetting("YouTube Max Duration", null, nameof(SettingsInstance.YoutubeMaxVideoDurationSeconds)) { Unit = "Seconds", Min = 0 },
                new BoundedIntSetting("YouTube Max File Size", null, nameof(SettingsInstance.YoutubeMaxVideoSizeMb)) { Unit = "MB", Min = 0 },


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
