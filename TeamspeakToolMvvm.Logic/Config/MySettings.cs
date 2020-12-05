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

namespace TeamspeakToolMvvm.Logic.Config
{
    public class MySettings : Settings
    {
        #region Lazy Pattern
        private static readonly Lazy<MySettings> lazy = new Lazy<MySettings>(() => new MySettings());

        public static MySettings Instance { get { return lazy.Value; } }
        #endregion



        #region Window Properties
        public int WindowWidth { get; set; } = 830;
        public int WindowHeight { get; set; } = 600;
        public int WindowLeft { get; set; } = 200;
        public int WindowTop { get; set; } = 200;
        public bool WindowIsMaximized { get; set; } = false;
        #endregion

        #region TS Client Query Connection
        public string ClientAuthKey { get; set; } = "<your-api-key-here>";
        public string ClientHost { get; set; } = "localhost";
        public int ClientPort { get; set; } = 25639;

        public bool HasAdmin { get; set; } = false;

        public Dictionary<string, string> LastSeenUsernames { get; set; } = new Dictionary<string, string>();
        #endregion


        #region Modules

        #region No Move
        public bool NoMoveEnabled { get; set; } = false;
        public string NoMoveUsername { get; set; } = "";
        #endregion


        #region Door Channel
        public bool DoorChannelEnabled { get; set; } = false; 
        public string DoorChannelName { get; set; } = "Door";
        public string DoorMessage { get; set; } = "Left the house.";
        #endregion


        #region Immediate Chat Reaction
        public bool ImmediateYoutubeEnabled { get; set; } = true;
        public bool ImmediateGeneralEnabled { get; set; } = true;
        public bool ImmediateGeneralDescriptionEnabled { get; set; } = false;
        public bool ImmediateGeneralKeywordsEnabled { get; set; } = false;
        #endregion


        #region Chat Commands
        public bool ChatCommandsEnabled { get; set; } = true;
        public string ChatCommandPrefix { get; set; } = "!";
        public List<string> AdminUniqueIds { get; set; } = new List<string>();
        public List<string> BlockedUniqueIds { get; set; } = new List<string>();
        public int DefaultRateLimitPerMinute { get; set; } = 100;
        public int DefaultRateLimitPerMinuteAdmin { get; set; } = 999;
        public Dictionary<string, int> CommandRateLimitPerMinute { get; set; } = new Dictionary<string, int>() {
            ["chat_command:somecommandtest"] = 20,
        };
        #endregion


        #region Playsounds
        public bool PlaysoundsEnabled { get; set; } = true;
        public string PlaysoundsSoundDevice { get; set; } = null;
        public int PlaysoundsYoutubeMaxVideoDurationSeconds { get; set; } = 60 * 60 * 1;
        public double PlaysoundsYoutubeMaxVideoSizeMb { get; set; } = 50.0;
        public int PlaysoundsSoundsPerPage { get; set; } = 15;
        public Dictionary<string, Dictionary<string, double>> PlaysoundsModifiers { get; set; } = new Dictionary<string, Dictionary<string, double>>() {
            ["nc"] = new Dictionary<string, double>() {
                [PlaysoundsCommand.ModifierNameVolume] = 1.0,
                [PlaysoundsCommand.ModifierNameSpeed] = 1.25,
                [PlaysoundsCommand.ModifierNamePitch] = 1.25,
            },
            ["demon"] = new Dictionary<string, double>() {
                [PlaysoundsCommand.ModifierNameVolume] = 1.2,
                [PlaysoundsCommand.ModifierNameSpeed] = 0.7,
                [PlaysoundsCommand.ModifierNamePitch] = 0.7,
            },
            ["earrape"] = new Dictionary<string, double>() {
                [PlaysoundsCommand.ModifierNameVolume] = 10.0,
                [PlaysoundsCommand.ModifierNameSpeed] = 1.0,
                [PlaysoundsCommand.ModifierNamePitch] = 1.0,
            },
        };
        public List<Playsound> PlaysoundsSavedSounds { get; set; } = new List<Playsound>();
        public double PlaysoundsDurationCooldownMultiplier { get; set; } = 3;
        #endregion


        #region Access Manager
        public Dictionary<string, List<string>> UserGroups { get; set; } = new Dictionary<string, List<string>>();
        #endregion


        #region Economy Manager
        public string EcoPointUnitName { get; set; } = "Pts.";
        public int EcoTickGain { get; set; } = 1;
        public bool EcoTicksEnabled { get; set; } = true;
        public DateTime EcoLastTick { get; set; }
        public int EcoTickTimeSeconds { get; set; } = 60 * 45;
        public int EcoSoftBalanceLimit { get; set; } = 100;
        public int EcoHardBalanceLimit { get; set; } = int.MaxValue;
        public Dictionary<string, int> UserBalances { get; set; } = new Dictionary<string, int>();
        #endregion


        #region Cooldown Manager
        public Dictionary<string, Dictionary<string, DateTime>> CooldownEarliestPossibleCalls { get; set; } = new Dictionary<string, Dictionary<string, DateTime>>();
        #endregion


        #region Dynamic Commands
        public Dictionary<string, string> DynamicCommands { get; set; } = new Dictionary<string, string>();
        #endregion


        #region Roulette
        public bool RouletteEnabled { get; set; } = true;
        public TimeSpan RouletteCooldown { get; set; } = TimeSpan.FromMinutes(5);
        public double RouletteWinChancePercent { get; set; } = 50;
        public double RouletteWinYieldMultiplier { get; set; } = 2;
        public double RouletteJackpotChancePercent { get; set; } = 0.1;
        public double RouletteJackpotMultiplier { get; set; } = 3;
        #endregion


        #region Daily
        public bool DailyEnabled { get; set; } = true;
        public int DailyReward { get; set; } = 9;
        #endregion


        #endregion
    }
}
