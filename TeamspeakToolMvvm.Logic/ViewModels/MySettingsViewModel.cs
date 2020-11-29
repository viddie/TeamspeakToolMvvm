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

        public MySettingsViewModel(MySettings instance) : base(instance) {
            SettingsInstance = instance;
        }

        protected override List<Setting> CreateSettings()
        {
            return new List<Setting>() {
                new TitleSetting("TS Query Settings"),
                new PasswordStringSetting("Auth Key", null, nameof(SettingsInstance.ClientAuthKey)) { HasShowPasswordButton = true },
                new StringSetting("Host", null, nameof(SettingsInstance.ClientHost)),
                new BoundedIntSetting("Port", null, nameof(SettingsInstance.ClientPort)) { Min = 1 },

                #region Tools
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
                #endregion

                #region ChatCommands
                new HorizontalRuleSetting(),
                new TitleSetting("Chat Commands"),

                new TitleSetting("Roulette") { Type = TitleType.H2 },
                new BooleanSetting("Enabled", "Allows the users to gamble for points", nameof(SettingsInstance.RouletteEnabled)),
                new BoundedDoubleSetting("Win Chance", null, nameof(SettingsInstance.RouletteWinChancePercent)) { Unit = "%" },
                new BoundedDoubleSetting("Win Yield", "2 = you can double your points or lose all, 3 = triple, ...", nameof(SettingsInstance.RouletteWinYieldMultiplier)) { Unit = "x" },
                new BoundedDoubleSetting("Jackpot Chance", null, nameof(SettingsInstance.RouletteJackpotChancePercent)) { Unit = "%" },
                new BoundedDoubleSetting("Jackpot Multiplier", "The gained points through the win are multiplied by this factor if it was a jackpot", nameof(SettingsInstance.RouletteJackpotMultiplier)) { Unit = "x" },

                new TitleSetting("Playsounds") { Type = TitleType.H2 },
                new BooleanSetting("Enabled", "Playsounds allow users in your TS server to play sounds via your audio device", nameof(SettingsInstance.PlaysoundsEnabled)),
                new BoundedIntSetting("YouTube Max Duration", null, nameof(SettingsInstance.PlaysoundsYoutubeMaxVideoDurationSeconds)) { Unit = "Seconds", Min = 0 },
                new BoundedDoubleSetting("YouTube Max File Size", null, nameof(SettingsInstance.PlaysoundsYoutubeMaxVideoSizeMb)) { Unit = "MB", Min = 0 },

                new TitleSetting("Daily") { Type = TitleType.H2 },
                new BooleanSetting("Enabled", "Daily rewards allow your users to gain {Daily Reward} points each day for remembering this command!", nameof(SettingsInstance.DailyEnabled)),
                new BoundedIntSetting("Daily Reward", null, nameof(SettingsInstance.DailyReward)) { Unit = SettingsInstance.EcoPointUnitName },
                #endregion


                new HorizontalRuleSetting(),
                new TitleSetting("Economy") { },
                new StringSetting("Eco Unit Name", null, nameof(SettingsInstance.EcoPointUnitName)),
                new BooleanSetting("Ticks Enabled", "When enabled, all {Tick Intervall} seconds an eco unit is granted to everyone connected to the server", nameof(SettingsInstance.EcoTicksEnabled)),
                new BoundedIntSetting("Tick Intervall", null, nameof(SettingsInstance.EcoTickTimeSeconds)) { Unit = "Seconds" },
                new BoundedIntSetting("Balance Soft Limit", "Soft limit means one can't exceed this limit through ticks. Any other means of acquiring balance except roulette will be unavailable while at or above this limit.", nameof(SettingsInstance.EcoSoftBalanceLimit)) { Unit = SettingsInstance.EcoPointUnitName },
                new BoundedIntSetting("Balance Hard Limit", "This limit can't be overstepped in any case.", nameof(SettingsInstance.EcoHardBalanceLimit)) { Unit = SettingsInstance.EcoPointUnitName },


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
