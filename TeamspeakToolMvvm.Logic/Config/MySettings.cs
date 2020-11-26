﻿using AdvancedSettings.Logic.Settings;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.Groups;

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


        #region Statistics
        public int StatisticMovesDenied { get; set; } = 0;

        public int StatisticDoorUsed { get; set; } = 0;

        public int StatisticYouTubeLinksFetched { get; set; } = 0;

        public int StatisticCoinflipHeads { get; set; } = 0;
        public int StatisticCoinflipTails { get; set; } = 0;

        public int StatisticRoulettePointsWon { get; set; } = 0;
        public int StatisticRoulettePointsLost { get; set; } = 0;
        public int StatisticRouletteJackpots { get; set; } = 0;
        public int StatisticRouletteGames { get; set; } = 0;
        #endregion


        #region Playsounds
        public bool PlaysoundsEnabled { get; set; } = true;
        public int YoutubeMaxVideoDurationSeconds { get; set; } = 60 * 60 * 1;
        public int YoutubeMaxVideoSizeMb { get; set; } = 50;
        #endregion


        #region Access Manager
        public Dictionary<string, List<string>> UserGroups { get; set; } = new Dictionary<string, List<string>>();
        #endregion


        #region Economy Manager
        public string EcoPointUnitName { get; set; } = "Pts.";
        public int EcoTickGain { get; set; } = 1;
        public bool EcoTicksEnabled { get; set; } = true;
        public DateTime LastTick { get; set; }
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
        public int DailyReward { get; set; } = 9;
        #endregion


        #endregion
    }
}
